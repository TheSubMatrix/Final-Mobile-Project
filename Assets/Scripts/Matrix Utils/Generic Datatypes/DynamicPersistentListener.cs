using System;
using UnityEngine;

[Serializable]
public class DynamicPersistentListener
{
    [SerializeField] public UnityEngine.Object m_target;
    [SerializeField] public string m_methodName;
}