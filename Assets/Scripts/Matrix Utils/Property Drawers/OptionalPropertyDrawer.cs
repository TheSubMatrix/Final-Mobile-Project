#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(Optional<>))]
public class OptionalPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement container = new()
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                alignItems = Align.Center
            }
        };
        SerializedProperty hasValueProp = property.FindPropertyRelative("m_hasValue");
        SerializedProperty valueProp = property.FindPropertyRelative("m_value");
        PropertyField valueField = new(valueProp, property.displayName)
        {
            style =
            {
                flexGrow = 1
            },
            bindingPath = valueProp.propertyPath
        };
        Toggle toggle = new()
        {
            style =
            {
                width = 16,
                marginRight = 4
            },
            bindingPath = hasValueProp.propertyPath
        };
        container.TrackPropertyValue(hasValueProp, prop =>
        {
            valueField.SetEnabled(prop.boolValue);
        });
        valueField.SetEnabled(hasValueProp.boolValue);
        container.Add(toggle);
        container.Add(valueField);

        return container;
    }
}
#endif