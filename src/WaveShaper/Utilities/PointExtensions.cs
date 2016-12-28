using System.Windows;

namespace WaveShaper.Utilities
{
    internal static class PointExtensions
    {
        public static Point Mul(this Point p, double val)
        {
            return new Point(p.X * val, p.Y * val);
        }

        public static Point Add(this Point p, Point p2)
        {
            return new Point(p.X + p2.X, p.Y + p2.Y);
        }
    }

    class CalcPoint
    {
        private readonly Point p;

        public CalcPoint(Point p)
        {
            this.p = p;
        }

        public CalcPoint(double x, double y)
        {
            this.p = new Point(x, y);
        }

        public double X => p.X;
        public double Y => p.Y;
        public Point Point => p;

        public static CalcPoint operator +(CalcPoint a, CalcPoint b) => new CalcPoint(a.X + b.X, a.Y + b.Y);
        public static CalcPoint operator -(CalcPoint a, CalcPoint b) => new CalcPoint(a.X - b.X, a.Y - b.Y);
        public static CalcPoint operator *(CalcPoint a, double b) => new CalcPoint(a.X * b, a.Y * b);
        public static CalcPoint operator *(double b, CalcPoint a) => new CalcPoint(a.X * b, a.Y * b);
        public static CalcPoint operator *(CalcPoint b, CalcPoint a) => new CalcPoint(a.X * b.X, a.Y * b.Y);

    }
}
