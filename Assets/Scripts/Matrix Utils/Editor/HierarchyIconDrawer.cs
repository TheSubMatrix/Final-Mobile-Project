#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MatrixUtils.Attributes;
using MatrixUtils.Extensions;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class HierarchyIconDrawer
{
    static readonly Texture2D s_requiredIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Scripts/Matrix Utils/Editor/Editor Assets/RequiredFieldIcon.png");
    static readonly Type s_requiredFieldAttributeType = typeof(RequiredFieldAttribute);

    static HierarchyIconDrawer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemOnGUI;
        Undo.undoRedoEvent += OnUndoRedo;
    }

    static void OnUndoRedo(in UndoRedoInfo info)
    {
        EditorApplication.RepaintHierarchyWindow();
    }

    static void OnHierarchyItemOnGUI(int entityId, Rect selectionRect)
    {
        if (EditorUtility.EntityIdToObject(entityId) is not GameObject gameObject) return;

        Component[] components = gameObject.GetComponents<Component>();
        bool hasUnassignedField = (from component in components where component != null select new SerializedObject(component)).Any(HasUnassignedRequiredFieldInSerializedObject);

        if (!hasUnassignedField) return;

        Rect iconRect = new Rect(selectionRect.xMax - 20, selectionRect.y, 16, 16);

        if (s_requiredIcon != null)
        {
            GUI.Label(iconRect, new GUIContent(s_requiredIcon, "One or more required fields are missing or empty."));
        }
        else
        {
            EditorGUI.DrawRect(iconRect, Color.red);
        }
    }

    static bool HasUnassignedRequiredFieldInSerializedObject(SerializedObject serializedObject)
    {
        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;

        while (property.Next(enterChildren))
        {
            // Skip the script reference
            if (property.propertyPath == "m_Script")
            {
                enterChildren = false;
                continue;
            }

            // Get the FieldInfo for this property
            FieldInfo fieldInfo = property.GetFieldInfoAndStaticType(out _);
            
            // Check if this field has the RequiredField attribute
            if (fieldInfo != null && fieldInfo.IsDefined(s_requiredFieldAttributeType, false))
            {
                if (RequiredFieldUtility.IsFieldUnassigned(property))
                {
                    return true;
                }
            }

            // Continue traversing children
            enterChildren = true;
        }

        return false;
    }
}
#endif