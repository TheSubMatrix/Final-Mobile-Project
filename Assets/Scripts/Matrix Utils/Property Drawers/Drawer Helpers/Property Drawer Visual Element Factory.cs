#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MatrixUtils.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MatrixUtils.PropertyDrawers.Helpers
{
    /// <summary>
    /// Options for controlling UI generation behavior.
    /// </summary>
    public class DrawerOptions
    {
        public Type ExcludeDrawerType { get; set; }
        public HashSet<Type> ExcludeDrawerTypes { get; set; }
        public HashSet<string> ExcludeFields { get; set; }
        public bool ShowDefaultFields { get; set; } = true;
        public bool ExcludeAttributeDrawers { get; set; } = false;
    }

    /// <summary>
    /// Factory for creating VisualElements from types and instances.
    /// Clean, type-safe API for UI generation.
    /// </summary>
    public static class PropertyDrawerVisualElementFactory
    {
        // Delegate for cached UI builders
        delegate void BuildUIForType(SerializedProperty property, VisualElement container, DrawerOptions options);
        static readonly Dictionary<Type, BuildUIForType> s_uiBuilderCache;

        static PropertyDrawerVisualElementFactory()
        {
            s_uiBuilderCache = new();
        }

        #region Public API

        /// <summary>
        /// Creates UI for a specific type using the property as data source.
        /// This is the main entry point for type-based UI generation.
        /// </summary>
        public static VisualElement CreateUI(Type type, SerializedProperty property, DrawerOptions options = null)
        {
            if (type == null || property == null)
                return null;

            options ??= new();
            VisualElement container = new();
            
            DrawUIForType(type, property, container, options);
            
            return container;
        }

        /// <summary>
        /// Creates UI for a property, automatically detecting its type.
        /// </summary>
        public static VisualElement CreateUI(SerializedProperty property, DrawerOptions options = null)
        {
            if (property == null) return null;

            property.GetFieldInfoAndStaticType(out Type fieldType);
            return fieldType == null ? null : CreateUI(fieldType, property, options);
        }

        /// <summary>
        /// Creates UI for a property within a container. Automatically handles fallback to default fields.
        /// This is the recommended method for property drawers.
        /// </summary>
        public static void CreateUIInContainer(SerializedProperty property, VisualElement container, DrawerOptions options = null)
        {
            if (property == null || container == null) return;

            property.GetFieldInfoAndStaticType(out Type fieldType);
            
            if (fieldType != null)
            {
                DrawUIForType(fieldType, property, container, options ?? new DrawerOptions());
            }
            else
            {
                // Fallback: render all children as PropertyFields
                CreateDefaultPropertyFields(property, container, options);
            }
        }

        #endregion

        #region Core UI Building

        /// <summary>
        /// Draws UI for a specific type into a container.
        /// Automatically handles fallback to default PropertyFields if no custom drawer exists.
        /// </summary>
        static void DrawUIForType(
            Type type, 
            SerializedProperty property, 
            VisualElement container,
            DrawerOptions options)
        {
            if (type == null || property == null || container == null)
                return;

            options ??= new();

            // For excluded drawers or custom options, bypass cache
            if (HasCustomOptions(options))
            {
                DrawUIForTypeUncached(type, property, container, options);
                return;
            }

            // Use cached builder for standard cases
            BuildUIForType builderDelegate = GetOrCacheUIBuilder(type);
            
            if (builderDelegate != null)
            {
                builderDelegate.Invoke(property, container, options);
            }
            else if (options.ShowDefaultFields)
            {
                // Fallback to default property fields
                CreateDefaultPropertyFields(property, container, options);
            }
        }

        static bool HasCustomOptions(DrawerOptions options)
        {
            if (options == null) return false;
            return options.ExcludeDrawerType != null || 
                   options.ExcludeDrawerTypes != null ||
                   options.ExcludeFields != null ||
                   !options.ShowDefaultFields;
        }

        /// <summary>
        /// Draws UI without using the cache (for custom options or one-off cases).
        /// </summary>
        static void DrawUIForTypeUncached(
            Type type,
            SerializedProperty property, 
            VisualElement container,
            DrawerOptions options)
        {
            bool usedCustomDrawer = TryCreateCustomDrawerUI(
                property, container, type, options);

            if (!usedCustomDrawer && options.ShowDefaultFields)
                CreateDefaultPropertyFields(property, container, options);
        }

        /// <summary>
        /// Gets or creates a cached UI builder for a type.
        /// </summary>
        static BuildUIForType GetOrCacheUIBuilder(Type type)
        {
            if (type == null) return null;

            if (s_uiBuilderCache.TryGetValue(type, out BuildUIForType cached))
                return cached;
            
            Type drawerType = PropertyDrawerRegistry.GetDrawerTypeForType(type);
            
            BuildUIForType builder;
            
            if (drawerType != null)
            {
                // Cache drawer metadata
                Type drawerTargetType = PropertyDrawerRegistry.GetDrawerTargetType(drawerType);
                HashSet<string> handledFields = PropertyDrawerRegistry.GetDrawerHandledFields(drawerType);
                
                builder = (prop, typeContainer, opts) =>
                {
                    FieldInfo fieldInfo = prop.GetFieldInfoAndStaticType(out _);
                    if (fieldInfo == null)
                    {
                        CreateDefaultPropertyFields(prop, typeContainer, opts);
                        return;
                    }
                    
                    PropertyDrawer drawer = PropertyDrawerFactory.CreateDrawer(drawerType, fieldInfo);
                    if (drawer == null)
                    {
                        CreateDefaultPropertyFields(prop, typeContainer, opts);
                        return;
                    }
                    
                    VisualElement customUI = drawer.CreatePropertyGUI(prop);
                    if (customUI != null)
                        typeContainer.Add(customUI);
                    
                    // Add additional fields if needed
                    if (drawerTargetType != null && drawerTargetType != type)
                        AddAdditionalFields(prop, typeContainer, handledFields, opts);
                };
            }
            else
            {
                builder = CreateDefaultPropertyFields;
            }
            
            s_uiBuilderCache[type] = builder;
            return builder;
        }

        #endregion

        #region Custom Drawer Support

        /// <summary>
        /// Attempts to create UI using a custom drawer (hybrid approach).
        /// </summary>
        static bool TryCreateCustomDrawerUI(
            SerializedProperty property, 
            VisualElement container,
            Type actualType,
            DrawerOptions options)
        {
            if (property == null || container == null || actualType == null)
                return false;

            PropertyDrawer drawer = PropertyDrawerFactory.CreateDrawerForProperty(
                property, 
                options?.ExcludeDrawerType, 
                options?.ExcludeDrawerTypes);
            
            if (drawer == null) return false;

            Type drawerTargetType = PropertyDrawerRegistry.GetDrawerTargetType(drawer.GetType());
            HashSet<string> fieldsHandledByDrawer = PropertyDrawerRegistry.GetDrawerHandledFields(drawer.GetType());

            VisualElement customUI = drawer.CreatePropertyGUI(property);
            if (customUI != null)
                container.Add(customUI);

            if (drawerTargetType != null && drawerTargetType != actualType)
                AddAdditionalFields(property, container, fieldsHandledByDrawer, options);

            return true;
        }

        /// <summary>
        /// Adds fields not handled by a custom drawer.
        /// </summary>
        static void AddAdditionalFields(
            SerializedProperty property,
            VisualElement container,
            HashSet<string> handledFields,
            DrawerOptions options = null)
        {
            VisualElement additionalContainer = new() { style = { marginTop = 8 } };
            bool hasFields = false;

            foreach (SerializedProperty child in property.GetChildren()
                         .Where(child => handledFields == null || !handledFields.Contains(child.name))
                         .Where(child => options?.ExcludeFields == null || !options.ExcludeFields.Contains(child.name)))
            {
                hasFields = true;
                
                PropertyDrawer childDrawer = PropertyDrawerFactory.CreateDrawerForProperty(
                    child, 
                    options?.ExcludeDrawerType, 
                    options?.ExcludeDrawerTypes);

                VisualElement customElement = childDrawer?.CreatePropertyGUI(child);
                if (customElement != null)
                {
                    additionalContainer.Add(customElement);
                    continue;
                }

                additionalContainer.Add(new PropertyField(child));
            }

            if (hasFields)
                container.Add(additionalContainer);
        }

        /// <summary>
        /// Creates default PropertyFields for all children, respecting their custom drawers.
        /// </summary>
        public static void CreateDefaultPropertyFields(
            SerializedProperty property,
            VisualElement container,
            DrawerOptions options = null)
        {
            if (property == null || container == null)
                return;

            foreach (SerializedProperty child in property.GetChildren())
            {
                // Skip if excluded by options
                if (options?.ExcludeFields != null && options.ExcludeFields.Contains(child.name))
                    continue;

                PropertyDrawer childDrawer = PropertyDrawerFactory.CreateDrawerForProperty(
                    child, 
                    options?.ExcludeDrawerType, 
                    options?.ExcludeDrawerTypes);

                VisualElement customElement = childDrawer?.CreatePropertyGUI(child);
                if (customElement != null)
                {
                    container.Add(customElement);
                    continue;
                }

                container.Add(new PropertyField(child));
            }
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// Clears the UI builder cache. Useful after hot reloads.
        /// </summary>
        public static void ClearCache()
        {
            s_uiBuilderCache?.Clear();
        }

        #endregion
    }
}
#endif