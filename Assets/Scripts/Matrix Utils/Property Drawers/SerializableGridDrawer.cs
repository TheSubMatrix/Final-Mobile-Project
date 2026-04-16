#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SerializableGrid<>), true)]
public class SerializableGridDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement gridContainer = new();
        SerializedProperty rowsProp = property.FindPropertyRelative("<Rows>k__BackingField");
        SerializedProperty colsProp = property.FindPropertyRelative("<Columns>k__BackingField");
        SerializedProperty flatProp = property.FindPropertyRelative("m_flatData");
        Foldout foldout = new()
        {
            text  = property.displayName,
            value = property.isExpanded
        };
        foldout.RegisterValueChangedCallback(e => property.isExpanded = e.newValue);
        VisualElement dimRow = new()
        {
            style = { flexDirection = FlexDirection.Row, marginBottom = 4 }
        };
        IntegerField rowsField = new("Rows")
        {
            bindingPath = rowsProp.propertyPath,
            style = { flexGrow = 1 }
        };

        IntegerField colsField = new("Columns")
        {
            bindingPath = colsProp.propertyPath,
            style = { flexGrow = 1 }
        };

        dimRow.Add(rowsField);
        dimRow.Add(colsField);
        foldout.Add(dimRow);
        foldout.Add(gridContainer);
        foldout.TrackPropertyValue(rowsProp, _ => RebuildGrid());
        foldout.TrackPropertyValue(colsProp, _ => RebuildGrid());

        RebuildGrid();
        return foldout;
        
        void RebuildGrid()
        {
            gridContainer.Clear();
            property.serializedObject.Update();

            int rows = rowsProp.intValue;
            int cols = colsProp.intValue;

            if (rows <= 0 || cols <= 0)
            {
                gridContainer.Add(new Label("Set Rows and Columns...") {
                    style = { unityFontStyleAndWeight = FontStyle.Italic, color = Color.gray, paddingTop = 4 }
                });
                return;
            }

            if (flatProp.arraySize != rows * cols)
            {
                flatProp.arraySize = rows * cols;
                property.serializedObject.ApplyModifiedProperties();
            }
            VisualElement headerRow = MakeRow();
            headerRow.Add(MakeLabel(string.Empty, 40)); 
            for (int c = 0; c < cols; c++)
            {
                Label lbl = MakeLabel($"[{c}]", bold: true);
                lbl.style.minWidth = 40;
                headerRow.Add(lbl);
            }
            gridContainer.Add(headerRow);
            for (int r = 0; r < rows; r++)
            {
                VisualElement dataRow = MakeRow();
                dataRow.Add(MakeLabel($"[{r}]", 40));

                for (int c = 0; c < cols; c++)
                {
                    int idx = r * cols + c;
                    SerializedProperty cell = flatProp.GetArrayElementAtIndex(idx);
                    
                    PropertyField field = new(cell, string.Empty)
                    {
                        style = 
                        { 
                            flexGrow = 1, 
                            flexBasis = 0, 
                            minWidth = 40,
                            marginLeft = 2,
                            marginRight = 2
                        }
                    };
                    
                    field.RegisterCallback<GeometryChangedEvent>(_ => {
                        Label label = field.Q<Label>();
                        if (label != null) label.style.width = 0;
                    });
                    
                    dataRow.Add(field);
                }
                gridContainer.Add(dataRow);
            }
            gridContainer.Bind(property.serializedObject);
        }
    }

    static VisualElement MakeRow() => new()
    {
        style = { flexDirection = FlexDirection.Row, marginBottom = 2, alignItems = Align.Center }
    };

    static Label MakeLabel(string text, int fixedWidth = 0, bool bold = false)
    {
        Label lbl = new(text)
        {
            style =
            {
                unityTextAlign = TextAnchor.MiddleCenter,
                color = Color.gray,
                fontSize = 10,
                marginLeft = 2,
                marginRight = 2,
                unityFontStyleAndWeight = bold ? FontStyle.Bold : FontStyle.Normal
            }
        };

        if (fixedWidth > 0)
        {
            lbl.style.width = fixedWidth;
            lbl.style.flexGrow = 0;
            lbl.style.flexShrink = 0;
        }
        else
        {
            lbl.style.flexGrow = 1;
            lbl.style.flexBasis = 0;
            lbl.style.overflow = Overflow.Hidden;
        }
        return lbl;
    }
}
#endif