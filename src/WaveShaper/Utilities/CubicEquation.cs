using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.RootFinding;
using WaveShaper.Bezier;

namespace WaveShaper.Utilities
{
    public static class CubicEquation
    {
        public static double SolveBezier(BezierCurve bc, double x)
        {
            //TODO a == 0

            double a = bc.P3.X;
            double b = bc.P2.X;
            double c = bc.P1.X;
            double d = bc.P0.X - x;

            b /= a;
            c /= a;
            d /= a;

            var realRoots = Cubic.RealRoots(d, c, b);

            //Tuple<Complex, Complex, Complex> roots = Cubic.Roots(d, c, b, a);

            //var root = roots.AsEnumerable().OrderBy(r => Math.Abs(r.Imaginary)).First();
            //double t = root.Real;

            double t = realRoots.Item1;
            double y = (bc.P0.Y) + (bc.P1.Y * t) + (bc.P2.Y * t * t) + (bc.P3.Y * t * t * t);

            Debug.WriteLine($"x: {x}  y: {y}   {realRoots}");


            return Math.Round(y, 10);

            //var tRoots = Solve((decimal) c.EndPoint.X, (decimal) c.EndPointHandle.X, (decimal) c.StartPointHandle.X, (decimal) (c.StartPoint.X - x));

            //if (tRoots == null || tRoots.Length == 0)
            //    throw new Exception("No roots?");

            //Debug.WriteLine($"Roots: {tRoots}");

            //var t = tRoots[0];

            //double y = c.StartPoint.Y + c.StartPointHandle.Y * t + c.EndPointHandle.Y * t * t + c.EndPoint.Y * t * t * t;
            //return y;
        }

        //public static double[] Solve(decimal a, decimal b, decimal c, decimal d)
        //{
        //    if (a == 0)
        //        return SolveQuadratic(b, c, d);

        //    b /= a;
        //    c /= a;
        //    d /= a;

        //    decimal p = (3 * c - b * b) / 3m;
        //    decimal q = (2 * b * b * b - 9 * b * c + 27 * d) / 27m;

        //    if (p == 0)
        //        return new[] {Math.Pow((double) -q, ThirdRoot) };

        //    if (q == 0)
        //    {
        //        double rootOfP = Math.Sqrt((double) -p);
        //        return new[] {rootOfP, -rootOfP};
        //    }

        //    decimal qTmp = q / 2m, pTmp = p / 3m;
        //    var discriminant = Math.Pow((double) (q / 2), 2) + Math.Pow((double) (p / 3), 3);
        //    //decimal discriminant = qTmp * qTmp + pTmp * pTmp * pTmp;

        //    if (discriminant == 0)
        //        return new[] {Math.Pow((double) qTmp, ThirdRoot) - (double) (b/3m)};

        //    if (discriminant > 0)
        //    {
        //        double p1 = Math.Pow(-((double) q / 2) + Math.Sqrt((double) discriminant), 1 / 3d);
        //        double p2 = Math.Pow(((double) q / 2) + Math.Sqrt((double) discriminant), 1 / 3d);
        //        double p3 = (double) b / 3d;

        //        return new[]
        //        {
        //            p1 - p2 - p3
        //            /* return [
        //            Math.pow(-(q / 2) + Math.sqrt(discriminant), 1 / 3) 
        //            - Math.pow((q / 2) + Math.sqrt(discriminant), 1 / 3) 
        //            - b / 3];*/

        //            //Math.Pow(((double) -qTmp) + Math.Sqrt((double) discriminant), ThirdRoot)
        //            //- Math.Pow(((double) qTmp) + Math.Sqrt((double) discriminant), ThirdRoot)
        //            //- (double) (b / 3m)
        //        };
        //    }

        //    double pTmp3 = (double) ((-pTmp) * (-pTmp) * (-pTmp));
        //    double r = Math.Sqrt(pTmp3);
        //    double phi = Math.Acos(-((double) q / (2 * r)));
        //    double s = 2 * Math.Pow(r, ThirdRoot);

        //    double bTmp = (double) b / 3d;
        //    return new[]
        //    {
        //        s * Math.Cos(phi / 3d) - bTmp,
        //        s * Math.Cos((phi + 2 * Math.PI) / 3d) - bTmp,
        //        s * Math.Cos((phi + 4 * Math.PI) / 3d) - bTmp,
        //    };
        //}

        //private static double[] SolveQuadratic(decimal a, decimal b, decimal c)
        //{
        //    decimal discriminant = b * b - 4 * a * c;

        //    if (discriminant < 0)
        //        return null;

        //    double a2 = (double) (2 * a);
        //    return new[]
        //    {
        //        (-(double)b + Math.Sqrt((double) discriminant)) / a2,
        //        (-(double)b - Math.Sqrt((double) discriminant)) / a2
        //    };
        //}
    }
}


/*

function solveQuadraticEquation(a, b, c) {


      function roundToDecimal(num, dec) {
          return Math.round(num * Math.pow(10, dec)) / Math.pow(10, dec);
      }

      function solveCubicBezier(p0, p1, p2, p3, x) {

          p0 -= x;
          p1 -= x;
          p2 -= x;
          p3 -= x;

          var a = p3 - 3 * p2 + 3 * p1 - p0;
          var b = 3 * p2 - 6 * p1 + 3 * p0;
          var c = 3 * p1 - 3 * p0;
          var d = p0;

          var roots = solveCubicEquation(
              p3 - 3 * p2 + 3 * p1 - p0,
              3 * p2 - 6 * p1 + 3 * p0,
              3 * p1 - 3 * p0,
              p0
          );

          var result = [];
          var root;
          for (var i = 0; i < roots.length; i++) {
              root = roundToDecimal(roots[i], 15);
              if (root >= 0 && root <= 1) result.push(root);
          }

          return result;

      }

           */
