#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEditor;
public static class RequiredFieldUtility
{
    // Shared list of common variable names used in Wrapper classes (like InterfaceReference, etc.)
    static readonly string[] s_wrapperFieldNames =
        { "m_underlyingValue", "m_value", "value", "_value", "m_target", "target" };

    /// <summary>
    /// Checks if a field is unassigned using SerializedProperty (Inspector context).
    /// </summary>
    public static bool IsFieldUnassigned(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue == null;

            case SerializedPropertyType.ExposedReference:
                return property.exposedReferenceValue == null;

            case SerializedPropertyType.String:
                return string.IsNullOrEmpty(property.stringValue);

            case SerializedPropertyType.AnimationCurve:
                return property.animationCurveValue == null || property.animationCurveValue.length == 0;
            default:
	            object valueToCheck;
	            try
	            {
		            valueToCheck = property.boxedValue;
	            }
	            catch
	            {
		            return false;
	            }
	            return IsFieldUnassigned(valueToCheck);
        }
    }
    public static bool IsFieldUnassigned(object fieldValue)
    {
	    switch (fieldValue)
	    {
		    case null:
			    return true;
		    case UnityEngine.Object unityObj:
			    return unityObj == null;
		    case string str:
			    return string.IsNullOrEmpty(str);
	    }

	    Type type = fieldValue.GetType();

        foreach (object wrappedValue in from fieldName in s_wrapperFieldNames select GetFieldRecursively(type, fieldName) into info where info != null select info.GetValue(fieldValue))
        {
	        return IsFieldUnassigned(wrappedValue);
        }
        if (fieldValue is IEnumerable enumerable)
        {
            return IsEnumerableEmpty(enumerable);
        }
        return CheckForCustomContainerEmptiness(fieldValue, type);
    }

    /// <summary>
    /// Helper to find a field in a type or its base classes.
    /// </summary>
    static FieldInfo GetFieldRecursively(Type type, string fieldName)
    {
        Type currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            FieldInfo field = currentType.GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (field != null) return field;

            currentType = currentType.BaseType;
        }
        return null;
    }

    static bool CheckForCustomContainerEmptiness(object fieldValue, Type fieldType)
    {
        PropertyInfo countProperty = fieldType.GetProperty("Count");
        if (countProperty != null && countProperty.PropertyType == typeof(int))
        {
            return (int)countProperty.GetValue(fieldValue, null) == 0;
        }
        PropertyInfo lengthProperty = fieldType.GetProperty("Length");
        if (lengthProperty != null && lengthProperty.PropertyType == typeof(int))
        {
            return (int)lengthProperty.GetValue(fieldValue, null) == 0;
        }
        PropertyInfo isEmptyProperty = fieldType.GetProperty("IsEmpty");
        if (isEmptyProperty != null && isEmptyProperty.PropertyType == typeof(bool))
        {
            return (bool)isEmptyProperty.GetValue(fieldValue, null);
        }

        return false;
    }

    static bool IsEnumerableEmpty(IEnumerable enumerable)
    {
        IEnumerator enumerator = enumerable.GetEnumerator();
        try
        {
            return !enumerator.MoveNext();
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }
}
#endif