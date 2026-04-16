#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using MatrixUtils.Extensions;
using UnityEditor;
using UnityEngine;

namespace MatrixUtils.PropertyDrawers.Helpers
{
/// <summary>
    /// Options for controlling IMGUI drawer behavior.
    /// Shared by all IMGUI drawer classes.
    /// </summary>
    public class IMGUIDrawerOptions
    {
        public Type ExcludeDrawerType { get; set; }
        public HashSet<Type> ExcludeDrawerTypes { get; set; }
        public HashSet<string> ExcludeFields { get; set; }
        public bool ShowDefaultFields { get; set; } = true;
    }

    /// <summary>
    /// Factory for drawing properties in IMGUI with automatic custom drawer resolution and caching.
    /// Mirrors VisualElementFactory for IMGUI contexts.
    /// </summary>
    public static class PropertyDrawerIMGUIFactory
    {
        // Delegate for cached drawer functions
        delegate float DrawPropertyDelegate(Rect position, SerializedProperty property, IMGUIDrawerOptions options);
        delegate float GetHeightDelegate(SerializedProperty property, IMGUIDrawerOptions options);

        static readonly Dictionary<Type, DrawPropertyDelegate> s_drawPropertyCache;
        static readonly Dictionary<Type, GetHeightDelegate> s_getHeightCache;

        static PropertyDrawerIMGUIFactory()
        {
            s_drawPropertyCache = new();
            s_getHeightCache = new();
        }

        #region Core Drawing Methods

