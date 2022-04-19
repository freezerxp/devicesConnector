using System.Diagnostics.Contracts;

namespace devicesConnector.Helpers;

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
    

        return variants.Contains(obj, comparer);
    }

    [Pure]
    public static bool IsEither<T>(this T obj, params T[] variants)
    {
        return IsEither(obj, (IEnumerable<T>)variants);
    }

    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    /// <summary>
    /// Конвертация в битовый массив
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    /// <example>5 = [1,0,1,0,0,0,0,0] (реверс от 00000101)</example>
    public static int[] ToBitsArray(this int x, int width = 8)
    {
        var s = Convert.ToString(x, 2);

        var bits = s.PadLeft(width, '0')
            .Select(c => int.Parse(c.ToString()))
            .Reverse()
            .ToArray();

        return bits;
    }
}