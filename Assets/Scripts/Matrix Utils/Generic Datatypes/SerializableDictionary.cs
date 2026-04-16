using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct SerializableKeyValuePair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;

    public SerializableKeyValuePair(TKey key, TValue value)
    {
        Key = key;
        Value = value;
    }
}

[Serializable]
public class SerializableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [FormerlySerializedAs("list")] 
    [SerializeField]
    List<SerializableKeyValuePair<TKey, TValue>> m_list = new();
    
    [SerializeField]
    SerializableKeyValuePair<TKey, TValue> m_stagingEntry;

    [NonSerialized]
    Dictionary<TKey, TValue> m_dictionary;
    
    [NonSerialized]
    bool m_initialized;

    public Dictionary<TKey, TValue> Dictionary 
    { 
        get 
        {
            EnsureInitialized();
            return m_dictionary;
        }
    }

    void EnsureInitialized()
    {
        if (m_initialized && m_dictionary != null) return;
        
        if (m_dictionary == null)
            m_dictionary = new();
        else
            m_dictionary.Clear();

        foreach (SerializableKeyValuePair<TKey, TValue> kvp in m_list.Where(kvp => kvp.Key != null))
        {
            m_dictionary[kvp.Key] = kvp.Value;
        }
        
        m_initialized = true;
    }

    public void Rebuild()
    {
        if (m_dictionary == null)
            m_dictionary = new();
        else
            m_dictionary.Clear();

        foreach (SerializableKeyValuePair<TKey, TValue> kvp in m_list.Where(kvp => kvp.Key != null))
        {
            m_dictionary[kvp.Key] = kvp.Value;
        }

        m_initialized = true;
    }
    
    public TValue this[TKey key]
    {
        get => Dictionary[key];
        set 
        { 
            Dictionary[key] = value;
            m_initialized = true;
        }
    }

    public ICollection<TKey> Keys => Dictionary.Keys;
    public ICollection<TValue> Values => Dictionary.Values;
    public int Count => Dictionary.Count;
    public bool IsReadOnly => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TKey key, TValue value) 
    { 
        Dictionary.Add(key, value);
        m_initialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        ((IDictionary<TKey, TValue>)Dictionary).Add(item);
        m_initialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(TKey key) 
    { 
        bool result = Dictionary.Remove(key);
        if (result) m_initialized = true;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        bool result = ((IDictionary<TKey, TValue>)Dictionary).Remove(item);
        if (result) m_initialized = true;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(KeyValuePair<TKey, TValue> item) => ((IDictionary<TKey, TValue>)Dictionary).Contains(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(TKey key, out TValue value) => Dictionary.TryGetValue(key, out value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() 
    { 
        Dictionary.Clear();
        m_initialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((IDictionary<TKey, TValue>)Dictionary).CopyTo(array, arrayIndex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator() => Dictionary.GetEnumerator();

    public void OnBeforeSerialize()
    {
        if (!m_initialized || m_dictionary is not { Count: > 0 }) return;
        m_list.Clear();
        foreach (KeyValuePair<TKey, TValue> kvp in m_dictionary)
        {
            m_list.Add(new(kvp.Key, kvp.Value));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnAfterDeserialize() => m_initialized = false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Dictionary<TKey, TValue>(SerializableDictionary<TKey, TValue> dictionary)
    {
        return dictionary.Dictionary;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator SerializableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        return new() { m_dictionary = new(dictionary), m_initialized = true }; 
    }
}