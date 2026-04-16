using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DynamicEventBinding
{
    [SerializeField] public string m_eventName;
    [SerializeField] public List<DynamicPersistentListener> m_listeners = new();
    [NonSerialized] public Delegate CachedDelegate;
}