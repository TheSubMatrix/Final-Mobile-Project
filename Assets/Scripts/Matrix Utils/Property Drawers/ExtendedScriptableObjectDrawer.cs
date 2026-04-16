#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using MatrixUtils.PropertyDrawers.Helpers;

namespace MatrixUtils.PropertyDrawers
{
    /// <summary>
    /// Extends how ScriptableObject object references are displayed in the inspector.
    /// Shows you all values under the object reference.
    /// Also provides a button to create a new ScriptableObject if the property is null.
    /// </summary>
    [CustomPropertyDrawer(typeof(ScriptableObject), true)]
    public class ExtendedScriptableObjectDrawer : PropertyDrawer
    {
        static readonly List<string> s_ignoreClassFullNames = new() { "TMPro.TMP_FontAsset" };

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Type type = GetFieldType();
            if (type == null || s_ignoreClassFullNames.Contains(type.FullName))
            {
                return new PropertyField(property);
            }
            VisualElement root = new() { style = { marginTop = 2, marginBottom = 2 } };
            VisualElement headerContainer = new()
            {
                style = 
                { 
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center
                }
            };
            ObjectField objectField = new(property.displayName)
            {
                objectType = type,
                allowSceneObjects = false,
                style = { flexGrow = 1 }
            };
            objectField.BindProperty(property);
            Button createButton = new(() => CreateScriptableObjectAsset(property, type))
            {
                text = "Create",
                style = { width = 66, marginLeft = 4 }
            };
            headerContainer.Add(objectField);
            headerContainer.Add(createButton);
            root.Add(headerContainer);
            VisualElement expandedContainer = new()
            {
                style = 
                { 
                    marginLeft = 15,
                    marginTop = 4,
                    paddingTop = 4,
                    paddingBottom = 4,
                    paddingLeft = 8,
                    paddingRight = 8,
                    backgroundColor = new Color(0, 0, 0, 0.1f),
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3
                }
            };
            root.Add(expandedContainer);
            root.TrackPropertyValue(property, _ => UpdateUI(property, createButton, expandedContainer));
            UpdateUI(property, createButton, expandedContainer);
            return root;
        }

        static void UpdateUI(SerializedProperty property, Button createButton, VisualElement expandedContainer)
        {
            bool hasValue = property.objectReferenceValue != null;
            createButton.style.display = hasValue ? DisplayStyle.None : DisplayStyle.Flex;
            if (hasValue && property.objectReferenceValue is ScriptableObject so && HasVisibleProperties(so))
            {
                if (property.isExpanded)
                {
                    expandedContainer.style.display = DisplayStyle.Flex;
                    RebuildExpandedFields(expandedContainer, so);
                }
                else
                {
                    expandedContainer.style.display = DisplayStyle.None;
                }
            }
            else
            {
                expandedContainer.style.display = DisplayStyle.None;
            }
        }

        static void RebuildExpandedFields(VisualElement container, ScriptableObject so)
        {
            container.Clear();

            SerializedObject serializedObject = new(so);
            SerializedProperty iterator = serializedObject.GetIterator();
            if (!iterator.NextVisible(true)) return;
            DrawerOptions options = new()
            {
                ExcludeDrawerType = typeof(ExtendedScriptableObjectDrawer),
                ExcludeFields = new() { "m_Script" }
            };
            do
            {
                if (iterator.name == "m_Script") continue;
                SerializedProperty propCopy = iterator.Copy();
                PropertyDrawer drawer = PropertyDrawerFactory.CreateDrawerForProperty(propCopy, options.ExcludeDrawerType);
                VisualElement customElement = drawer?.CreatePropertyGUI(propCopy);
                container.Add(customElement ?? new PropertyField(propCopy));
            }
            while (iterator.NextVisible(false));
            container.TrackSerializedObjectValue(serializedObject, _ =>
            {
                serializedObject.ApplyModifiedProperties();
            });
        }

