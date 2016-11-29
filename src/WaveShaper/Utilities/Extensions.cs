using System;
using System.Collections.Generic;

namespace WaveShaper.Utilities
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
    }
}
