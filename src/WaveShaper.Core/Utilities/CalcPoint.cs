using System.Windows;

namespace WaveShaper.Core.Utilities
{
    public class CalcPoint
    {
        public CalcPoint(Point p)
        {
            this.Point = p;
        }

        public CalcPoint(double x, double y)
        {
            this.Point = new Point(x, y);
        }

        public double X => Point.X;

        public double Y => Point.Y;

        public Point Point { get; }

        public static CalcPoint operator +(CalcPoint a, CalcPoint b) => new CalcPoint(a.X + b.X, a.Y + b.Y);

        public static CalcPoint operator -(CalcPoint a, CalcPoint b) => new CalcPoint(a.X - b.X, a.Y - b.Y);

        public static CalcPoint operator *(CalcPoint a, double b) => new CalcPoint(a.X * b, a.Y * b);

        public static CalcPoint operator *(double b, CalcPoint a) => new CalcPoint(a.X * b, a.Y * b);

        public static CalcPoint operator *(CalcPoint b, CalcPoint a) => new CalcPoint(a.X * b.X, a.Y * b.Y);
    }
}
