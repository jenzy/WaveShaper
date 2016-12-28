using System;
using System.Windows;
using System.Windows.Controls;

namespace WaveShaper.Bezier
{
    public class BezierFigure : Control
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        #region StartPoint
        /// <summary>
        /// StartPoint Dependency Property
        /// </summary>
        public static readonly DependencyProperty StartPointProperty = DependencyProperty.Register(
                nameof(StartPoint),
                typeof(Point),
                typeof(BezierFigure),
                new FrameworkPropertyMetadata(new Point(), StartPointChanged));

        private static void StartPointChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var bf = dependencyObject as BezierFigure;
            bf?.PreviousFigure?.SetPointFromNeighbour(EndPointProperty, (Point) dependencyPropertyChangedEventArgs.NewValue);
        }

        /// <summary>
        /// Gets or sets the StartPoint property
        /// </summary>
        public Point StartPoint
        {
            get { return (Point)GetValue(StartPointProperty); }
            set
            {
                SetValue(StartPointProperty, value);
                PreviousFigure?.SetPointFromNeighbour(EndPointProperty, value);
            }
        }
        #endregion

        #region EndPoint
        /// <summary>
        /// EndPoint Dependency Property
        /// </summary>
        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
                nameof(EndPoint),
                typeof(Point),
                typeof(BezierFigure),
                new FrameworkPropertyMetadata(new Point(), EndPointChanged));

        private static void EndPointChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var bf = dependencyObject as BezierFigure;
            bf?.NextFigure?.SetPointFromNeighbour(StartPointProperty, (Point)dependencyPropertyChangedEventArgs.NewValue);
        }

        /// <summary>
        /// Gets or sets the EndPoint property
        /// </summary>
        public Point EndPoint
        {
            get { return (Point)GetValue(EndPointProperty); }
            set
            {
                SetValue(EndPointProperty, value);
                NextFigure?.SetPointFromNeighbour(StartPointProperty, value);
            }
        }
        #endregion

        #region StartBezierPoint
        /// <summary>
        /// StartBezierPoint Dependency Property
        /// </summary>
        public static readonly DependencyProperty StartBezierPointProperty = DependencyProperty.Register(
                nameof(StartBezierPoint),
                typeof(Point),
                typeof(BezierFigure),
                new FrameworkPropertyMetadata(new Point()));

        /// <summary>
        /// Gets or sets the StartBezierPoint property
        /// </summary>
        public Point StartBezierPoint
        {
            get { return (Point)GetValue(StartBezierPointProperty); }
            set { SetValue(StartBezierPointProperty, value); }
        }
        #endregion

        #region EndBezierPoint
        /// <summary>
        /// StartBezierPoint Dependency Property
        /// </summary>
        public static readonly DependencyProperty EndBezierPointProperty = DependencyProperty.Register(
                nameof(EndBezierPoint),
                typeof(Point),
                typeof(BezierFigure),
                new FrameworkPropertyMetadata(new Point()));

        private BezierFigure previousFigure;
        private BezierFigure nextFigure;

        /// <summary>
        /// Gets or sets the StartBezierPoint property
        /// </summary>
        public Point EndBezierPoint
        {
            get { return (Point)GetValue(EndBezierPointProperty); }
            set { SetValue(EndBezierPointProperty, value); }
        }
        #endregion

        public BezierFigure PreviousFigure
        {
            get { return previousFigure; }
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (previousFigure == value)
                    return;

                previousFigure = value;
                value.NextFigure = this;
            }
        }

        public BezierFigure NextFigure
        {
            get { return nextFigure; }
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (nextFigure == value)
                    return;

                nextFigure = value;
                value.PreviousFigure = this;
            }
        }

        private void SetPointFromNeighbour(DependencyProperty propery, Point p)
        {
            SetValue(propery, p);
        }

        static BezierFigure()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BezierFigure), new FrameworkPropertyMetadata(typeof(BezierFigure)));
        }
    }
}
