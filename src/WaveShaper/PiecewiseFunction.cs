﻿using System;
using System.Collections.Generic;

namespace WaveShaper
{
    public class PiecewiseFunction<T>
    {
        private readonly List<Piece<T>> pieces = new List<Piece<T>>();

        public void AddPiece(Piece<T> piece)
        {
            pieces.Add(piece);
        }

        public T Calculate(T x)
        {
            foreach (var piece in pieces)
            {
                if (piece.Condition(x))
                    return piece.Function(x);
            }

            throw new Exception("calc fail");
        }
    }

    public class Piece<T>
    {
        public Predicate<T> Condition { get; set; }

        public Func<T, T> Function { get; set; }
    }


}