        static bool HasVisibleProperties(ScriptableObject so)
        {
            if (!so) return false;
            SerializedObject serializedObject = new(so);
            SerializedProperty prop = serializedObject.GetIterator();
            while (prop.NextVisible(true))
            {
                if (prop.name != "m_Script")
                    return true;
            }
            return false;
        }
        static void CreateScriptableObjectAsset(SerializedProperty property, Type type)
        {
            string selectedAssetPath = "Assets";
            
            // Try to get a better default path from the target object
            if (property.serializedObject.targetObject is MonoBehaviour behaviour)
            {
                MonoScript ms = MonoScript.FromMonoBehaviour(behaviour);
                string scriptPath = AssetDatabase.GetAssetPath(ms);
                if (!string.IsNullOrEmpty(scriptPath))
                {
                    selectedAssetPath = Path.GetDirectoryName(scriptPath);
                }
            }
            string path = EditorUtility.SaveFilePanelInProject(
                "Save ScriptableObject", 
                $"{type.Name}.asset", 
                "asset", 
                "Enter a file name for the ScriptableObject.", 
                selectedAssetPath);
            if (string.IsNullOrEmpty(path)) return;
            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            EditorGUIUtility.PingObject(asset);
            property.objectReferenceValue = asset;
            property.serializedObject.ApplyModifiedProperties();
        }

        Type GetFieldType()
        {
            if (fieldInfo == null) return null;
            
            Type type = fieldInfo.FieldType;
            
            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                type = type.GetGenericArguments()[0];
            }
            
            return type;
        }

        #region IMGUI Fallback

        const int ButtonWidth = 66;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Type type = GetFieldType();
            if (type == null || s_ignoreClassFullNames.Contains(type.FullName)) return EditorGUI.GetPropertyHeight(property, label, true);
            float totalHeight = EditorGUIUtility.singleLineHeight;
            if (property.objectReferenceValue == null ||
                property.objectReferenceValue is not ScriptableObject so ||
                !HasVisibleProperties(so) ||
                !property.isExpanded) return totalHeight;
            SerializedObject serializedObject = new(so);
            IMGUIDrawerOptions options = new()
            {
                ExcludeDrawerType = typeof(ExtendedScriptableObjectDrawer),
                ExcludeFields = new() { "m_Script" }
            };
            totalHeight += PropertyDrawerIMGUIFactory.GetSerializedObjectHeight(serializedObject, options);
            totalHeight += EditorGUIUtility.standardVerticalSpacing;
            return totalHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            Type type = GetFieldType();
            if (type == null || s_ignoreClassFullNames.Contains(type.FullName))
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.EndProperty();
                return;
            }
            bool hasValue = property.objectReferenceValue != null;
            bool shouldShowFoldout = hasValue && property.objectReferenceValue is ScriptableObject so && HasVisibleProperties(so);
            IMGUILayoutUtility.DrawFoldoutHeader(
                position, 
                property, 
                label, 
                out Rect contentRect, 
                shouldShowFoldout);
            Rect buttonRect = Rect.zero;
            Rect objectFieldRect = !hasValue ? IMGUILayoutUtility.GetContentRectWithButton(position, ButtonWidth, out buttonRect) : contentRect;
            IMGUIControlFactory.DrawObjectField(objectFieldRect, property, type);

            switch (hasValue)
            {
                case false when IMGUIControlFactory.DrawButton(buttonRect, "Create"):
                    CreateScriptableObjectAsset(property, type);
                    break;
                case true when property.isExpanded && property.objectReferenceValue is ScriptableObject scriptableObject:
                {
                    float yStart = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing - 1;
                    float contentHeight = position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
                    IMGUILayoutUtility.DrawExpandedBackground(position, yStart, contentHeight);
                    EditorGUI.indentLevel++;
                    SerializedObject serializedObject = new(scriptableObject);
                    float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    IMGUIDrawerOptions options = new()
                    {
                        ExcludeDrawerType = typeof(ExtendedScriptableObjectDrawer),
                        ExcludeFields = new() { "m_Script" }
                    };
                    Rect childrenRect = IMGUILayoutUtility.GetFieldRect(position, yOffset, contentHeight);
                    PropertyDrawerIMGUIFactory.DrawSerializedObject(childrenRect, serializedObject, options);
                    if (GUI.changed)
                        serializedObject.ApplyModifiedProperties();
                    EditorGUI.indentLevel--;
                    break;
                }
            }
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }

        #endregion
    }
}
#endif