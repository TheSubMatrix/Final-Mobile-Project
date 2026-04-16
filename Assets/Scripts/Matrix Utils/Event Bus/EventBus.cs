namespace MatrixUtils.EventBus
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class EventBus<T> where T : IEvent
    {
        static readonly HashSet<IEventBinding<T>> s_bindings = new HashSet<IEventBinding<T>>();
        static readonly List<IEventBinding<T>> s_buffer = new List<IEventBinding<T>>();

        public static void Register(IEventBinding<T> binding) => s_bindings.Add(binding);
        public static void Deregister(IEventBinding<T> binding) => s_bindings.Remove(binding);

        public static void Raise(T @event)
        {
            s_buffer.AddRange(s_bindings);
            try
            {
                foreach (IEventBinding<T> binding in s_buffer)
                {
                    binding.OnEvent.Invoke(@event);
                    binding.OnEventNoArgs.Invoke();
                }
            }
            finally
            {
                s_buffer.Clear();
            }
        }

        static void Clear()
        {
            Debug.Log($"Clearing all bindings for {typeof(T).Name}");
            s_bindings.Clear();
        }
    }
}