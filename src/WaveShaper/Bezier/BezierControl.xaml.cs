using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WaveShaper.Annotations;
using WaveShaper.Utilities;

namespace WaveShaper.Bezier
{
    /// <summary>
    ///     Interaction logic for BezierControl.xaml
    /// </summary>
    public partial class BezierControl : UserControl, INotifyPropertyChanged
    {
        private const double Offset = 40;
        private readonly List<BezierFigure> bezierFigures = new List<BezierFigure>();
        private readonly Stack<List<BezierCurve>> stackUndo = new Stack<List<BezierCurve>>();
        private readonly Stack<List<BezierCurve>> stackRedo = new Stack<List<BezierCurve>>();
        private string currentMousePosition;

        public BezierControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            UndoCommand = new ActionCommand(p =>
            {
                stackRedo.Push(GetCurves().ToList());
                RestoreStateFromStack(stackUndo);
                RedoCommand.OnCanExecuteChanged();
            }, p => stackUndo.Count > 0);
            RedoCommand = new ActionCommand(p =>
            {
                stackUndo.Push(GetCurves().ToList());
                RestoreStateFromStack(stackRedo);
                UndoCommand.OnCanExecuteChanged();
            }, p => stackRedo.Count > 0);

            ButtonUndo.Command = UndoCommand;
            ButtonRedo.Command = RedoCommand;
        }

        private Size AreaSize { get; set; }

        private double AreaTop => Offset;
        private double AreaRight => Offset + AreaSize.Width;
        private double AreaBottom => Offset + AreaSize.Height;
        private double AreaLeft => Offset;

        public ActionCommand UndoCommand { get; set; }
        public ActionCommand RedoCommand { get; set; }

        public string CurrentMousePosition
        {
            get { return currentMousePosition; }
            set
            {
                if (value == currentMousePosition) return;
                currentMousePosition = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<BezierCurve> GetCurves() => bezierFigures.Select(ConvertFigureToCurve).ToList();

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
            double y = 1.0 - (canvasPoint.Y - AreaTop)/AreaSize.Height;
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

            AddCanvasText("0", new Point(-0.03, 0));
            AddCanvasText("1", new Point(0.99, 0));
            AddCanvasText("1", new Point(-0.03, 1.02));

            if (!bezierFigures.Any())
            {
                var bc = new BezierCurve
                {
                    P0 = new Point(0, 0),
                    P1 = new Point(0.1, 0.1),
                    P2 = new Point(0.9, 0.9),
                    P3 = new Point(1, 1)
                };

                AddFigure(ConvertCurveToFigure(bc));
            }
        }

        private void AddCanvasText(string text, Point normalisedPoint)
        {
            var tb = new TextBlock() {Text = text, };
            Canvas.Children.Add(tb);

            var p = ConvertPointToCanvas(normalisedPoint);
            Canvas.SetLeft(tb, p.X);
            Canvas.SetTop(tb, p.Y);
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
            Size size = Canvas.RenderSize;
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
            var bf = WpfUtil.FindParent<BezierFigure>(e.Source as DependencyObject);
            BezierCurve bc = ConvertFigureToCurve(bf);

            Tuple<BezierCurve, BezierCurve> newCurves = bc.Split(p);
            BezierFigure bf1 = ConvertCurveToFigure(newCurves.Item1);
            BezierFigure bf2 = ConvertCurveToFigure(newCurves.Item2);

            if (bf.PreviousFigure != null)
                bf1.PreviousFigure = bf.PreviousFigure;

            bf1.NextFigure = bf2;

            if (bf.NextFigure != null)
                bf2.NextFigure = bf.NextFigure;

            RemoveFigure(bf);
            AddFigure(bf1);
            AddFigure(bf2);
        }

        public void SaveStateToStack(List<BezierCurve> curves = null)
        {
            curves = curves ?? GetCurves().ToList();
            stackUndo.Push(curves);

            stackRedo.Clear();

            UndoCommand.OnCanExecuteChanged();
            RedoCommand.OnCanExecuteChanged();
        }

        private void RestoreStateFromStack(Stack<List<BezierCurve>> stack)
        {
            if (stack.Count == 0)
                return;

            List<BezierCurve> curves = stack.Pop();
            Dictionary<Guid, BezierFigure> newFigures = curves.Select(ConvertCurveToFigure).ToDictionary(f => f.Id, f => f);

            foreach (BezierFigure bf in bezierFigures.ToArray())
                RemoveFigure(bf);

            foreach (BezierCurve c in curves.Where(c => c.Next != null))
                // ReSharper disable once PossibleInvalidOperationException
                newFigures[c.Id].NextFigure = newFigures[c.Next.Value];

            foreach (BezierFigure bf in newFigures.Values)
                AddFigure(bf);
        }

        private void Canvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(Canvas);
            p = ConvertPointFromCanvas(p);
            p.X = Math.Round(p.X, 2);
            p.Y = Math.Round(p.Y, 2);

            CurrentMousePosition = $"X: {p.X:0.00}\nY: {p.Y:0.00}";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}