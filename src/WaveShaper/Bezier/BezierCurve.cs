using System;
using System.Diagnostics;
using System.Windows;
using MathNet.Numerics.RootFinding;
using WaveShaper.Utilities;

namespace WaveShaper.Bezier
{
    public class BezierCurve
    {
        public Point P0 { get; set; }

        public Point P1 { get; set; }

        public Point P2 { get; set; }

        public Point P3 { get; set; }

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

        private static double SolveBezierAtX(BezierCurve bc, double x)
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

            //Debug.WriteLine($"  ROOTS {roots}");

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
            {
                root = rootBackup;
                Debug.WriteLine("Taking backup root " + rootBackup);
            }

            if (root == null)
                throw new Exception("No root found. " + roots);

            double t = root.Value;
            double tt = 1 - t;

            double y = tt * tt * tt * bc.P0.Y + 3 * tt * tt * t * bc.P1.Y + 3 * tt * t * t * bc.P2.Y + t * t * t * bc.P3.Y;
            return y;
        }
    }
}
