using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WaveShaper.Utilities;

namespace WaveShaper.Bezier
{
    /// <summary>
    /// Interaction logic for BezierControl.xaml
    /// </summary>
    public partial class BezierControl : UserControl
    {
        private const double Offset = 30;
        private readonly List<BezierFigure> bezierFigures = new List<BezierFigure>();
        private readonly Stack<List<BezierCurve>> stack = new Stack<List<BezierCurve>>();

        public BezierControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            UndoCommand = new UndoCommand(RestoreStateFromStack, stack);
            ButtonUndo.Command = UndoCommand;
        }

        public IEnumerable<BezierCurve> GetCurves() => bezierFigures.Select(ConvertFigureToCurve).ToList();

        private Size AreaSize { get; set; }

        private double AreaTop => Offset;
        private double AreaRight => Offset + AreaSize.Width;
        private double AreaBottom => Offset + AreaSize.Height;
        private double AreaLeft => Offset;

        private BezierCurve ConvertFigureToCurve(BezierFigure figure) => new BezierCurve
        {
            Id = figure.Id,
            P0 = ConvertPointFromCanvas(figure.StartPoint),
            P1 = ConvertPointFromCanvas(figure.StartBezierPoint),
            P2 = ConvertPointFromCanvas(figure.EndBezierPoint),
            P3 = ConvertPointFromCanvas(figure.EndPoint),
            Next = figure.NextFigure?.Id,
            Prev = figure.PreviousFigure?.Id
        };

        private BezierFigure ConvertCurveToFigure(BezierCurve curve) => new BezierFigure
        {
            Id = curve.Id,
            StartPoint = ConvertPointToCanvas(curve.P0),
            StartBezierPoint = ConvertPointToCanvas(curve.P1),
            EndBezierPoint = ConvertPointToCanvas(curve.P2),
            EndPoint = ConvertPointToCanvas(curve.P3)
        };


        internal Point ConvertPointFromCanvas(Point canvasPoint)
        {
            double x = (canvasPoint.X - AreaLeft)/AreaSize.Width;
            double y = 1.0 - ((canvasPoint.Y - AreaTop)/AreaSize.Height);
            return new Point(x, y);
        }

        private Point ConvertPointToCanvas(Point point)
        {
            double x = point.X*AreaSize.Width + AreaLeft;
            double y = (1.0 - point.Y)*AreaSize.Height + AreaTop;
            return new Point(x, y);
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            InitCanvasBorder();

            if (AreaSize == default(Size))
                return;

            if (!bezierFigures.Any())
            {
                var bc = new BezierCurve()
                {
                    P0 = new Point(0, 0),
                    P1 = new Point(0.1, 0.1),
                    P2 = new Point(0.9, 0.9),
                    P3 = new Point(1, 1)
                };

                AddFigure(ConvertCurveToFigure(bc));
            }
        }

        private void AddFigure(BezierFigure f)
        {
            Canvas.Children.Add(f);
            bezierFigures.Add(f);
        }

        private void RemoveFigure(BezierFigure f)
        {
            Canvas.Children.Remove(f);
            bezierFigures.Remove(f);
        }

        private void InitCanvasBorder()
        {
            var size = Canvas.RenderSize;
            if (size == default(Size))
                return;

            double min = Math.Min(size.Height, size.Width);
            min -= 2*Offset;
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

        private void BezierFigure_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SaveStateToStack();

            Point p = ConvertPointFromCanvas(e.GetPosition(Canvas));
            BezierFigure bf = WpfUtil.FindParent<BezierFigure>(e.Source as DependencyObject);
            BezierCurve bc = ConvertFigureToCurve(bf);

            var newCurves = bc.Split(p);
            var bf1 = ConvertCurveToFigure(newCurves.Item1);
            var bf2 = ConvertCurveToFigure(newCurves.Item2);

            if (bf.PreviousFigure != null)
                bf1.PreviousFigure = bf.PreviousFigure;

            bf1.NextFigure = bf2;

            if (bf.NextFigure != null)
                bf2.NextFigure = bf.NextFigure;

            RemoveFigure(bf);
            AddFigure(bf1);
            AddFigure(bf2);
        }

        public UndoCommand UndoCommand { get; set; }

        private void SaveStateToStack()
        {
            var curves = GetCurves().ToList();
            stack.Push(curves);
            UndoCommand.OnCanExecuteChanged();
        }

        private void RestoreStateFromStack()
        {
            if (stack.Count == 0)
                return;

            var curves = stack.Pop();
            var newFigures = curves.Select(ConvertCurveToFigure).ToDictionary(f => f.Id, f => f);

            foreach (var bf in bezierFigures.ToArray())
                RemoveFigure(bf);

            foreach (var c in curves.Where(c => c.Next != null))
            {
                // ReSharper disable once PossibleInvalidOperationException
                newFigures[c.Id].NextFigure = newFigures[c.Next.Value];
            }

            foreach (var bf in newFigures.Values)
                AddFigure(bf);
        }
    }
}
