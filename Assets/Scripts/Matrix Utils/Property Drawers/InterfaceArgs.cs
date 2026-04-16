using System;
using UnityEngine;
using Object = UnityEngine.Object;

public struct InterfaceArgs {
	public readonly Type ObjectType;
	public readonly Type InterfaceType;
	public readonly bool IsValid;

	public InterfaceArgs(Type objectType, Type interfaceType) {
		ObjectType = objectType;
		InterfaceType = interfaceType;
		IsValid = objectType != null && interfaceType != null;

		if (!IsValid) return;
		Debug.Assert(typeof(Object).IsAssignableFrom(objectType),
			$"{nameof(objectType)} needs to be of Type {typeof(Object)}.");
		Debug.Assert(interfaceType.IsInterface,
			$"{nameof(interfaceType)} needs to be an interface.");
	}
}