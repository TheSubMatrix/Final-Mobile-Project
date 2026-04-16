using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class SerializableHashSet<T> : ISet<T>, ISerializationCallbackReceiver
{
    [SerializeField]
    List<T> m_list = new();
    
    [SerializeField]
    T m_stagingValue;

    [NonSerialized]
    HashSet<T> m_hashSet;
    
    [NonSerialized]
    bool m_initialized;

    public HashSet<T> HashSet 
    { 
        get 
        {
            EnsureInitialized();
            return m_hashSet;
        }
    }

    void EnsureInitialized()
    {
        if (m_initialized && m_hashSet != null) return;
        
        if (m_hashSet == null)
            m_hashSet = new();
        else
            m_hashSet.Clear();

        foreach (T item in m_list.Where(item => item != null))
        {
            m_hashSet.Add(item);
        }
        
        m_initialized = true;
    }

    public void Rebuild()
    {
        if (m_hashSet == null)
            m_hashSet = new();
        else
            m_hashSet.Clear();

        foreach (T item in m_list.Where(item => item != null))
        {
            m_hashSet.Add(item);
        }

        m_initialized = true;
    }

    public int Count => HashSet.Count;
    public bool IsReadOnly => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T item)
    {
        bool result = HashSet.Add(item);
        if (result) m_initialized = true;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ICollection<T>.Add(T item)
    {
        if (HashSet.Add(item))
            m_initialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        bool result = HashSet.Remove(item);
        if (result) m_initialized = true;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item) => HashSet.Contains(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        HashSet.Clear();
        m_initialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array, int arrayIndex) => HashSet.CopyTo(array, arrayIndex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExceptWith(IEnumerable<T> other)
    {
        HashSet.ExceptWith(other);
        m_initialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IntersectWith(IEnumerable<T> other)
    {
        HashSet.IntersectWith(other);
        m_initialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsProperSubsetOf(IEnumerable<T> other) => HashSet.IsProperSubsetOf(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsProperSupersetOf(IEnumerable<T> other) => HashSet.IsProperSupersetOf(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSubsetOf(IEnumerable<T> other) => HashSet.IsSubsetOf(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSupersetOf(IEnumerable<T> other) => HashSet.IsSupersetOf(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Overlaps(IEnumerable<T> other) => HashSet.Overlaps(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool SetEquals(IEnumerable<T> other) => HashSet.SetEquals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        HashSet.SymmetricExceptWith(other);
        m_initialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnionWith(IEnumerable<T> other)
    {
        HashSet.UnionWith(other);
        m_initialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<T> GetEnumerator() => HashSet.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => HashSet.GetEnumerator();

    public void OnBeforeSerialize()
    {
        if (!m_initialized || m_hashSet is not { Count: > 0 }) return;
        m_list.Clear();
        foreach (T item in m_hashSet)
        {
            m_list.Add(item);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnAfterDeserialize() => m_initialized = false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator HashSet<T>(SerializableHashSet<T> hashSet)
    {
        return hashSet.HashSet;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator SerializableHashSet<T>(HashSet<T> hashSet)
    {
        return new() { m_hashSet = new(hashSet), m_initialized = true };
    }
}