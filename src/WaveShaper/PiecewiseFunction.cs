using System;
using System.Collections.Generic;

namespace WaveShaper
{
    public class PiecewiseFunction<T>
    {
        private readonly List<Piece<T>> pieces = new List<Piece<T>>();

        public Func<T, T> Preprocess { get; set; }

        public void AddPiece(Piece<T> piece)
        {
            pieces.Add(piece);
        }

        public void AddPieces(IEnumerable<Piece<T>> piecesToAdd)
        {
            pieces.AddRange(piecesToAdd);
        }

        public T Calculate(T x)
        {
            if (Preprocess != null)
                x = Preprocess(x);

            foreach (var piece in pieces)
            {
                if (piece.Condition(x))
                    return piece.Function(x);
            }

            throw new PiecewiseFunctionInputOutOfRange(nameof(x), x, null);
        }
    }

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

    public class PiecewiseFunctionInputOutOfRange : ArgumentOutOfRangeException
    {
        public PiecewiseFunctionInputOutOfRange()
        {
        }

        public PiecewiseFunctionInputOutOfRange(string paramName) : base(paramName)
        {
        }

        public PiecewiseFunctionInputOutOfRange(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
        {
        }
    }
}
