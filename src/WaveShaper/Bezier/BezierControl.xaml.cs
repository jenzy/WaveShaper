using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WaveShaper.Bezier
{
    /// <summary>
    /// Interaction logic for BezierControl.xaml
    /// </summary>
    public partial class BezierControl : UserControl
    {
        private const double Offset = 10;

        public BezierControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            //var bc = new BezierFigure
            //{
            //    StartPoint = new Point(10, 10),
            //    StartBezierPoint = new Point(10, 30),
            //    EndBezierPoint = new Point(50, 80),
            //    EndPoint = new Point(50, 50)
            //};

            //Canvas.Children.Add(bc);
            //bezierFigures.Add(bc);
        }

        private List<BezierFigure> bezierFigures = new List<BezierFigure>();

        private Size AreaSize { get; set; }

        private double AreaTop => Offset;
        private double AreaRight => Offset + AreaSize.Width;
        private double AreaBottom => Offset + AreaSize.Height;
        private double AreaLeft => Offset;

        //private BezierCurve ConvertFigureToCurve(BezierFigure figure)
        //{
            
        //}

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
                    EndPoint = ConvertPointToCanvas(new Point(1, 1))
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
    }
}