        /// <summary>
        /// Draws UI for a property into a rect with automatic fallback.
        /// </summary>
        public static float DrawProperty(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            IMGUIDrawerOptions options = null)
        {
            if (property == null) return 0f;

            property.GetFieldInfoAndStaticType(out Type fieldType);
            if (fieldType == null)
            {
                // Fallback to default
                EditorGUI.PropertyField(position, property, label, true);
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            options ??= new IMGUIDrawerOptions();

            // For excluded drawers or custom options, bypass cache
            if (HasCustomOptions(options))
            {
                return DrawPropertyUncached(position, property, label, fieldType, options);
            }

            // Use cached drawer delegate
            DrawPropertyDelegate drawDelegate = GetOrCacheDrawer(fieldType);
            return drawDelegate?.Invoke(position, property, options) ?? 0f;
        }

        /// <summary>
        /// Draws all child properties of a SerializedProperty or SerializedObject.
        /// </summary>
        public static float DrawChildProperties(
            Rect position,
            SerializedProperty property,
            IMGUIDrawerOptions options = null)
        {
            if (property == null) return 0f;

            options ??= new IMGUIDrawerOptions();

            float yOffset = 0f;
            SerializedProperty iterator = property.Copy();

            // Move to first visible property
            if (!iterator.NextVisible(true))
                return yOffset;

            do
            {
                // Skip excluded fields
                if (options.ExcludeFields != null && options.ExcludeFields.Contains(iterator.name))
                    continue;

                SerializedProperty childProperty = iterator.Copy();
                float height = GetPropertyHeight(childProperty, options);

                Rect fieldRect = new Rect(
                    position.x,
                    position.y + yOffset,
                    position.width,
                    height);

                DrawProperty(fieldRect, childProperty, new GUIContent(childProperty.displayName), options);

                yOffset += height + EditorGUIUtility.standardVerticalSpacing;
            }
            while (iterator.NextVisible(false));

            return yOffset;
        }
        
        /// <summary>
        /// Draws all properties from a SerializedObject.
        /// </summary>
        public static float DrawSerializedObject(
            Rect position,
            SerializedObject serializedObject,
            IMGUIDrawerOptions options = null)
        {
            if (serializedObject == null) return 0f;

            SerializedProperty iterator = serializedObject.GetIterator();
            return DrawChildProperties(position, iterator, options);
        }

        static bool HasCustomOptions(IMGUIDrawerOptions options)
        {
            if (options == null) return false;
            return options.ExcludeDrawerType != null ||
                   options.ExcludeDrawerTypes != null ||
                   options.ExcludeFields != null ||
                   !options.ShowDefaultFields;
        }

        static float DrawPropertyUncached(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            Type fieldType,
            IMGUIDrawerOptions options)
        {
            PropertyDrawer drawer = PropertyDrawerFactory.CreateDrawerForProperty(
                property,
                options.ExcludeDrawerType,
                options.ExcludeDrawerTypes);

            if (drawer != null || options.ShowDefaultFields)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return 0f;
        }

        static DrawPropertyDelegate GetOrCacheDrawer(Type type)
        {
            if (type == null) return null;

            if (s_drawPropertyCache.TryGetValue(type, out DrawPropertyDelegate cached))
                return cached;

            Type drawerType = PropertyDrawerRegistry.GetDrawerTypeForType(type);

            DrawPropertyDelegate drawer;

            if (drawerType != null)
            {
                // Has custom drawer
                drawer = (pos, prop, opts) =>
                {
                    GUIContent label = new GUIContent(prop.displayName);
                    EditorGUI.PropertyField(pos, prop, label, true);
                    return EditorGUI.GetPropertyHeight(prop, label, true);
                };
            }
            else
            {
                // No custom drawer - use default
                drawer = (pos, prop, opts) =>
                {
                    GUIContent label = new GUIContent(prop.displayName);
                    EditorGUI.PropertyField(pos, prop, label, true);
                    return EditorGUI.GetPropertyHeight(prop, label, true);
                };
            }

            s_drawPropertyCache[type] = drawer;
            return drawer;
        }

        #endregion

        #region Height Calculation

        /// <summary>
        /// Gets the height needed for a property (considering custom drawers).
        /// </summary>
        public static float GetPropertyHeight(
            SerializedProperty property,
            IMGUIDrawerOptions options = null)
        {
            if (property == null) return EditorGUIUtility.singleLineHeight;

            property.GetFieldInfoAndStaticType(out Type fieldType);
            if (fieldType == null)
            {
                return EditorGUI.GetPropertyHeight(property, true);
            }

            options ??= new IMGUIDrawerOptions();

            // For excluded drawers, bypass cache
            if (HasCustomOptions(options))
            {
                return EditorGUI.GetPropertyHeight(property, true);
            }

            // Use cached height delegate
            GetHeightDelegate heightDelegate = GetOrCacheHeight(fieldType);
            return heightDelegate?.Invoke(property, options) ?? EditorGUIUtility.singleLineHeight;
        }

        /// <summary>
        /// Gets the total height for all child properties.
        /// </summary>
        public static float GetChildPropertiesHeight(
            SerializedProperty property,
            IMGUIDrawerOptions options = null)
        {
            if (property == null) return 0f;

            options ??= new IMGUIDrawerOptions();

            float totalHeight = 0f;
            SerializedProperty iterator = property.Copy();

            if (!iterator.NextVisible(true))
                return totalHeight;

            do
            {
                // Skip excluded fields
                if (options.ExcludeFields != null && options.ExcludeFields.Contains(iterator.name))
                    continue;

                SerializedProperty childProperty = iterator.Copy();
                totalHeight += GetPropertyHeight(childProperty, options) + EditorGUIUtility.standardVerticalSpacing;
            }
            while (iterator.NextVisible(false));

            return totalHeight;
        }
        
        /// <summary>
        /// Gets the total height for all properties in a SerializedObject.
        /// </summary>
        public static float GetSerializedObjectHeight(
            SerializedObject serializedObject,
            IMGUIDrawerOptions options = null)
        {
            if (serializedObject == null) return 0f;

            SerializedProperty iterator = serializedObject.GetIterator();
            return GetChildPropertiesHeight(iterator, options);
        }

        static GetHeightDelegate GetOrCacheHeight(Type type)
        {
            if (type == null) return null;

            if (s_getHeightCache.TryGetValue(type, out GetHeightDelegate cached))
                return cached;

            Type drawerType = PropertyDrawerRegistry.GetDrawerTypeForType(type);

            GetHeightDelegate heightDelegate = (prop, opts) =>
            {
                return EditorGUI.GetPropertyHeight(prop, true);
            };

            s_getHeightCache[type] = heightDelegate;
            return heightDelegate;
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// Clears all cached drawer delegates. Useful after hot reloads.
        /// </summary>
        public static void ClearCache()
        {
            s_drawPropertyCache?.Clear();
            s_getHeightCache?.Clear();
        }

        #endregion
    }
}
#endif