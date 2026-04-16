#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SerializableType))]
public class SerializableTypeDrawer : PropertyDrawer {
    string[] m_typeNames, m_typeFullNames;

    void Initialize(SerializedProperty property) {
       if (m_typeFullNames != null) return;
       TypeFilterAttribute typeFilter = FindTypeFilterAttribute(property);

       Type[] filteredTypes = AppDomain.CurrentDomain.GetAssemblies()
          .SelectMany(assembly => assembly.GetTypes())
          .Where(t => typeFilter == null ? DefaultFilter(t) : typeFilter.Filter(t))
          .ToArray();

       m_typeNames = filteredTypes.Select(t => t.ReflectedType == null ? t.Name : $"{t.ReflectedType.Name}+{t.Name}").ToArray();
       m_typeFullNames = filteredTypes.Select(t => t.AssemblyQualifiedName).ToArray();
    }

    static TypeFilterAttribute FindTypeFilterAttribute(SerializedProperty property) {
       string path = property.propertyPath;
       int lastDot = path.LastIndexOf('.');
       if (lastDot <= 0) return null;
       string[] parts = path.Split('.');
       for (int i = parts.Length - 1; i >= 0; i--) {
          string partialPath = string.Join(".", parts.Take(i + 1));
          SerializedProperty parentProp = property.serializedObject.FindProperty(partialPath);
          if (parentProp == null) continue;
          FieldInfo field = GetFieldInfoFromProperty(parentProp, property.serializedObject.targetObject);
          if (field == null) continue;
          TypeFilterAttribute attr = (TypeFilterAttribute)Attribute.GetCustomAttribute(field, typeof(TypeFilterAttribute));
          if (attr != null) return attr;
       }

       return null;
    }

    static FieldInfo GetFieldInfoFromProperty(SerializedProperty property, object target) {
	    string[] parts = property.propertyPath.Split('.');
	    Type type = target.GetType();
	    FieldInfo field = null;

	    foreach (string part in parts)
	    {
		    if (IsCollectionAccessor(part))
		    {
			    switch (type)
			    {
				    case { IsArray: true }:
					    type = type.GetElementType();
					    break;
				    case { IsGenericType: true }:
				    {
					    Type genericDef = type.GetGenericTypeDefinition();
					    if (genericDef == typeof(System.Collections.Generic.List<>))
					    {
						    type = type.GetGenericArguments()[0];
					    }
					    break;
				    }
			    }

			    continue;
		    }

		    field = type?.GetField(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		    if (field == null) return null;

		    type = field.FieldType;
	    }

	    return field;
    }

    static bool IsCollectionAccessor(string part)
    {
	    // Unity's serialization patterns:
	    // Arrays: "Array" followed by "data[index]"
	    // Lists: "Array" followed by "data[index]"
	    return part == "Array" || part.StartsWith("data[");
    }

    static bool DefaultFilter(Type type) {
       return !type.IsAbstract && !type.IsInterface && !type.IsGenericType;
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property) {
       Initialize(property);

       SerializedProperty typeIdProperty = property.FindPropertyRelative("m_assemblyQualifiedName");
       if (string.IsNullOrEmpty(typeIdProperty.stringValue) && m_typeFullNames.Length > 0) {
          typeIdProperty.stringValue = m_typeFullNames.First();
          property.serializedObject.ApplyModifiedProperties();
       }

       int currentIndex = Array.IndexOf(m_typeFullNames, typeIdProperty.stringValue);
       if (currentIndex < 0 && m_typeFullNames.Length > 0) currentIndex = 0;

       PopupField<string> popupField = new(
          property.displayName,
          m_typeNames.ToList(),
          currentIndex
       );

       popupField.RegisterValueChangedCallback(evt => {
          int selectedIndex = m_typeNames.ToList().IndexOf(evt.newValue);
          if (selectedIndex < 0 || selectedIndex >= m_typeFullNames.Length) return;
          typeIdProperty.stringValue = m_typeFullNames[selectedIndex];
          property.serializedObject.ApplyModifiedProperties();
       });

       return popupField;
    }
}
#endif