using System.Diagnostics.Contracts;

namespace devicesConnector;

public static class Extensions
{
    [Pure]
    public static bool IsEither<T>(this T obj, IEnumerable<T> variants)
    {
        return IsEither(obj, variants, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// Determines whether an object is equal to any of the elements in a sequence.
    /// </summary>
    [Pure]
    public static bool IsEither<T>(this T obj, IEnumerable<T> variants,
        IEqualityComparer<T> comparer)
    {
        //variants.GuardNotNull(nameof(variants));
        //comparer.GuardNotNull(nameof(comparer));

        return variants.Contains(obj, comparer);
    }

    [Pure]
    public static bool IsEither<T>(this T obj, params T[] variants)
    {
        return IsEither(obj, (IEnumerable<T>)variants);
    }
}