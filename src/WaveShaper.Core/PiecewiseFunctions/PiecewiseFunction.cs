using System;
using System.Collections.Generic;

namespace WaveShaper.Core.PiecewiseFunctions
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
}
