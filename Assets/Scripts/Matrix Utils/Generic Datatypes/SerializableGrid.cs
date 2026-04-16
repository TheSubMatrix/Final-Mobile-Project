using System;
using UnityEngine;

[Serializable]
public class SerializableGrid<T> : ISerializationCallbackReceiver
{
    public T[,] Value;
    public T this[int row, int col] => Value[row, col];
    [field:SerializeField] public int Rows { get; private set; }
    [field:SerializeField] public int Columns{ get; private set;}
    [SerializeField] T[] m_flatData;
    [SerializeField, HideInInspector] int m_prevRows;
    [SerializeField, HideInInspector] int m_prevCols;

    public void OnBeforeSerialize()
    {
        if (Value == null)
        {
            m_flatData = null;
            return;
        }

        Rows = Value.GetLength(0);
        Columns = Value.GetLength(1);
        m_prevRows = Rows;
        m_prevCols = Columns;
        m_flatData = new T[Rows * Columns];

        for (int r = 0; r < Rows; r++)
        for (int c = 0; c < Columns; c++)
            m_flatData[r * Columns + c] = Value[r, c];
    }

    public void OnAfterDeserialize()
    {
        if (Rows <= 0 || Columns <= 0)
        {
            Value = null;
            return;
        }

        Value = new T[Rows, Columns];

        if (m_flatData == null) return;
        int oldCols = m_prevCols > 0 ? m_prevCols : Columns;
        int oldRows = m_prevRows > 0 ? m_prevRows : Rows;
        int copyRows = Mathf.Min(Rows, oldRows);
        int copyCols = Mathf.Min(Columns, oldCols);
        for (int r = 0; r < copyRows; r++)
        for (int c = 0; c < copyCols; c++)
        {
            int oldIndex = r * oldCols + c;
            if (oldIndex < m_flatData.Length)
                Value[r, c] = m_flatData[oldIndex];
        }
    }
}