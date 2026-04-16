#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace MatrixUtils.PropertyDrawers.Helpers
{
    public static class IMGUIControlFactory
    {
        /// <summary>
        /// Draws an ObjectField with proper change tracking and indentation.
        /// </summary>
        public static UnityEngine.Object DrawObjectField(
            Rect position,
            SerializedProperty property,
            Type objectType,
            GUIContent label = null)
        {
            EditorGUI.BeginChangeCheck();

            UnityEngine.Object result = label != null 
                ? EditorGUI.ObjectField(position, label, property.objectReferenceValue, objectType, false) 
                : EditorGUI.ObjectField(position, property.objectReferenceValue, objectType, false);

            if (!EditorGUI.EndChangeCheck()) return result;
            property.objectReferenceValue = result;
            property.serializedObject.ApplyModifiedProperties();

            return result;
        }

        /// <summary>
        /// Draws a button at the specified position.
        /// Returns true if the button was clicked.
        /// </summary>
        public static bool DrawButton(Rect position, string text)
        {
            return GUI.Button(position, text);
        }

        /// <summary>
        /// Draws a button with custom GUIContent.
        /// Returns true if the button was clicked.
        /// </summary>
        public static bool DrawButton(Rect position, GUIContent content)
        {
            return GUI.Button(position, content);
        }

        /// <summary>
        /// Draws a label at the specified position.
        /// </summary>
        public static void DrawLabel(Rect position, string text, GUIStyle style = null)
        {
            if (style != null)
            {
                GUI.Label(position, text, style);
            }
            else
            {
                GUI.Label(position, text);
            }
        }

        /// <summary>
        /// Draws a label with custom GUIContent.
        /// </summary>
        public static void DrawLabel(Rect position, GUIContent content, GUIStyle style = null)
        {
            if (style != null)
            {
                GUI.Label(position, content, style);
            }
            else
            {
                GUI.Label(position, content);
            }
        }
    }
}
#endif