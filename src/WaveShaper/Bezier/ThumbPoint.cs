using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WaveShaper.Core.Bezier;
using WaveShaper.Core.Utilities;
using WaveShaper.Wpf;

namespace WaveShaper.Bezier
{
    /// <summary>
    /// Inherit Thumb control to be able to update a Point dependency property while the thumb
    /// is being dragged.
    /// </summary>
    public class ThumbPoint : Thumb
    {
        #region Point

        /// <summary>
        /// Point Dependency Property
        /// </summary>
        public static readonly DependencyProperty PointProperty = DependencyProperty.Register(
            nameof(Point),
            typeof(Point),
            typeof(ThumbPoint),
            new FrameworkPropertyMetadata(new Point()));

        private BezierControl bezierControl;
        private BezierFigure bezierFigure;

        /// <summary>
        /// Gets or sets the Point property
        /// </summary>
        public Point Point
        {
            get { return (Point)GetValue(PointProperty); }
            set { SetValue(PointProperty, value); }
        }

        #endregion

        private Point mouseMoveStartPoint;
        private List<BezierCurve> mouseMoveStartingState;

        private BezierControl BezierControl => bezierControl ?? (bezierControl = WpfUtil.FindParent<BezierControl>(this));

        private BezierFigure BezierFigure => bezierFigure ?? (bezierFigure = WpfUtil.FindParent<BezierFigure>(this));

        static ThumbPoint()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ThumbPoint), new FrameworkPropertyMetadata(typeof(ThumbPoint)));
        }

        public ThumbPoint()
        {
            this.DragDelta += this.OnDragDelta;
            this.Loaded += OnLoaded;

            // Thumb swallows mouse events, need to register with true, so we also receive handled events
            this.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
            this.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(OnMouseLeftButtonUp), true);

            Cursor = Cursors.Hand;
        }

        internal ThumbPointType? Type { get; set; }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            UpdateTooltip();
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var p = Point;
            mouseMoveStartPoint = new Point(p.X, p.Y);
            mouseMoveStartingState = BezierControl.GetCurves().ToList();
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            double newX = this.Point.X + e.HorizontalChange;
            if ((Type == ThumbPointType.P0 && BezierFigure?.PreviousFigure == null)
                || (Type == ThumbPointType.P3 && BezierFigure?.NextFigure == null))
            {
                newX = this.Point.X;
            }

            this.Point = new Point(newX, this.Point.Y + e.VerticalChange);
            UpdateTooltip();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var p = Point;
            if (p != mouseMoveStartPoint)
                BezierControl.SaveStateToStack(mouseMoveStartingState);
        }

        private void UpdateTooltip()
        {
            if (BezierControl == null)
                return;

            var p = BezierControl.ConvertPointFromCanvas(this.Point);
            ToolTip = $"X: {p.X:0.####}\nY: {p.Y:0.####}";
        }

    }

    internal enum ThumbPointType
    {
        P0,
        P1,
        P2,
        P3,
    }
}
