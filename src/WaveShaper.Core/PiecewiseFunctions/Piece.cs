using System;

namespace WaveShaper.Core.PiecewiseFunctions
{
    public class Piece<T>
    {
        public static readonly Piece<T> DefaultPiece = new Piece<T>
        {
            Condition = x => true,
            Function = x => default(T)
        };

        public Piece(Predicate<T> condition = null, Func<T, T> function = null)
        {
            Condition = condition;
            Function = function;
        }

        public Predicate<T> Condition { get; set; }

        public Func<T, T> Function { get; set; }
    }
}