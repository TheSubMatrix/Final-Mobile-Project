#if UNITY_EDITOR
using MatrixUtils.Attributes;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(RequiredFieldAttribute))]
public class RequiredFieldDrawer : PropertyDrawer {
    static readonly Texture2D s_requiredIcon;

    static RequiredFieldDrawer() {
        s_requiredIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Scripts/Matrix Utils/Property Drawers/Drawer Assets/RequiredFieldIcon.png");
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
        VisualElement container = new()
        {
            style =
            {
                flexDirection = FlexDirection.Row
            }
        };

        // Create the property field
        PropertyField propertyField = new(property)
        {
            style =
            {
                flexGrow = 1
            }
        };
        container.Add(propertyField);

        // Create the icon
        Image icon = new()
        {
            image = s_requiredIcon,
            tooltip = "This field is required and is either missing or empty!",
            style =
            {
                width = 16,
                height = 16,
                marginLeft = 2
            }
        };
        container.Add(icon);

        string propertyPath = property.propertyPath;
        SerializedObject serializedObject = property.serializedObject;

        // Initial check
        UpdateIconVisibility();

        // Track the serialized object for ANY changes
        container.TrackSerializedObjectValue(serializedObject, _ => {
            UpdateIconVisibility();
        });

        Undo.undoRedoEvent += OnUndoRedo;

        container.RegisterCallback<DetachFromPanelEvent>(_ => {
            Undo.undoRedoEvent -= OnUndoRedo;
        });

        // Also repaint hierarchy on changes
        propertyField.RegisterValueChangeCallback(_ => {
            UpdateIconVisibility();
            EditorApplication.RepaintHierarchyWindow();
        });

        return container;

        // Also handle undo/redo explicitly
        void OnUndoRedo(in UndoRedoInfo info) {
	        if (serializedObject.targetObject != null) {
		        EditorApplication.delayCall += () =>
		        {
			        if (serializedObject.targetObject == null) return;
			        UpdateIconVisibility();
			        EditorApplication.RepaintHierarchyWindow();
		        };
	        }
        }

        // Update icon visibility based on field value
        void UpdateIconVisibility() {
            serializedObject.Update();
            SerializedProperty prop = serializedObject.FindProperty(propertyPath);
            if (prop != null) {
                icon.style.display = IsFieldUnassigned(prop) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }

    static bool IsFieldUnassigned(SerializedProperty property)
    {
        return RequiredFieldUtility.IsFieldUnassigned(property);
    }
}
#endif