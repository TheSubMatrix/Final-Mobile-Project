#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MatrixUtils.Extensions;
using UnityEditor;
using UnityEngine;

namespace MatrixUtils.PropertyDrawers.Helpers
{
    /// <summary>
    /// Factory for creating PropertyDrawer instances.
    /// Handles instantiation and configuration only.
    /// </summary>
    public static class PropertyDrawerFactory
    {
        /// <summary>
        /// Creates a PropertyDrawer instance for a given drawer type.
        /// </summary>
        public static PropertyDrawer CreateDrawer(Type drawerType, FieldInfo fieldInfo, PropertyAttribute attribute = null)
        {
            if (drawerType == null || !typeof(PropertyDrawer).IsAssignableFrom(drawerType))
                return null;

            if (fieldInfo == null)
            {
                Debug.LogWarning($"PropertyDrawerFactory: Cannot create drawer {drawerType.Name} without FieldInfo");
                return null;
            }

            try
            {
                PropertyDrawer drawer = (PropertyDrawer)Activator.CreateInstance(drawerType);
                drawer.SetFieldValue("m_FieldInfo", fieldInfo);
                if (attribute != null)
                    drawer.SetFieldValue("m_Attribute", attribute);
                return drawer;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"PropertyDrawerFactory: Failed to create drawer instance for {drawerType.Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Creates a PropertyDrawer for the given property.
        /// </summary>
        public static PropertyDrawer CreateDrawerForProperty(
            SerializedProperty property, 
            Type excludeDrawerType = null,
            HashSet<Type> excludeDrawerTypes = null,
            bool excludeAttributeDrawers = false)
        {
            if (property == null) return null;

            FieldInfo fieldInfo = property.GetFieldInfoAndStaticType(out _);
            if (fieldInfo == null) return null;

            Type drawerType = PropertyDrawerRegistry.GetDrawerTypeForField(fieldInfo, excludeDrawerType, excludeDrawerTypes, excludeAttributeDrawers);
            if (drawerType == null) return null;

            // Get the matching attribute if it's an attribute-based drawer
            PropertyAttribute matchingAttr = fieldInfo.GetCustomAttributes<PropertyAttribute>(true)
                .FirstOrDefault(attr => PropertyDrawerRegistry.GetDrawerTypeForType(attr.GetType()) == drawerType);

            return CreateDrawer(drawerType, fieldInfo, matchingAttr);
        }
    }
}
#endif