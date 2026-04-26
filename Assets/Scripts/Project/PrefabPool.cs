using System;
using MatrixUtils.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

[Serializable]
public class PrefabPool<T> : IObjectPool<T> where T : MonoBehaviour
{
    [field: SerializeField, RequiredField] public T Prefab { get; private set; }
    ObjectPool<T> m_pool;
    bool m_initialized;
    InvalidOperationException m_notInitialized = new("Pool was not initialized");

    public void Initialize(
        Func<T> createFunc = null,
        Action<T> actionOnGet = null,
        Action<T> actionOnRelease = null,
        Action<T> actionOnDestroy = null,
        bool collectionCheck = true,
        int defaultCapacity = 10,
        int maxSize = 10000)
    {
        m_pool = new(createFunc ?? (() =>
            {
                T obj = Object.Instantiate(Prefab);
                obj.gameObject.SetActive(false);
                return obj;
            }),
            actionOnGet ?? (obj => obj.gameObject.SetActive(true)),
            actionOnRelease ?? (obj => obj.gameObject.SetActive(false)),
            actionOnDestroy,
            collectionCheck,
            defaultCapacity,
            maxSize
        );
        m_initialized = true;
    }

    public T DefaultCreate()
    {
        T instantiated = Object.Instantiate(Prefab);
        instantiated.gameObject.SetActive(false);
        return instantiated;
    }

    public T Get()
    {
        return m_initialized ? m_pool.Get() : throw m_notInitialized;
    }

    public PooledObject<T> Get(out T v)
    {
        return m_initialized ? m_pool.Get(out v) : throw m_notInitialized;
    }

    public void Release(T element)
    {
        if (!m_initialized) throw m_notInitialized;
        m_pool.Release(element);
    }

    public void Clear()
    {
        if (!m_initialized) throw m_notInitialized;
        m_pool.Clear();
    }

    public int CountInactive => m_initialized ? m_pool.CountInactive : throw m_notInitialized;
}