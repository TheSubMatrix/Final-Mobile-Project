#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MatrixUtils.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[CustomEditor(typeof(ScriptableObjectEventListener))]
public class DynamicEventHandlerEditor : Editor
{
    void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode) return;
        SerializedProperty bindings = serializedObject.FindProperty("m_bindings");
        for (int i = 0; i < bindings.arraySize; i++)
        {
            SerializedProperty b = bindings.GetArrayElementAtIndex(i);
            SerializedProperty listeners = b.FindPropertyRelative("m_listeners");
            for (int j = 0; j < listeners.arraySize; j++)
            {
                SerializedProperty listener = listeners.GetArrayElementAtIndex(j);
                Debug.Log($"Before play — binding: {b.FindPropertyRelative("m_eventName").stringValue} method: {listener.FindPropertyRelative("m_methodName").stringValue}");
            }
        }
    }
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new();
        PropertyField listener = new(serializedObject.FindProperty("m_listener"));
        listener.RegisterValueChangeCallback(evt =>
        {
            RebuildPropertyList(root, evt.changedProperty);
        });
        root.Add(listener);
        return root;
    }

    void RebuildPropertyList(VisualElement root, SerializedProperty scriptableObject)
    {
        while (root.childCount > 1) root.RemoveAt(1);
        if (scriptableObject.objectReferenceValue == null) return;
        SerializedObject so = new(scriptableObject.objectReferenceValue);
        SerializedProperty prop = so.GetIterator();
        prop.NextVisible(true);
        while (prop.NextVisible(false))
        {
            FieldInfo fieldInfo = prop.GetFieldInfoAndStaticType(out Type type);
            if (fieldInfo == null || !IsUnityEvent(type)) continue;

            string eventName = prop.name.StartsWith("<")
                ? prop.name[1..prop.name.IndexOf('>')]
                : prop.name;

            Type[] paramTypes = type.IsGenericType ? type.GetGenericArguments() : Type.EmptyTypes;

            SerializedProperty bindings = serializedObject.FindProperty("m_bindings");
            SerializedProperty binding = FindOrCreateBinding(bindings, eventName);

            root.Add(BuildEventBindingUI(binding, eventName, paramTypes));
        }
    }

    VisualElement BuildEventBindingUI(SerializedProperty binding, string eventName, Type[] paramTypes)
    {
        VisualElement container = new()
        {
            style =
            {
                borderTopWidth = 1,
                borderTopColor = new(new Color(0.3f, 0.3f, 0.3f)),
                marginTop = 4,
                paddingTop = 4
            }
        };

        Label header = new(eventName)
        {
            style =
            {
                unityFontStyleAndWeight = FontStyle.Bold,
                marginBottom = 4
            }
        };
        container.Add(header);
        SerializedProperty listeners = binding.FindPropertyRelative("m_listeners");
        VisualElement listContainer = new();
        container.Add(listContainer);
        RebuildListUI();
        Button addButton = new(() =>
        {
            listeners.arraySize++;
            serializedObject.ApplyModifiedProperties();
            RebuildListUI();
        })
        {
            text = "+",
            style =
            {
                marginTop = 4,
                width = 20
            }
        };
        container.Add(addButton);
        return container;
        void RebuildListUI()
        {
            listContainer.Clear();
            for (int i = 0; i < listeners.arraySize; i++)
            {
                int index = i;
                SerializedProperty listenerProp = listeners.GetArrayElementAtIndex(i);
                listContainer.Add(BuildListenerUI(listenerProp, paramTypes, () =>
                {
                    listeners.DeleteArrayElementAtIndex(index);
                    serializedObject.ApplyModifiedProperties();
                    RebuildListUI();
                }));
            }
        }
    }

    VisualElement BuildListenerUI(SerializedProperty listenerProp, Type[] paramTypes, Action onRemove)
    {
        VisualElement row = new()
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                marginBottom = 2
            }
        };
        SerializedProperty targetProp = listenerProp.FindPropertyRelative("m_target");
        SerializedProperty methodProp = listenerProp.FindPropertyRelative("m_methodName");
        ObjectField targetField = new()
        {
            objectType = typeof(UnityEngine.Object),
            style =
            {
                flexGrow = 1
            }
        };
        targetField.BindProperty(targetProp);

        DropdownField methodDropdown = new()
        {
            style =
            {
                flexGrow = 1,
                marginLeft = 4
            }
        };
        serializedObject.Update();
        RefreshMethodDropdown(targetProp.objectReferenceValue);

        targetField.RegisterValueChangedCallback(evt =>
        {
            RefreshMethodDropdown(evt.newValue);
            if (evt.newValue == null) return;
            string currentMethod = listenerProp.FindPropertyRelative("m_methodName").stringValue;
            if (GetCompatibleMethods(evt.newValue.GetType(), paramTypes).Contains(currentMethod)) return;
            serializedObject.Update();
            listenerProp.FindPropertyRelative("m_methodName").stringValue = "";
            serializedObject.ApplyModifiedProperties();
        });

        methodDropdown.RegisterValueChangedCallback(evt =>
        {
            methodProp.stringValue = evt.newValue == "No Function" ? "" : evt.newValue;
            serializedObject.ApplyModifiedProperties(); 
        });
        Button removeButton = new(onRemove)
        {
            text = "-",
            style =
            {
                width = 20,
                marginLeft = 4
            }
        };
        row.Add(targetField);
        row.Add(methodDropdown);
        row.Add(removeButton);
        return row;
        void RefreshMethodDropdown(UnityEngine.Object targetObject)
        {
            methodDropdown.choices = new() { "No Function" };
            methodDropdown.SetValueWithoutNotify("No Function");
            if (targetObject == null) return;
            List<string> methods = GetCompatibleMethods(targetObject.GetType(), paramTypes);
            methodDropdown.choices = new List<string> { "No Function" }.Concat(methods).ToList();
            string current = methodProp.stringValue;
            methodDropdown.SetValueWithoutNotify(
                methods.Contains(current) ? current : "No Function");
        }
    }

    static List<string> GetCompatibleMethods(Type type, Type[] paramTypes)
    {
        return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m =>
            {
                if (m.IsSpecialName) return false;
                if (m.ReturnType != typeof(void)) return false;
                ParameterInfo[] methodParams = m.GetParameters();
                if (methodParams.Length == 0) return true;
                if (methodParams.Length != paramTypes.Length) return false;
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    if (!IsCompatibleParameter(methodParams[i].ParameterType, paramTypes[i])) return false;
                }
                return true;
            })
            .Select(m => m.Name)
            .ToList();
    }

    static bool IsCompatibleParameter(Type methodParamType, Type eventParamType)
    {
        if (methodParamType == eventParamType) return true;
        if (methodParamType.IsAssignableFrom(eventParamType)) return true;
        MethodInfo implicitOp = eventParamType.GetMethods(BindingFlags.Static | BindingFlags.Public)
            .FirstOrDefault(m =>
                m.Name == "op_Implicit" &&
                m.ReturnType == methodParamType &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == eventParamType);
        return implicitOp != null;
    }
    
    SerializedProperty FindOrCreateBinding(SerializedProperty bindings, string eventName)
    {
        for (int i = 0; i < bindings.arraySize; i++)
        {
            SerializedProperty b = bindings.GetArrayElementAtIndex(i);
            if (b.FindPropertyRelative("m_eventName").stringValue == eventName)
                return b;
        }

        bindings.arraySize++;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        bindings = serializedObject.FindProperty("m_bindings");
        SerializedProperty binding = bindings.GetArrayElementAtIndex(bindings.arraySize - 1);
        binding.FindPropertyRelative("m_eventName").stringValue = eventName;
        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        return binding;
    }

    static bool IsUnityEvent(Type type)
    {
        while (type != null)
        {
            if (type == typeof(UnityEventBase)) return true;
            type = type.BaseType;
        }
        return false;
    }
}
#endif