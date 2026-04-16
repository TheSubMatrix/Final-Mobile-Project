using System;
using System.Text.RegularExpressions;
using UnityEngine;
[Serializable]
public class SerializableRegex : ISerializationCallbackReceiver
{
    public Regex Value{ get; private set;}
    [SerializeField] public string Pattern;
    public static implicit operator Regex(SerializableRegex regex) => regex.Value;
    public void OnBeforeSerialize()
    {
        Pattern = Value.ToString();
    }
    public void OnAfterDeserialize()
    {
        Value = new(Pattern, RegexOptions.Compiled);
    }
}
