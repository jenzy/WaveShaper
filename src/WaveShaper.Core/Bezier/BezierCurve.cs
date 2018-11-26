using System;
using System.Collections.Generic;
using System.Windows;
using MathNet.Numerics.RootFinding;
using WaveShaper.Core.Utilities;

namespace WaveShaper.Core.Bezier
{
    public class BezierCurve
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Point P0 { get; set; }

        public Point P1 { get; set; }

        public Point P2 { get; set; }

        public Point P3 { get; set; }

        public Guid? Next { get; set; }

        public Guid? Prev { get; set; }

        public Func<double, double> GetFunctionOfX()
        {
            return x =>
            {
                bool neg = x < 0;
                x = Math.Abs(x);

                double r = SolveBezierAtX(this, x);
                return neg ? -r : r;
            };
        }

        public Tuple<BezierCurve, BezierCurve> Split(Point p)
        {
            var p0 = new CalcPoint(this.P0);
            var p1 = new CalcPoint(this.P1);
            var p2 = new CalcPoint(this.P2);
            var p3 = new CalcPoint(this.P3);
            double t = GetBezierT(this, p.X);
            double u = 1 - t;

            var c1 = new BezierCurve
            {
                P0 = p0.Point,
                P1 = ((t * p1) + (u * p0)).Point,
                P2 = ((t * t * p2) + (2 * t * u * p1) + (u * u * p0)).Point,
                P3 = ((t * t * t * p3) + (3 * t * t * u * p2) + (3 * t * u * u * p1) + (u * u * u * p0)).Point,
            };

            var c2 = new BezierCurve
            {
                P0 = (t * t * t * p3 + 3 * t * t * u * p2 + 3 * t * u * u * p1 + u * u * u * p0).Point,
                P1 = (t * t * p3 + 2 * t * u * p2 + u * u * p1).Point,
                P2 = (t * p3 + u * p2).Point,
                P3 = p3.Point,
            };

            return Tuple.Create(c1, c2);
        }

        private static double GetBezierT(BezierCurve bc, double x)
        {
            // B(t) = (1-t)^3 P0 + 3 (1-t)^2 t P1 + 3 (1-t) t^2 P2 + t^3 P3 = x
            // (-P0 + 3 P1 - 3 P2 + P3) t^3 + (3 P0 - 6 P1 + 3 P2) t^2 + (-3 P0 + 3 P1) t + (P0 - x) = 0 
            // a t^3 + b t^2 + c t + d = 0
            double a = -bc.P0.X + 3 * bc.P1.X - 3 * bc.P2.X + bc.P3.X;
            double b = 3 * bc.P0.X - 6 * bc.P1.X + 3 * bc.P2.X;
            double c = -3 * bc.P0.X + 3 * bc.P1.X;
            double d = bc.P0.X - x;

            b /= a;
            c /= a;
            d /= a;

            var roots = Cubic.RealRoots(d, c, b);

            double? root = null;
            double? rootBackup = null;

            foreach (double r in roots.AsEnumerable())
            {
                if (double.IsNaN(r))
                    continue;

                if (rootBackup == null)
                    rootBackup = r;

                if (0 <= r && r <= 1)
                {
                    root = r;
                    break;
                }
            }

            if (root == null)
                root = rootBackup;

            if (root == null)
                throw new Exception("No root found. " + roots);

            return root.Value;
        }

        private static double SolveBezierAtX(BezierCurve bc, double x)
        {
            double t = GetBezierT(bc, x);
            double tt = 1 - t;

            double y = tt * tt * tt * bc.P0.Y + 3 * tt * tt * t * bc.P1.Y + 3 * tt * t * t * bc.P2.Y + t * t * t * bc.P3.Y;
            return y;
        }

        public static IList<BezierCurve> GetIdentity()
        {
            return new List<BezierCurve>
            {
                new BezierCurve
                {
                    P0 = new Point(0, 0),
                    P1 = new Point(0.1, 0.1),
                    P2 = new Point(0.9, 0.9),
                    P3 = new Point(1.00001, 1.00001)
                }
            };
        }
    }
}
