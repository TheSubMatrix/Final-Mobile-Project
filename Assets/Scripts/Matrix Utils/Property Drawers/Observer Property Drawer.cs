#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using MatrixUtils.GenericDatatypes;
using MatrixUtils.PropertyDrawers.Helpers;

namespace MatrixUtils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(Observer<>), true)]
    public class ObserverPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement root = new();
            
            // Create foldout
            Foldout foldout = new()
            {
                text = property.displayName,
                value = property.isExpanded
            };
            
            foldout.RegisterValueChangedCallback(evt =>
            {
                property.isExpanded = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });
            
            // Create a container for the field content
            VisualElement contentContainer = new()
            {
                style = { paddingLeft = 15, marginTop = 4 }
            };
            DrawerOptions options = new()
            {
                ExcludeDrawerType = typeof(ObserverPropertyDrawer)
            };
            
            PropertyDrawerVisualElementFactory.CreateUIInContainer(property, contentContainer, options);
            foldout.Add(contentContainer);
            root.Add(foldout);
            return root;
        }
    }
}
#endif