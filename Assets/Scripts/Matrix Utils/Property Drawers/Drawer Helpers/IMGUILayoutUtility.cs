#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MatrixUtils.PropertyDrawers.Helpers
{
/// <summary>
    /// Layout utilities for IMGUI drawing.
    /// Provides common rect calculations and UI patterns.
    /// </summary>
    public static class IMGUILayoutUtility
    {
        /// <summary>
        /// Draws a foldout header with content area calculation.
        /// Returns true if foldout is expanded.
        /// </summary>
        public static bool DrawFoldoutHeader(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            out Rect contentRect,
            bool shouldShowFoldout = true)
        {
            Rect foldoutRect = new Rect(
                position.x,
                position.y,
                EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight);

            if (shouldShowFoldout)
            {
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            }
            else
            {
                // Fake foldout that looks like a label
                foldoutRect.x += 12;
                EditorGUI.Foldout(foldoutRect, false, label, true, EditorStyles.label);
            }

            // Calculate content rect (area after label)
            Rect indentedPosition = EditorGUI.IndentedRect(position);
            float indentOffset = indentedPosition.x - position.x;

            contentRect = new Rect(
                position.x + (EditorGUIUtility.labelWidth - indentOffset),
                position.y,
                position.width - (EditorGUIUtility.labelWidth - indentOffset),
                EditorGUIUtility.singleLineHeight);

            return property.isExpanded;
        }

        /// <summary>
        /// Creates a content rect with space reserved for a button.
        /// </summary>
        public static Rect GetContentRectWithButton(
            Rect position,
            float buttonWidth,
            out Rect buttonRect)
        {
            Rect indentedPosition = EditorGUI.IndentedRect(position);
            float indentOffset = indentedPosition.x - position.x;

            Rect contentRect = new Rect(
                position.x + (EditorGUIUtility.labelWidth - indentOffset),
                position.y,
                position.width - (EditorGUIUtility.labelWidth - indentOffset) - buttonWidth,
                EditorGUIUtility.singleLineHeight);

            buttonRect = new Rect(
                position.x + position.width - buttonWidth,
                position.y,
                buttonWidth,
                EditorGUIUtility.singleLineHeight);

            return contentRect;
        }

        /// <summary>
        /// Draws a background box for expanded content.
        /// </summary>
        public static void DrawExpandedBackground(Rect position, float yStart, float height)
        {
            Rect boxRect = new Rect(
                0,
                yStart,
                Screen.width,
                height);

            GUI.Box(boxRect, "", EditorStyles.helpBox);
        }

        /// <summary>
        /// Creates a rect for a field at a specific vertical offset.
        /// </summary>
        public static Rect GetFieldRect(Rect position, float yOffset, float height)
        {
            return new Rect(
                position.x,
                position.y + yOffset,
                position.width,
                height);
        }

        /// <summary>
        /// Gets the content rect after the label, accounting for indentation.
        /// </summary>
        public static Rect GetContentRect(Rect position)
        {
            Rect indentedPosition = EditorGUI.IndentedRect(position);
            float indentOffset = indentedPosition.x - position.x;

            return new Rect(
                position.x + (EditorGUIUtility.labelWidth - indentOffset),
                position.y,
                position.width - (EditorGUIUtility.labelWidth - indentOffset),
                EditorGUIUtility.singleLineHeight);
        }
    }
}
#endif