#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MatrixUtils.Extensions;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using MatrixUtils.Attributes;
using MatrixUtils.PropertyDrawers.Helpers;
using UnityEngine;

namespace MatrixUtils.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ClassSelectorAttribute))]
    public class ClassSelectorPropertyDrawer : PropertyDrawer
    {
        ClassSelectorAttribute m_attributeData;
        bool m_isManagedReference;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            m_attributeData = attribute as ClassSelectorAttribute;
            m_isManagedReference = property.propertyType == SerializedPropertyType.ManagedReference;

            Type baseType = m_attributeData?.Type;
            if (baseType == null)
            {
                property.GetFieldInfoAndStaticType(out Type staticType);
                baseType = staticType;
            }

            if (baseType == null)
                return CreateErrorBox("Could not determine base type", $"Property: {property.propertyPath}");

            string validationError = ValidateBaseType(baseType, m_isManagedReference);
            if (validationError != null)
                return CreateErrorBox(validationError, $"Type: {baseType.Name}, Property: {property.propertyPath}");

            return m_isManagedReference
                ? CreatePolymorphicTypeUI(property, baseType)
                : CreateConcreteTypeUI(property);
        }

        static string ValidateBaseType(Type baseType, bool isManagedReference)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(baseType))
            {
                return isManagedReference
                    ? "[ClassSelector] cannot be used with UnityEngine.Object types on [SerializeReference] fields.\n" +
                      "Unity objects cannot be serialized as managed references.\n" +
                      "Remove [ClassSelector] or use a non-Unity object type."
                    : "[ClassSelector] is not needed for UnityEngine.Object types.\n" +
                      "Unity already provides object pickers for these types.\n" +
                      "Remove the [ClassSelector] attribute.";
            }

            if (baseType.IsGenericTypeDefinition)
                return "[ClassSelector] cannot be used with open generic types.\n" +
                       "Use a closed generic type (e.g., MyClass<int> instead of MyClass<T>).";

            // Static class check (abstract + sealed)
            if (baseType.IsAbstract && baseType.IsSealed)
                return "[ClassSelector] cannot be used with static classes.\n" +
                       "Static classes cannot be instantiated.";

            // Abstract base / interface types are valid targets for polymorphic fields
            if (isManagedReference && (baseType.IsAbstract || baseType.IsInterface))
                return null;

            // Structs are supported — they always have an implicit default constructor
            if (baseType.IsValueType)
                return null;

            // Concrete class: must have a parameterless constructor
            if (!baseType.IsAbstract && !HasParameterlessConstructor(baseType))
                return "[ClassSelector] requires types to have a parameterless constructor.\n" +
                       $"Type '{baseType.Name}' does not have one.\n" +
                       $"Add: public {baseType.Name}() {{ }}";

            // Warn against unexpected System types
            if (baseType.Namespace != null &&
                baseType.Namespace.StartsWith("System") &&
                !baseType.IsInterface &&
                baseType != typeof(string) &&
                baseType != typeof(Uri) &&
                !baseType.IsGenericType)
            {
                return "[ClassSelector] should not be used with System types.\n" +
                       $"Type '{baseType.FullName}' is a framework type that may not serialize correctly.";
            }

            return null;
        }
        static VisualElement CreateErrorBox(string message, string details)
        {
            VisualElement container = new() { style = { marginTop = 2, marginBottom = 2 } };
            container.Add(new HelpBox(message, HelpBoxMessageType.Error));

            if (!string.IsNullOrEmpty(details))
            {
                container.Add(new Label(details)
                {
                    style =
                    {
                        fontSize = 10,
                        color = new(Color.gray),
                        marginLeft = 4,
                        marginTop = 2,
                        whiteSpace = WhiteSpace.Normal
                    }
                });
            }

            return container;
        }

        static VisualElement CreateConcreteTypeUI(SerializedProperty property)
        {
            VisualElement root = new() { style = { marginTop = 2, marginBottom = 2 } };

            Foldout foldout = new()
            {
                text = ObjectNames.NicifyVariableName(property.name),
                value = property.isExpanded
            };

            foldout.RegisterValueChangedCallback(evt =>
            {
                property.isExpanded = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            VisualElement propertiesContainer = new()
            {
                style = { paddingLeft = 15, marginTop = 4 }
            };

            foldout.Add(propertiesContainer);
            root.Add(foldout);

            PropertyDrawerVisualElementFactory.CreateUIInContainer(property, propertiesContainer, new()
            {
                ExcludeDrawerType = typeof(ClassSelectorPropertyDrawer)
            });

            return root;
        }

        static VisualElement CreatePolymorphicTypeUI(SerializedProperty property, Type baseType)
        {
            VisualElement root = new() { style = { marginTop = 2, marginBottom = 2 } };

            Foldout foldout = new()
            {
                text = ObjectNames.NicifyVariableName(property.name),
                value = property.isExpanded
            };

            foldout.RegisterValueChangedCallback(evt =>
            {
                property.isExpanded = evt.newValue;
                property.serializedObject.ApplyModifiedProperties();
            });

            DropdownField dropdown = new()
            {
                name = "TypeSelectionDropdown",
                style = { marginBottom = 4, marginLeft = 0 }
            };

            VisualElement propertiesContainer = new()
            {
                name = "ObjectProperties",
                style = { paddingLeft = 15, marginTop = 4 }
            };

            foldout.Add(dropdown);
            foldout.Add(propertiesContainer);
            root.Add(foldout);

            List<Type> derivedTypes = PropertyDrawerRegistry
                .GetDerivedTypes(baseType, includeBaseType: !baseType.IsAbstract)
                .Where(IsTypeInstantiable)
                .ToList();

            if (derivedTypes.Count == 0)
            {
                foldout.Add(new HelpBox(
                    $"No valid instantiable types found that derive from '{baseType.Name}'.\n" +
                    "Types must be instantiable (classes with parameterless constructors, or structs) " +
                    "and cannot be Unity objects.",
                    HelpBoxMessageType.Warning
                ));
                return root;
            }

            Dictionary<string, Type> typesByName = derivedTypes.ToDictionary(t => t.Name);

            List<string> choices = new() { "None" };
            choices.AddRange(typesByName.Keys.OrderBy(name => name));
            dropdown.choices = choices;
            dropdown.SetValueWithoutNotify("None");

            dropdown.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue == "None")
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                    propertiesContainer.Clear();
                    return;
                }

                if (!typesByName.TryGetValue(evt.newValue, out Type selectedType)) return;

                if (!TryCreateInstance(selectedType, out object instance, out string error))
                {
                    Debug.LogError($"[ClassSelector] Failed to create instance of {selectedType.Name}: {error}");
                    propertiesContainer.Clear();
                    propertiesContainer.Add(new HelpBox(
                        $"Failed to instantiate '{selectedType.Name}'.\n{error}",
                        HelpBoxMessageType.Error
                    ));
                    return;
                }

                property.managedReferenceValue = instance;
                property.serializedObject.ApplyModifiedProperties();
                propertiesContainer.Clear();
                DrawManagedReferenceFields(property, propertiesContainer);
            });

            // Restore existing value
            object currentValue = GetCurrentManagedReferenceValue(property);
            if (currentValue == null)
            {
                dropdown.SetValueWithoutNotify("None");
                return root;
            }

            string currentTypeName = currentValue.GetType().Name;
            if (dropdown.choices.Contains(currentTypeName))
            {
                dropdown.SetValueWithoutNotify(currentTypeName);
                DrawManagedReferenceFields(property, propertiesContainer);
            }

            return root;
        }
        static bool IsTypeInstantiable(Type type)
        {
            if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return false;
            if (type.IsAbstract) return false;
            if (type.IsInterface) return false;
            if (type.IsGenericTypeDefinition) return false;
            return type.IsValueType || HasParameterlessConstructor(type);

        }

        static bool HasParameterlessConstructor(Type type)
        {
            return type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null) != null;
        }
        static bool TryCreateInstance(Type type, out object instance, out string error)
        {
            try
            {
                instance = Activator.CreateInstance(type);
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                instance = null;
                error = ex.InnerException?.Message ?? ex.Message;
                return false;
            }
        }

        static void DrawManagedReferenceFields(SerializedProperty property, VisualElement container)
        {
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = property.GetEndProperty();

            if (!iterator.NextVisible(true)) return;

            do
            {
                if (SerializedProperty.EqualContents(iterator, endProperty)) break;

                SerializedProperty childProperty = iterator.Copy();
                PropertyDrawer childDrawer = PropertyDrawerFactory.CreateDrawerForProperty(
                    childProperty,
                    excludeDrawerType: typeof(ClassSelectorPropertyDrawer),
                    excludeAttributeDrawers: true
                );

                VisualElement element = childDrawer?.CreatePropertyGUI(childProperty)
                                        ?? new PropertyField(childProperty);
                container.Add(element);

            } while (iterator.NextVisible(false));
        }

        static object GetCurrentManagedReferenceValue(SerializedProperty property)
        {
            object current = property.managedReferenceValue;
            if (current != null) return current;
            SerializedProperty backingField = property.serializedObject.FindProperty(
                $"<{property.name}>k__BackingField");

            if (backingField == null) return null;

            current = backingField.managedReferenceValue;
            if (current != null)
                property.managedReferenceValue = current;

            return current;
        }
    }
}
#endif