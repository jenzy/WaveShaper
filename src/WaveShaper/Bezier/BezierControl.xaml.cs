using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;
using WaveShaper.Windows;

namespace WaveShaper.Bezier
{
    /// <summary>
    /// Interaction logic for BezierControl.xaml
    /// </summary>
    public partial class BezierControl : UserControl
    {
        private const double Offset = 20;
        private readonly List<BezierFigure> bezierFigures = new List<BezierFigure>();

        public BezierControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        public IEnumerable<BezierCurve> GetCurves()
        {
            var list = new List<BezierCurve>();

            foreach (var bf in bezierFigures)
            {
                //Debug.WriteLine($"{bf.StartPoint} {bf.StartBezierPoint} {bf.EndBezierPoint} {bf.EndPoint}");

                var c = ConvertFigureToCurve(bf);
                list.Add(c);

                //Debug.WriteLine($"{c.P0} {c.P1} {c.P2} {c.P3}");
            }

            return list;
            //return bezierFigures.Select(ConvertFigureToCurve);
        }

        private Size AreaSize { get; set; }

        private double AreaTop => Offset;
        private double AreaRight => Offset + AreaSize.Width;
        private double AreaBottom => Offset + AreaSize.Height;
        private double AreaLeft => Offset;

        private BezierCurve ConvertFigureToCurve(BezierFigure figure)
        {
            return new BezierCurve
            {
                P0 = ConvertPointFromCanvas(figure.StartPoint),
                P1 = ConvertPointFromCanvas(figure.StartBezierPoint),
                P2 = ConvertPointFromCanvas(figure.EndBezierPoint),
                P3 = ConvertPointFromCanvas(figure.EndPoint)
            };
        }

        private Point ConvertPointFromCanvas(Point canvasPoint)
        {
            double x = (canvasPoint.X - AreaLeft) / AreaSize.Width;
            double y = 1.0 - ((canvasPoint.Y - AreaTop) / AreaSize.Height);
            return new Point(x, y);
        }

        private Point ConvertPointToCanvas(Point point)
        {
            double x = point.X * AreaSize.Width + AreaLeft;
            double y = (1.0 - point.Y) * AreaSize.Height + AreaTop;
            return new Point(x, y);
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            InitCanvasBorder();

            if (AreaSize == default(Size))
                return;

            if (!bezierFigures.Any())
            {
                var bf = new BezierFigure
                {
                    StartPoint = ConvertPointToCanvas(new Point(0, 0)),
                    StartBezierPoint = ConvertPointToCanvas(new Point(0.1, 0.1)),
                    EndBezierPoint = ConvertPointToCanvas(new Point(0.9, 0.9)),
                    EndPoint = ConvertPointToCanvas(new Point(1, 1)),
                };

                Canvas.Children.Add(bf);
                bezierFigures.Add(bf);
            }
        }

        private void InitCanvasBorder()
        {
            var size = Canvas.RenderSize;
            if (size == default(Size))
                return;

            double min = Math.Min(size.Height, size.Width);
            min -= 2 * Offset;
            AreaSize = new Size(min, min);

            var rect = new Rectangle
            {
                Width = AreaSize.Width,
                Height = AreaSize.Height,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.Children.Add(rect);
            Canvas.SetLeft(rect, Offset);
            Canvas.SetTop(rect, Offset);

            Debug.WriteLine($"Area size: {AreaSize}");
            Debug.WriteLine($"Area: {AreaTop}, {AreaRight}, {AreaBottom}, {AreaLeft}");
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (bezierFigures.Count == 0)
                return;

            //var bf = bezierFigures[0];
            var bc = ConvertFigureToCurve(bezierFigures[0]);
            Debug.WriteLine($"{bc.P0} {bc.P1} {bc.P2} {bc.P3}");

            var pm = new PlotModel();
            var ls1 = new LineSeries();

            for (double t = 0; t < 1; t+=0.01)
            {
                double tt = 1 - t;

                //var x = bc.P0.X + bc.P1.X * t + bc.P2.X * t * t + bc.P3.X * t * t * t;
                //var y = bc.P0.Y + bc.P1.Y * t + bc.P2.Y * t * t + bc.P3.Y * t * t * t;

                //var x = bf.StartPoint.X + bf.StartBezierPoint.X * t + bf.EndBezierPoint.X * t * t + bf.EndPoint.X * t * t * t;
                //var y = bf.StartPoint.Y + bf.StartBezierPoint.Y * t + bf.EndBezierPoint.Y * t * t + bf.EndPoint.Y * t * t * t;

                //var x = bf.EndPoint.X + bf.EndBezierPoint.X * t + bf.StartBezierPoint.X * t * t + bf.StartPoint.X * t * t * t;
                //var y = bf.EndPoint.Y + bf.EndBezierPoint.Y * t + bf.StartBezierPoint.Y * t * t + bf.StartPoint.Y * t * t * t;

                var x = bc.P0.X * tt * tt * tt + 3 * bc.P1.X * tt * tt * t + 3 * bc.P2.X * tt * t * t + bc.P3.X * t * t * t;
                var y = bc.P0.Y * tt * tt * tt + 3 * bc.P1.Y * tt * tt * t + 3 * bc.P2.Y * tt * t * t + bc.P3.Y * t * t * t;

                ls1.Points.Add(new DataPoint(x, y));
            }

            pm.Series.Add(ls1);

            var w = new DebugPlot(pm);
            w.Show();

        }
    }
}
