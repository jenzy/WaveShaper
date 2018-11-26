using System;
using System.Collections.Generic;

namespace WaveShaper.Core.Utilities
{
    public static class Extensions
    {
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
        {
            var c = Comparer<T>.Default;

            if (c.Compare(value, min) < 0)
                return min;

            if (c.Compare(value, max) > 0)
                return max;

            return value;
        }

        public static IEnumerable<T> AsEnumerable<T>(this Tuple<T, T, T> tuple)
        {
            yield return tuple.Item1;
            yield return tuple.Item2;
            yield return tuple.Item3;
        }
    }
}
