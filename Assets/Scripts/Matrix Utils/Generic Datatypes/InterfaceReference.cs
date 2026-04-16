using UnityEngine;
using System;
using Object = UnityEngine.Object;
[Serializable]
public class InterfaceReference<TInterface, TObject> where TObject : Object where TInterface : class
{
	[SerializeField, HideInInspector] TObject m_underlyingValue;

	public TInterface Value
	{
		get => m_underlyingValue switch
		{
			null => null,
			TInterface @interface => @interface,
			_ => throw new InvalidOperationException(
				$"{m_underlyingValue} needs to implement interface {nameof(TInterface)}")
		};
		set => m_underlyingValue = value switch
		{
			null => null,
			TObject newValue => newValue,
			_ => throw new ArgumentException($"{value} needs to be of type {typeof(TObject)}.", string.Empty)
		};
	}

	public TObject UnderlyingValue
	{
		get => m_underlyingValue;
		set => m_underlyingValue = value;
	}

	public InterfaceReference() { }
	public InterfaceReference(TInterface @interface) => m_underlyingValue = @interface as TObject;
	public InterfaceReference(TObject target) => m_underlyingValue = target;
}

[Serializable]
public class InterfaceReference<TInterface> : InterfaceReference<TInterface, Object> where TInterface : class{}