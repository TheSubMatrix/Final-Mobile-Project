#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

[CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
public class RequireInterfaceDrawer : PropertyDrawer {
    RequireInterfaceAttribute RequireInterfaceAttribute => (RequireInterfaceAttribute)attribute;

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        Type interfaceType = RequireInterfaceAttribute.InterfaceType;

        if (interfaceType is not { IsInterface: true }) {
            return new Label("Error: Invalid interface type specified");
        }

        VisualElement container = new();
        Type baseType = GetObjectTypeForInterface(fieldInfo.FieldType, interfaceType);

        ObjectField objectField = new(property.displayName) {
            objectType = baseType,
            allowSceneObjects = true,
            value = property.objectReferenceValue
        };

        bool isUpdating = false;
        string propertyPath = property.propertyPath;
        SerializedObject serializedObject = property.serializedObject;

        objectField.RegisterValueChangedCallback(evt => {
            if (isUpdating) return;

            Object newValue = evt.newValue;
            Object validatedValue = ValidateAndGetComponent(newValue, interfaceType);

            if (validatedValue != newValue) {
                if (validatedValue == null && newValue != null) {
                    Debug.LogWarning($"The assigned object does not implement '{interfaceType.Name}'.");
                }

                isUpdating = true;
                serializedObject.Update();
                SerializedProperty prop = serializedObject.FindProperty(propertyPath);
                prop.objectReferenceValue = validatedValue;
                serializedObject.ApplyModifiedProperties();
                objectField.value = validatedValue;
                isUpdating = false;
            } else {
                serializedObject.Update();
                SerializedProperty prop = serializedObject.FindProperty(propertyPath);
                prop.objectReferenceValue = validatedValue;
                serializedObject.ApplyModifiedProperties();
            }
        });

        Undo.undoRedoEvent += OnUndoRedo;

        container.RegisterCallback<DetachFromPanelEvent>(_ => {
            Undo.undoRedoEvent -= OnUndoRedo;
        });

        container.Add(objectField);
        container.Add(CreateInterfaceLabelOverlay(interfaceType.Name, objectField));

        return container;

        // Track the ROOT serialized object for undo/redo on nested properties
        void OnUndoRedo(in UndoRedoInfo info) {
	        if (serializedObject.targetObject == null) return;

	        EditorApplication.delayCall += () => {
		        if (serializedObject.targetObject == null) return;

		        isUpdating = true;
		        serializedObject.Update();
		        SerializedProperty prop = serializedObject.FindProperty(propertyPath);

		        if (prop != null) {
			        Object newValue = prop.objectReferenceValue;
			        objectField.SetValueWithoutNotify(null);
			        objectField.value = newValue;
			        container.MarkDirtyRepaint();
		        }

		        isUpdating = false;
	        };
        }
    }

    static Object ValidateAndGetComponent(Object obj, Type interfaceType) {
        if (obj == null) return null;

        if (obj is GameObject gameObject) {
            return gameObject.GetComponent(interfaceType);
        }

        return interfaceType.IsAssignableFrom(obj.GetType()) ? obj : null;
    }

    static Type GetObjectTypeForInterface(Type fieldType, Type interfaceType) {
        Type elementType = GetElementType(fieldType);
        if (interfaceType.IsAssignableFrom(elementType))
            return elementType;
        if (typeof(ScriptableObject).IsAssignableFrom(elementType))
            return typeof(ScriptableObject);

        return typeof(MonoBehaviour).IsAssignableFrom(elementType) ? typeof(MonoBehaviour) : typeof(Object);
    }

    static Type GetElementType(Type type) {
        if (type.IsArray)
            return type.GetElementType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            return type.GetGenericArguments()[0];

        return type;
    }

    static Label CreateInterfaceLabelOverlay(string interfaceName, ObjectField objectField) {
        Label label = new()
        {
            pickingMode = PickingMode.Ignore,
            style = {
                position = Position.Absolute,
                right = 20,
                top = 1,
                bottom = 1,
                unityTextAlign = TextAnchor.MiddleRight,
                paddingRight = 2,
                fontSize = 11,
                color = new Color(0.7f, 0.7f, 0.7f, 1f)
            }
        };
        UpdateLabelText(false);
        objectField.RegisterCallback<MouseEnterEvent>(_ => UpdateLabelText(true));
        objectField.RegisterCallback<MouseLeaveEvent>(_ => UpdateLabelText(false));
        objectField.RegisterValueChangedCallback(_ => UpdateLabelText(false));

        return label;

        void UpdateLabelText(bool isHovering) {
            label.text = (objectField.value == null || isHovering) ? $"({interfaceName})" : "*";
        }
    }
}
#endif