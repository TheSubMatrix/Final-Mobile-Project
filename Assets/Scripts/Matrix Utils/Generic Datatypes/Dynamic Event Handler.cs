using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;


public class ScriptableObjectEventListener : MonoBehaviour
{
    [SerializeField] ScriptableObject m_listener;
    [SerializeField] List<DynamicEventBinding> m_bindings = new();

    void OnEnable()
    {
        if (m_listener == null) return;
        foreach (DynamicEventBinding binding in m_bindings)
        {
            FieldInfo field = GetEventField(binding.m_eventName);
            if (field?.GetValue(m_listener) is not UnityEventBase soEvent) continue;

            Type soEventType = soEvent.GetType();
            Type[] paramTypes = Type.EmptyTypes;
            Type baseType = soEventType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(UnityEvent<>))
                {
                    paramTypes = baseType.GetGenericArguments();
                    break;
                }
                baseType = baseType.BaseType;
            }

            Type delegateType = paramTypes.Length switch
            {
                0 => typeof(UnityAction),
                1 => typeof(UnityAction<>).MakeGenericType(paramTypes),
                2 => typeof(UnityAction<,>).MakeGenericType(paramTypes),
                3 => typeof(UnityAction<,,>).MakeGenericType(paramTypes),
                4 => typeof(UnityAction<,,,>).MakeGenericType(paramTypes),
                _ => throw new NotSupportedException("UnityEvent supports max 4 parameters")
            };

            ParameterExpression[] parameters = Array.ConvertAll(paramTypes, Expression.Parameter);

            List<Expression> calls = new();
            foreach (DynamicPersistentListener listener in binding.m_listeners)
            {
                if (listener.m_target == null || string.IsNullOrEmpty(listener.m_methodName)) continue;
                MethodInfo method = listener.m_target.GetType().GetMethod(
                    listener.m_methodName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null) continue;

                if (method.GetParameters().Length == 0)
                {
                    calls.Add(Expression.Call(Expression.Constant(listener.m_target), method));
                }
                else
                {
                    ParameterInfo[] methodParams = method.GetParameters();
                    Expression[] convertedParams = parameters.Select((p, i) =>
                    {
                        Type methodParamType = methodParams[i].ParameterType;
                        if (methodParamType == p.Type) return p;

                        MethodInfo implicitOp = p.Type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                            .FirstOrDefault(m =>
                                m.Name == "op_Implicit" &&
                                m.ReturnType == methodParamType &&
                                m.GetParameters()[0].ParameterType == p.Type);

                        return implicitOp != null
                            ? (Expression)Expression.Call(implicitOp, p)
                            : Expression.Convert(p, methodParamType);
                    }).ToArray();

                    calls.Add(Expression.Call(Expression.Constant(listener.m_target), method, convertedParams));
                }
            }

            if (calls.Count == 0) continue;
            Expression body = calls.Count == 1 ? calls[0] : Expression.Block(calls);
            binding.CachedDelegate = Expression.Lambda(delegateType, body, parameters).Compile();
            soEventType.GetMethod("AddListener")?.Invoke(soEvent, new object[] { binding.CachedDelegate });
        }
    }

    void OnDisable()
    {
        if (m_listener == null) return;
        foreach (DynamicEventBinding binding in m_bindings)
        {
            if (binding.CachedDelegate == null) continue;
            FieldInfo field = GetEventField(binding.m_eventName);
            if (field?.GetValue(m_listener) is not UnityEventBase soEvent) continue;
            soEvent.GetType().GetMethod("RemoveListener")?.Invoke(soEvent, new object[] { binding.CachedDelegate });
            binding.CachedDelegate = null;
        }
    }

    FieldInfo GetEventField(string eventName)
    {
        Type type = m_listener.GetType();
        while (type != null)
        {
            FieldInfo backing = type.GetField(
                $"<{eventName}>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (backing != null) return backing;

            FieldInfo direct = type.GetField(
                eventName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (direct != null) return direct;

            type = type.BaseType;
        }
        return null;
    }
}