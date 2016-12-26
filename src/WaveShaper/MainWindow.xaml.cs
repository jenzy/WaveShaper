using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Jace;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using WaveShaper.Annotations;
using WaveShaper.Bezier;
using WaveShaper.Utilities;
using WaveShaper.Windows;

namespace WaveShaper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private PlotModel shapingFunctionPlot;
        private ObservableCollection<PiecewiseFunctionRow> rows;

        private ObservableCollection<PiecewiseFunctionRow> piecewiseFunctionRows = new ObservableCollection<PiecewiseFunctionRow>
        {
            new PiecewiseFunctionRow(ProcessingType.PiecewiseFunction)
            {
                FromOperator = Operator.LessOrEqualThan,
                ToOperator = Operator.LessOrEqualThan,
                Expression = "x"
            }
        };

        private ObservableCollection<PiecewiseFunctionRow> piecewisePolynomialRows = new ObservableCollection<PiecewiseFunctionRow>
        {
            new PiecewiseFunctionRow(ProcessingType.PiecewisePolynomial)
            {
                FromOperator = Operator.LessOrEqualThan,
                ToOperator = Operator.LessOrEqualThan,
                Expression = "0, 1"
            }
        };

        public MainWindow()
        {
            InitialisePlot();
            InitializeComponent();
            PlotShapingFunction(Player.ShapingFunction);
        }

        private void InitialisePlot()
        {
            shapingFunctionPlot = new PlotModel
            {
                IsLegendVisible = false,
                Title = "Shaping function",
                TitleFontSize = 12,
                TitleFont = "Segoe UI",
                TitleFontWeight = 1
            };
            shapingFunctionPlot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = -1.1,
                Maximum = 1.1,
                ExtraGridlines = new[] { 0d },
                ExtraGridlineColor = OxyColors.LightGray,
                Title = "f(x)"
            });
            shapingFunctionPlot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = -1.1,
                Maximum = 1.1,
                ExtraGridlines = new[] { 0d },
                ExtraGridlineColor = OxyColors.LightGray,
                Title = "x"
            });
        }

        public ObservableCollection<PiecewiseFunctionRow> Rows
        {
            get { return rows; }
            set
            {
                if (Equals(value, rows)) return;
                rows = value;
                OnPropertyChanged();
            }
        }

        public PlotModel ShapingFunctionPlot
        {
            get { return shapingFunctionPlot; }
            set
            {
                if (Equals(value, shapingFunctionPlot)) return;
                shapingFunctionPlot = value;
                OnPropertyChanged();
            }
        }

        private void DdlProcessingType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl == null)
                return;

            var previousType = (ProcessingType) e.RemovedItems.Cast<EnumUtil.EnumListItem>().Single().Value;
            var newType = (ProcessingType) e.AddedItems.Cast<EnumUtil.EnumListItem>().Single().Value;

            switch (newType)
            {
                case ProcessingType.NoProcessing:
                    TabControl.SelectedItem = TabNone;
                    break;

                case ProcessingType.PiecewisePolynomial:
                    InitFunctionRows(previousType, newType);
                    TabControl.SelectedItem = TabTable;
                    break;

                case ProcessingType.PiecewiseFunction:
                    InitFunctionRows(previousType, newType);
                    TabControl.SelectedItem = TabTable;
                    break;

                case ProcessingType.Bezier:
                    TabControl.SelectedItem = TabBezier;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            BtnPresets.IsEnabled = newType != ProcessingType.NoProcessing;
        }

        private void BtnApply_OnClick(object sender, RoutedEventArgs e)
        {
            Player.ShapingFunction = BuildFunction();

            PlotShapingFunction(Player.ShapingFunction);
        }

        private void InitFunctionRows(ProcessingType old, ProcessingType newType)
        {
            switch (old)
            {
                case ProcessingType.PiecewisePolynomial:
                    piecewisePolynomialRows = Rows;
                    break;

                case ProcessingType.PiecewiseFunction:
                    piecewiseFunctionRows = Rows;
                    break;
            }

            switch (newType)
            {
                case ProcessingType.PiecewisePolynomial:
                    Rows = piecewisePolynomialRows;
                    break;

                case ProcessingType.PiecewiseFunction:
                    Rows = piecewiseFunctionRows;
                    break;
            }
        }

        private void PlotShapingFunction(Func<double, double> shapingFunction)
        {
            try
            {
                ShapingFunctionPlot.Series.Clear();
                ShapingFunctionPlot.Series.Add(new FunctionSeries(shapingFunction, -1, 1, 100, "f(x)"));
                ShapingFunctionPlot.InvalidatePlot(true);
            }
            catch (PiecewiseFunctionInputOutOfRange)
            {
                MessageBox.Show(this, "Function does not cover all possible values.", "Error");
            }
        }

        private Func<double, double> BuildFunction()
        {
            var mode = (ProcessingType)DdlProcessingType.SelectedValue;

            Func<double, double> func = null;
            switch (mode)
            {
                case ProcessingType.NoProcessing:
                    func = ShapingSampleProvider.DefaultShapingFunction;
                    break;
                case ProcessingType.PiecewiseFunction:
                    func = BuildPiecewiseFunction(Rows).Calculate;
                    break;
                case ProcessingType.PiecewisePolynomial:
                    func = BuildPiecewisePolynomial(Rows).Calculate;
                    break;
                case ProcessingType.Bezier:
                    func = BuildBezierFunction(Bezier.GetCurves()).Calculate;
                    break;
            }

            return func;
        }

        private static PiecewiseFunction<double> BuildPiecewiseFunction(IEnumerable<PiecewiseFunctionRow> rows)
        {
            var function = new PiecewiseFunction<double>();
            var engine = new CalculationEngine();
            foreach (var row in rows)
            {
                var piece = new Piece<double>
                {
                    Condition = row.GetCondition(),
                    Function = (Func<double, double>)engine.Formula(row.Expression.Replace("pi", Math.PI.ToString(CultureInfo.InvariantCulture)))
                                                            .Parameter("x", DataType.FloatingPoint)
                                                            .Result(DataType.FloatingPoint).Build()
                };
                function.AddPiece(piece);
            }

            function.Preprocess = x => x.Clamp(-1, 1);
            return function;
        }

        private static PiecewiseFunction<double> BuildPiecewisePolynomial(IEnumerable<PiecewiseFunctionRow> rows)
        {
            var function = new PiecewiseFunction<double>();
            foreach (var row in rows)
            {
                function.AddPiece(new Piece<double>
                {
                    Condition = row.GetCondition(),
                    Function = row.GetPolynomialFunction()
                });
            }

            function.Preprocess = x => x.Clamp(-1, 1);
            return function;
        }

        private static PiecewiseFunction<double> BuildBezierFunction(IEnumerable<BezierCurve> curves)
        {
            var function = new PiecewiseFunction<double>();

            foreach (var curve in curves.OrderBy(c => c.P0.X))
            {
                var func = curve.GetFunctionOfX();
                function.AddPiece(new Piece<double>
                {
                    Condition = x => curve.P0.X <= x && x <= curve.P3.X,
                    Function = func
                });
                function.AddPiece(new Piece<double>
                {
                    Condition = x => -curve.P3.X <= x && x <= -curve.P0.X,
                    Function = func
                });
            }

            function.AddPiece(new Piece<double>
            {
                Condition = x => true,
                Function = x => 0
            });

            function.Preprocess = x => x.Clamp(-1, 1);
            return function;
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var param = (string) e.Parameter;
            var type = (ProcessingType) DdlProcessingType.SelectedValue;
            if (type == ProcessingType.NoProcessing)
                return;

            switch (param)
            {
                case "Identity":
                    Rows.Clear();
                    if (type == ProcessingType.PiecewiseFunction)
                    {
                        Rows.Add(new PiecewiseFunctionRow
                        {
                            Expression = "x"
                        });
                    }
                    else if (type == ProcessingType.PiecewisePolynomial)
                    {
                        Rows.Add(new PiecewiseFunctionRow(mode: ProcessingType.PiecewisePolynomial)
                        {
                            Expression = "0,1"
                        });
                    }
                    break;

                case "SoftClipping1":
                    Rows.Clear();
                    if (type == ProcessingType.PiecewiseFunction)
                    {
                        Rows.Add(new PiecewiseFunctionRow { ToOperator = Operator.LessOrEqualThan, To = -1, Expression = "-2/3" });
                        Rows.Add(new PiecewiseFunctionRow
                        {
                            From = -1,
                            FromOperator = Operator.LessThan,
                            ToOperator = Operator.LessThan,
                            To = 1,
                            Expression = "x - (x^3)/3"
                        });
                        Rows.Add(new PiecewiseFunctionRow { FromOperator = Operator.LessOrEqualThan, From = 1, Expression = "2/3" });
                    }
                    else if (type == ProcessingType.PiecewisePolynomial)
                    {
                        Rows.Add(new PiecewiseFunctionRow(mode: ProcessingType.PiecewisePolynomial) { ToOperator = Operator.LessOrEqualThan, To = -1, Expression = "-0.666" });
                        Rows.Add(new PiecewiseFunctionRow(mode: ProcessingType.PiecewisePolynomial)
                        {
                            From = -1,
                            FromOperator = Operator.LessThan,
                            ToOperator = Operator.LessThan,
                            To = 1,
                            Expression = "0, 1, 0, -0.333"
                        });
                        Rows.Add(new PiecewiseFunctionRow(mode: ProcessingType.PiecewisePolynomial) { FromOperator = Operator.LessOrEqualThan, From = 1, Expression = "0.666" });
                    }
                    break;

                case "SoftClipping2":
                    if (type == ProcessingType.PiecewiseFunction)
                    {
                        Rows.Clear();
                        Rows.Add(new PiecewiseFunctionRow { ToOperator = Operator.LessOrEqualThan, To = -0.5, Expression = "-1" });
                        Rows.Add(new PiecewiseFunctionRow
                        {
                            From = -0.5,
                            FromOperator = Operator.LessThan,
                            ToOperator = Operator.LessThan,
                            To = 0.5,
                            Expression = "sin(pi * x / (2 * 0.5))"
                        });
                        Rows.Add(new PiecewiseFunctionRow { FromOperator = Operator.LessOrEqualThan, From = 0.5, Expression = "1" });
                    }
                    break;
            }

        }

        private void BtnPreviewEffect_OnClick(object sender, RoutedEventArgs e)
        {
            var func = BuildFunction();

            var window = new EffectPreview(func);
            window.Show();
        }
    }
}
