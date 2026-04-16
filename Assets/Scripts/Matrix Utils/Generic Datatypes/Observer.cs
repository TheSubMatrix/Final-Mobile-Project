using System;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor.Events;
#endif


namespace MatrixUtils.GenericDatatypes
{
    [Serializable]
    public class Observer<T>
    {
        public Observer(T value, UnityAction<T> callback = null)
        {
            m_onValueChanged = new();
            if (callback is not null) m_onValueChanged.AddListener(callback);
            Value = value;
        }

        public Observer()
        {
            m_onValueChanged = new();
            m_value = default;
        }
        
        [SerializeField] T m_value;
        [SerializeField] UnityEvent<T> m_onValueChanged;

        public UnityEvent<T> GetUnderlyingUnityEvent()
        {
            return m_onValueChanged;
        }
        public T Value
        {
            get => m_value;
            set => Set(value);
        }

        public override string ToString()
        {
	        return $"Observer<{typeof(T).Name}>: {m_value?.ToString() ?? "Null"}";
        }
        public static implicit operator T(Observer<T> observer) => observer.Value;


        public void SetValueWithoutNotify(T value)
        {
            m_value = value;
        }

        void Set(T value)
        {
            if (Equals(m_value, value)) return;
            m_value = value;
            Notify();
        }

        public void Notify()
        {
            m_onValueChanged?.Invoke(m_value);
        }

        public void AddListener(UnityAction<T> callback)
        {
            if (callback is null) return;
            m_onValueChanged ??= new();
            m_onValueChanged.AddListener(callback);
        }

        public void RemoveListener(UnityAction<T> callback)
        {
            if (callback is null) return;
            m_onValueChanged ??= new();
            m_onValueChanged.RemoveListener(callback);

        }

        public void RemoveAllListeners()
        {
            m_onValueChanged.RemoveAllListeners();
        }

        public void Dispose()
        {
            RemoveAllListeners();
            m_onValueChanged = null;
            m_value = default;
        }
    }
}
