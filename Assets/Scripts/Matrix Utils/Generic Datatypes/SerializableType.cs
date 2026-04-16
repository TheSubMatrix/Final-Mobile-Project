using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class SerializableType : ISerializationCallbackReceiver {
	[FormerlySerializedAs("assemblyQualifiedName")] [SerializeField] string m_assemblyQualifiedName = string.Empty;

	Type m_type;

	public Type Type
	{
		get => m_type;
		private set => m_type = value;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() {
		m_assemblyQualifiedName = Type?.AssemblyQualifiedName ?? m_assemblyQualifiedName;
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (string.IsNullOrEmpty(m_assemblyQualifiedName)) {
			Type = null;
			return;
		}

		if (!TryGetType(m_assemblyQualifiedName, out Type type)) {
			Debug.LogError($"Type {m_assemblyQualifiedName} not found");
			return;
		}
		Type = type;
	}

	static bool TryGetType(string typeString, out Type type) {
		type = Type.GetType(typeString);
		return type != null;
	}

	public static implicit operator Type(SerializableType sType) => sType?.Type;
	public static implicit operator SerializableType(Type type) => new() { Type = type };
	public override bool Equals(object obj)
	{
		return obj switch
		{
			SerializableType other => Type == other.Type,
			Type type => Type == type,
			_ => false
		};
	}

	protected bool Equals(SerializableType other)
	{
		return m_assemblyQualifiedName == other.m_assemblyQualifiedName && Type == other.Type;
	}

	public override int GetHashCode()
	{
		return Type?.GetHashCode() ?? 0;
	}
}