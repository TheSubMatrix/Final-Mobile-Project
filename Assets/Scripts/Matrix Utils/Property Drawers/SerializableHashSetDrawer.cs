#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SerializableHashSet<>), true)]
public class SerializableHashSetDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        Type[] genericArgs = fieldInfo.FieldType.GetGenericArguments();
        if (genericArgs.Length != 1)
            return new Label("Error: Invalid SerializableHashSet type");
        
        Type itemType = genericArgs[0];

        VisualElement container = new();
        SerializedProperty listProperty = property.FindPropertyRelative("m_list");
        SerializedProperty stagingProperty = property.FindPropertyRelative("m_stagingValue");

        Foldout foldout = new()
        {
            text = $"{property.displayName} ({listProperty.arraySize} items)",
            value = property.isExpanded
        };
        foldout.RegisterValueChangedCallback(evt => property.isExpanded = evt.newValue);

        // --- Staging area ---
        VisualElement newItemSection = new()
        {
            style = 
            {
                backgroundColor = new StyleColor(EditorGUIUtility.isProSkin 
                    ? new Color(0.3f, 0.3f, 0.3f, 0.2f) 
                    : new Color(0.8f, 0.8f, 0.8f, 0.2f)),
                paddingTop = 4,
                paddingBottom = 4,
                paddingLeft = 4,
                paddingRight = 4,
                marginBottom = 4,
                borderBottomLeftRadius = 3,
                borderBottomRightRadius = 3,
                borderTopLeftRadius = 3,
                borderTopRightRadius = 3
            }
        };

        DrawUIWithLabel(stagingProperty, newItemSection, "New Item", true, itemType);

        Button addButton = new(() =>
        {
            object stagingValue = stagingProperty.boxedValue;
            
            // Check if staging value is valid
            if (stagingValue == null)
            {
                Debug.LogWarning("Cannot add item: staging value is null");
                return;
            }
            
            // Check if item already exists using boxedValue comparison
            int existingIndex = -1;
            
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty element = listProperty.GetArrayElementAtIndex(i);
                object existingValue = element.boxedValue;

                if (existingValue == null || !Equals(existingValue, stagingValue)) continue;
                existingIndex = i;
                break;
            }

            if (existingIndex >= 0)
            {
                Debug.LogWarning($"Item already exists in set at index {existingIndex}");
                return;
            }

            // Add new item
            int index = listProperty.arraySize;
            listProperty.arraySize++;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();

            SerializedProperty newElement = listProperty.GetArrayElementAtIndex(index);

            // Copy value using boxedValue
            newElement.boxedValue = stagingValue;

            property.serializedObject.ApplyModifiedProperties();
        })
        {
            text = "Add Item",
            style = { alignSelf = Align.FlexEnd, width = 100, marginTop = 4 }
        };
        newItemSection.Add(addButton);

        foldout.Add(newItemSection);

        // --- Existing items list ---
        VisualElement listContainer = new();
        foldout.Add(listContainer);
        container.Add(foldout);

        RebuildList();
        
        // Track changes to list size
        int lastArraySize = listProperty.arraySize;
        container.TrackPropertyValue(listProperty, p =>
        {
            if (p.arraySize == lastArraySize) return;
            lastArraySize = p.arraySize;
            foldout.text = $"{property.displayName} ({p.arraySize} items)";
            RebuildList();
        });

        return container;

        void RebuildList()
        {
            listContainer.Clear();

            if (listProperty.arraySize == 0)
            {
                Label emptyLabel = new("No items")
                {
                    style = 
                    {
                        unityFontStyleAndWeight = FontStyle.Italic,
                        color = new StyleColor(Color.gray),
                        paddingTop = 4,
                        paddingBottom = 4
                    }
                };
                listContainer.Add(emptyLabel);
                return;
            }

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                int idx = i;
                SerializedProperty element = listProperty.GetArrayElementAtIndex(i);

                VisualElement row = new()
                {
                    style = 
                    {
                        flexDirection = FlexDirection.Row,
                        alignItems = Align.Center,
                        paddingTop = 4,
                        paddingBottom = 4,
                        paddingLeft = 4,
                        paddingRight = 4,
                        marginBottom = 4,
                        backgroundColor = new StyleColor(EditorGUIUtility.isProSkin 
                            ? new Color(0.25f, 0.25f, 0.25f, 0.3f) 
                            : new Color(0.85f, 0.85f, 0.85f, 0.3f)),
                        borderBottomLeftRadius = 3,
                        borderBottomRightRadius = 3,
                        borderTopLeftRadius = 3,
                        borderTopRightRadius = 3
                    }
                };

                // Item (read-only)
                VisualElement itemContainer = new()
                {
                    style = { flexGrow = 1 }
                };
                DrawUIWithLabel(element, itemContainer, $"Item {i}", false, itemType);
                row.Add(itemContainer);

                // Remove button
                Button removeButton = new(() =>
                {
                    listProperty.DeleteArrayElementAtIndex(idx);
                    property.serializedObject.ApplyModifiedProperties();
                })
                {
                    text = "Remove",
                    style = 
                    {
                        width = 70,
                        backgroundColor = new StyleColor(new Color(0.8f, 0.3f, 0.3f, 0.8f))
                    }
                };
                row.Add(removeButton);

                listContainer.Add(row);
            }
        }
    }

    static void DrawUIWithLabel(SerializedProperty prop, VisualElement container, string label, bool enabled, Type expectedType = null)
    {
        VisualElement fieldContainer = new()
        {
            style = { marginBottom = 2 }
        };
        
        // If it's an object reference and we know the expected type, use ObjectField
        if (expectedType != null && 
            typeof(UnityEngine.Object).IsAssignableFrom(expectedType) && 
            prop.propertyType == SerializedPropertyType.ObjectReference)
        {
            ObjectField objectField = new(label)
            {
                objectType = expectedType,
                allowSceneObjects = true,
                value = prop.objectReferenceValue
            };
            
            // Handle value changes
            objectField.RegisterValueChangedCallback(evt =>
            {
                prop.objectReferenceValue = evt.newValue;
                prop.serializedObject.ApplyModifiedProperties();
            });
            
            // Track property changes to update the field
            fieldContainer.TrackPropertyValue(prop, p =>
            {
                if (objectField.value != p.objectReferenceValue)
                    objectField.value = p.objectReferenceValue;
            });
            
            objectField.SetEnabled(enabled);
            fieldContainer.Add(objectField);
        }
        else
        {
            // Use standard PropertyField for everything else (enums, primitives, structs, etc.)
            PropertyField field = new(prop, label);
            field.BindProperty(prop);
            field.SetEnabled(enabled);
            fieldContainer.Add(field);
        }
        
        container.Add(fieldContainer);
    }
}
#endif