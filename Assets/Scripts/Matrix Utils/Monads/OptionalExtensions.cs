using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

public static class OptionalExtensions
{
    [Pure]
    public static Optional<TResult> SelectMany<T, TCollection, TResult>(
        this Optional<T> optional,
        Func<T, Optional<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector)
    {
        if (!optional.HasValue) return Optional<TResult>.NoValue;
        Optional<TCollection> collection = collectionSelector(optional.Value);
        return collection.HasValue 
            ? Optional<TResult>.Some(resultSelector(optional.Value, collection.Value))
            : Optional<TResult>.NoValue;
    }
    
    [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Optional<T> Flatten<T>(this Optional<Optional<T>> nested) => nested.HasValue ? nested.Value : Optional<T>.NoValue;
    
    [Pure]
    public static Optional<T> FirstOrNone<T>(this IEnumerable<T> source)
    {
        foreach (T item in source)
            return Optional<T>.Some(item);
        return Optional<T>.None();
    }
    [Pure]
    public static Optional<T> FirstOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        foreach (T item in source)
            if (predicate(item))
                return Optional<T>.Some(item);
        return Optional<T>.None();
    }
}