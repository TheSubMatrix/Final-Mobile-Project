#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MatrixUtils.Extensions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace MatrixUtils.PropertyDrawers.Helpers
{

    public static class PropertyDrawerRegistry
    {
        #region Cached Data Structures
        
        struct DrawerInfo
        {
            public Type DrawerType;
            public Type TargetType;
            public bool UseForChildren;
            public HashSet<string> HandledFields;
        }

        static Dictionary<Type, List<Type>> s_derivedTypesCache;
        static Dictionary<Type, DrawerInfo> s_drawerByTargetTypeCache;
        static Dictionary<Type, DrawerInfo> s_drawerByDrawerTypeCache;
        static Dictionary<string, Type> s_fieldDrawerCache;
        
        #endregion

        #region Initialization

        [InitializeOnLoadMethod]
        static void Initialize()
        {
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            RebuildCache();
        }

        static void OnCompilationFinished(object obj)
        {
            RebuildCache();
        }

        public static void RebuildCache()
        {
            s_derivedTypesCache = new();
            s_drawerByTargetTypeCache = new();
            s_drawerByDrawerTypeCache = new();
            s_fieldDrawerCache = new();

            IEnumerable<Type> allDrawers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(ass => ass.GetTypes())
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(PropertyDrawer)));

            foreach (Type drawerType in allDrawers)
            {
                foreach (CustomPropertyDrawer cpd in drawerType.GetCustomAttributes<CustomPropertyDrawer>(true))
                {
                    Type targetType = cpd.GetFieldValue<Type>("m_Type");
                    if (targetType == null) continue;

                    bool useForChildren = cpd.GetFieldValue<bool>("m_UseForChildren");
                    HashSet<string> handledFields = GetHandledFieldsForType(targetType);

                    DrawerInfo info = new()
                    {
                        DrawerType = drawerType,
                        TargetType = targetType,
                        UseForChildren = useForChildren,
                        HandledFields = handledFields
                    };

                    s_drawerByTargetTypeCache.TryAdd(targetType, info);
                    s_drawerByDrawerTypeCache.TryAdd(drawerType, info);
                }
            }
        }

        #endregion

        #region Type Hierarchy Queries

        /// <summary>
        /// Gets all non-abstract types that derive from or implement the specified base type.
        /// </summary>
        public static List<Type> GetDerivedTypes(Type baseType, bool includeBaseType = false)
        {
            if (baseType == null) return new();
            if (s_derivedTypesCache == null) RebuildCache();

            if (s_derivedTypesCache != null && s_derivedTypesCache.TryGetValue(baseType, out List<Type> types))
            {
                if (includeBaseType && !baseType.IsAbstract && !types.Contains(baseType))
                    return new(types) { baseType };
                return types;
            }

            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(ass => ass.GetTypes())
                .Where(t => t != baseType && !t.IsAbstract && baseType.IsAssignableFrom(t) 
                    && (baseType.IsInterface || !t.IsInterface))
                .ToList();

            if (includeBaseType && !baseType.IsAbstract)
                types.Add(baseType);

            if (s_derivedTypesCache != null) s_derivedTypesCache[baseType] = types;
            return types;
        }

        #endregion

        #region Drawer Resolution

        public static Type GetDrawerTypeForType(Type targetType)
        {
            if (targetType == null) return null;
            if (s_drawerByTargetTypeCache == null) RebuildCache();

            if (s_drawerByTargetTypeCache != null && s_drawerByTargetTypeCache.TryGetValue(targetType, out DrawerInfo info))
                return info.DrawerType;

            for (Type parentType = targetType.BaseType; parentType != null; parentType = parentType.BaseType)
            {
                if (s_drawerByTargetTypeCache != null && s_drawerByTargetTypeCache.TryGetValue(parentType, out info) && info.UseForChildren)
                    return info.DrawerType;
            }

            foreach (Type interfaceType in targetType.GetInterfaces())
            {
                if (s_drawerByTargetTypeCache != null && s_drawerByTargetTypeCache.TryGetValue(interfaceType, out info) && info.UseForChildren)
                    return info.DrawerType;
            }

            return null;
        }

        public static Type GetDrawerTypeForField(FieldInfo fieldInfo, Type excludeDrawerType = null, HashSet<Type> excludeDrawerTypes = null, bool excludeAttributeDrawers = false)
        {
            if (fieldInfo == null) return null;

            string cacheKey = $"{fieldInfo.DeclaringType?.FullName}.{fieldInfo.Name}";
            bool hasExclusions = excludeDrawerType != null || excludeDrawerTypes != null || excludeAttributeDrawers;

            if (!hasExclusions && s_fieldDrawerCache.TryGetValue(cacheKey, out Type cachedDrawerType))
                return cachedDrawerType;

            // Resolve drawer (prioritize attributes, then type-based)
            PropertyAttribute[] attributes = fieldInfo.GetCustomAttributes<PropertyAttribute>(true)
                .Reverse().ToArray();

            Type selectedDrawerType = null;
    
            if (!excludeAttributeDrawers)
            {
                selectedDrawerType = attributes.Select(attr => GetDrawerTypeForType(attr.GetType()))
                    .FirstOrDefault(drawerType => drawerType != null && !IsDrawerExcluded(drawerType, excludeDrawerType, excludeDrawerTypes));
            }
    
            if (selectedDrawerType == null)
            {
                Type typeDrawer = GetDrawerTypeForType(fieldInfo.FieldType);
                if (typeDrawer != null && !IsDrawerExcluded(typeDrawer, excludeDrawerType, excludeDrawerTypes))
                {
                    selectedDrawerType = typeDrawer;
                }
            }

            if (!hasExclusions)
                s_fieldDrawerCache[cacheKey] = selectedDrawerType;

            return selectedDrawerType;
        }

        static bool IsDrawerExcluded(Type drawerType, Type excludeDrawerType, HashSet<Type> excludeDrawerTypes)
        {
            if (drawerType == excludeDrawerType) return true;
            return excludeDrawerTypes != null && excludeDrawerTypes.Contains(drawerType);
        }

        public static Type GetDrawerTargetType(Type drawerType)
        {
            if (drawerType == null) return null;
            if (s_drawerByDrawerTypeCache == null) RebuildCache();
            return s_drawerByDrawerTypeCache != null && s_drawerByDrawerTypeCache.TryGetValue(drawerType, out DrawerInfo info) ? info.TargetType : null;
        }

        public static HashSet<string> GetDrawerHandledFields(Type drawerType)
        {
            if (drawerType == null) return null;
            if (s_drawerByDrawerTypeCache == null) RebuildCache();
            return s_drawerByDrawerTypeCache != null && s_drawerByDrawerTypeCache.TryGetValue(drawerType, out DrawerInfo info) ? info.HandledFields : null;
        }

        public static bool DrawerHandlesField(Type drawerType, string fieldName)
        {
            HashSet<string> handledFields = GetDrawerHandledFields(drawerType);
            return handledFields != null && handledFields.Contains(fieldName);
        }

        #endregion

        #region Helper Methods

        static HashSet<string> GetHandledFieldsForType(Type targetType)
        {
            HashSet<string> handledFields = new();
            if (targetType == null) return handledFields;

            FieldInfo[] fields = targetType.GetFields(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields.Where(field => field.IsPublic || field.GetCustomAttribute<SerializeField>() != null))
            {
                handledFields.Add(field.Name);
            }

            return handledFields;
        }

        #endregion
    }
}
#endif