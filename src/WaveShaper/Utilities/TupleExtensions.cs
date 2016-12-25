using System;
using System.Collections.Generic;

namespace WaveShaper.Utilities
{
    public static class TupleExtensions
    {
        public static IEnumerable<T> AsEnumerable<T>(this Tuple<T, T, T> tuple)
        {
            yield return tuple.Item1;
            yield return tuple.Item2;
            yield return tuple.Item3;
        }
    }
}
