using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

[Serializable, PublicAPI]
public struct Optional<T> : IEnumerable<T>
{
    public static readonly Optional<T> NoValue;
    [SerializeField] T m_value;
    
    public Optional([DisallowNull]T value)
    {
        m_value = value;
        m_hasValue = true;
    }
    
    [System.Diagnostics.Contracts.Pure]
    public T Value => HasValue ? m_value : throw new InvalidOperationException("No Value");

    public bool HasValue => m_hasValue;
    [SerializeField] bool m_hasValue;
    public Optional<T> Do([InstantHandle] Action<T> action) { if (HasValue) action(m_value); return this; }
    
    public Optional<T> DoIfNone([InstantHandle] Action action) { if (!HasValue) action(); return this; }
    
    public Optional<T> DoWhen(Predicate<T> predicate, [InstantHandle] Action action) 
    { 
        if (HasValue && predicate(m_value)) action(); 
        return this; 
    }

    [System.Diagnostics.Contracts.Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue([MaybeNullWhen(false), NotNullWhen(true)] out T value)
    {
        value = m_value;
        return HasValue;
    }
    
    [return: MaybeNull]
    [System.Diagnostics.Contracts.Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOrDefault() => m_value;
    
    [return: MaybeNull]
    [System.Diagnostics.Contracts.Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValueOrDefault([AllowNull] T defaultValue) => HasValue ? m_value : defaultValue;
    
    [System.Diagnostics.Contracts.Pure]
    public T GetValueOrCreate([InstantHandle]Func<T> defaultFactory) => HasValue ? m_value : defaultFactory();
    
    [System.Diagnostics.Contracts.Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Optional<T> Where(Predicate<T> predicate) => 
        HasValue && predicate(m_value) ? this : NoValue;
    
    [System.Diagnostics.Contracts.Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Optional<T> Or(Optional<T> alternative) => HasValue ? this : alternative;
    
    [System.Diagnostics.Contracts.Pure]
    public Optional<T> Or(Func<Optional<T>> alternativeFactory) => HasValue ? this : alternativeFactory();
    
    [System.Diagnostics.Contracts.Pure]
    public Optional<TResult> Select<TResult>([InstantHandle]Func<T, TResult> map) => 
        HasValue ? new(map(m_value)) : Optional<TResult>.NoValue;
    
    [System.Diagnostics.Contracts.Pure]
    public Optional<TResult> SelectMany<TResult>([InstantHandle]Func<T, Optional<TResult>> bind) => 
        HasValue ? bind(m_value) : Optional<TResult>.NoValue;

    [System.Diagnostics.Contracts.Pure]
    public Optional<TResult> Combine<TOther, TResult>(Optional<TOther> other, [InstantHandle] Func<T, TOther, TResult> combiner) => 
        Combine(this, other, combiner);
    
    [System.Diagnostics.Contracts.Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone) => 
        HasValue ? onSome(m_value) : onNone();
    
    [System.Diagnostics.Contracts.Pure]
    public IEnumerator<T> GetEnumerator()
    {
        if (HasValue) yield return m_value;
    }
    
    [System.Diagnostics.Contracts.Pure]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    [System.Diagnostics.Contracts.Pure, MustUseReturnValue, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Optional<T> Some([DisallowNull]T value) => new(value);
    
    [System.Diagnostics.Contracts.Pure, MustUseReturnValue, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Optional<T> None() => NoValue;
    
    [System.Diagnostics.Contracts.Pure, MustUseReturnValue, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Optional<T> OfNullable(T value) => value == null ? NoValue : new(value);
    
    [System.Diagnostics.Contracts.Pure]
    public static Optional<TResult> Combine<T1, T2, TResult>(
        Optional<T1> first, 
        Optional<T2> second, 
        [InstantHandle] Func<T1, T2, TResult> combiner) => 
        first.HasValue && second.HasValue 
            ? new(combiner(first.Value, second.Value)) 
            : Optional<TResult>.NoValue;

    [System.Diagnostics.Contracts.Pure]
    public override bool Equals(object obj) => obj is Optional<T> other && Equals(other);
    
    [System.Diagnostics.Contracts.Pure]
    public bool Equals(Optional<T> other) => 
        !HasValue ? !other.HasValue : EqualityComparer<T>.Default.Equals(m_value, other.m_value);
    
    [System.Diagnostics.Contracts.Pure]
    public override int GetHashCode() => 
        !HasValue ? 0 : (HasValue.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(m_value);
    
    [System.Diagnostics.Contracts.Pure]
    public override string ToString() => HasValue ? $"Some({m_value})" : "None";

    [System.Diagnostics.Contracts.Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Optional<T>(T value) => new(value);
    
    [System.Diagnostics.Contracts.Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(Optional<T> value) => value.HasValue;
    
    [System.Diagnostics.Contracts.Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator T(Optional<T> value) => value.m_value;
}