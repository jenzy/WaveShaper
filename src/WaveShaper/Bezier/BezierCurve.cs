using System;
using System.Windows;
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

                double r = CubicEquation.SolveBezier(this, x);
                return neg ? -r : r;
            };
        }

        //public Func<double, double> GetFunctionOfX()
        //{
        //    double y0 = StartPoint.Y;
        //    double y1 = StartPointHandle.Y;
        //    double y2 = EndPointHandle.Y;
        //    double y3 = EndPoint.Y;
        //    return x =>
        //    {
        //        bool neg = x < 0;
        //        x = Math.Abs(x);

        //        double oneMinusX = 1.0 - x;

        //        //double r = oneMinusX * oneMinusX * oneMinusX * y0
        //        //           + 3 * oneMinusX * oneMinusX * x * y1
        //        //           + 3 * oneMinusX * x * x * y2
        //        //           + x * x * x * y3;

        //        double r = (1 - x*x*x) * y0
        //                  + 3 * (1 - x*x) * x * y1
        //                  + 3 * (1-x) * x * x * y2
        //                  + x * x * x * y3;

        //        return neg ? -r : r;
        //    };
        //}
    }
}
