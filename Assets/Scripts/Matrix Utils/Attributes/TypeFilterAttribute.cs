using System;
using UnityEngine;

public class TypeFilterAttribute : PropertyAttribute {
	public Func<Type, bool> Filter { get; }
	public Type FilterType { get; }

	public TypeFilterAttribute(Type filterType) {
		FilterType = filterType;
		Filter = type => !type.IsAbstract &&
		                 !type.IsInterface &&
		                 !type.IsGenericType &&
		                 type.InheritsOrImplements(filterType);
	}
}