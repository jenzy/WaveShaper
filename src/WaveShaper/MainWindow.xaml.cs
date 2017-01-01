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

        private readonly Dictionary<ProcessingType, bool> mirroredPerMode = new Dictionary<ProcessingType, bool>();

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

            bool mirroredNew;
            if (!mirroredPerMode.TryGetValue(newType, out mirroredNew))
                mirroredNew = true;
            mirroredPerMode[previousType] = CbMirrored.IsChecked.HasValue && CbMirrored.IsChecked.Value;

            switch (newType)
            {
                case ProcessingType.NoProcessing:
                    TabControl.SelectedItem = TabNone;
                    CbMirrored.IsEnabled = false;
                    CbMirrored.IsChecked = false;
                    break;

                case ProcessingType.PiecewisePolynomial:
                    InitFunctionRows(previousType, newType);
                    TabControl.SelectedItem = TabTable;
                    CbMirrored.IsEnabled = true;
                    CbMirrored.IsChecked = mirroredNew;
                    break;

                case ProcessingType.PiecewiseFunction:
                    InitFunctionRows(previousType, newType);
                    TabControl.SelectedItem = TabTable;
                    CbMirrored.IsEnabled = true;
                    CbMirrored.IsChecked = mirroredNew;
                    break;

                case ProcessingType.Bezier:
                    TabControl.SelectedItem = TabBezier;
                    CbMirrored.IsEnabled = false;
                    CbMirrored.IsChecked = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetupPresets(newType);
        }

        private void SetupPresets(ProcessingType type)
        {
            BtnPresets.IsEnabled = type != ProcessingType.NoProcessing;
            var menu = BtnPresets.ContextMenu ?? (BtnPresets.ContextMenu = new ContextMenu());
            menu.Items.Clear();

            menu.Items.Add(CustomCommands.Presets.Identity.ToMenuItem(commandParameter: type));
            menu.Items.Add(CustomCommands.Presets.SoftClip1.ToMenuItem(commandParameter: type));

            if (type == ProcessingType.PiecewiseFunction || type == ProcessingType.Bezier)
                menu.Items.Add(CustomCommands.Presets.SoftClip2.ToMenuItem(commandParameter: type));
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
            bool mirrored = CbMirrored.IsChecked == true;

            Func<double, double> func = null;
            switch (mode)
            {
                case ProcessingType.NoProcessing:
                    func = ShapingSampleProvider.DefaultShapingFunction;
                    break;
                case ProcessingType.PiecewiseFunction:
                    func = BuildPiecewiseFunction(Rows, mirrored).Calculate;
                    break;
                case ProcessingType.PiecewisePolynomial:
                    func = BuildPiecewisePolynomial(Rows, mirrored).Calculate;
                    break;
                case ProcessingType.Bezier:
                    func = BuildBezierFunction(Bezier.GetCurves()).Calculate;
                    break;
            }

            return func;
        }

        private static PiecewiseFunction<double> BuildPiecewiseFunction(IEnumerable<PiecewiseFunctionRow> rows, bool mirrored = false)
        {
            var function = new PiecewiseFunction<double>();
            var mirroredList = new List<Piece<double>>();
            var engine = new CalculationEngine();
            foreach (var row in rows)
            {
                var pieceFunction = (Func<double, double>) engine.Formula(row.Expression.Replace("pi", Math.PI.ToString(CultureInfo.InvariantCulture)))
                                                            .Parameter("x", DataType.FloatingPoint)
                                                            .Result(DataType.FloatingPoint).Build();

                if (mirrored)
                {
                    function.AddPiece(new Piece<double>(row.GetCondition(inverted: false), x => pieceFunction(x)));
                    mirroredList.Add(new Piece<double>(row.GetCondition(inverted: true), x => -pieceFunction(Math.Abs(x))));
                }
                else
                {
                    function.AddPiece(new Piece<double>(row.GetCondition(), pieceFunction));
                }
            }

            if (mirroredList.Any())
            {
                mirroredList.Reverse();
                function.AddPieces(mirroredList);
            }

            function.AddPiece(Piece<double>.DefaultPiece);

            //function.Preprocess = x => x.Clamp(-1, 1);
            return function;
        }

        private static PiecewiseFunction<double> BuildPiecewisePolynomial(IEnumerable<PiecewiseFunctionRow> rows, bool mirrored = false)
        {
            var function = new PiecewiseFunction<double>();
            var mirroredList = new List<Piece<double>>();
            foreach (var row in rows)
            {
                var pieceFunction = row.GetPolynomialFunction();

                if (mirrored)
                {
                    function.AddPiece(new Piece<double>(row.GetCondition(inverted: false), x => pieceFunction(x)));
                    mirroredList.Add(new Piece<double>(row.GetCondition(inverted: true), x => -pieceFunction(Math.Abs(x))));
                }
                else
                {
                    function.AddPiece(new Piece<double>(row.GetCondition(), pieceFunction));
                }
            }

            if (mirroredList.Any())
            {
                mirroredList.Reverse();
                function.AddPieces(mirroredList);
            }

            function.AddPiece(Piece<double>.DefaultPiece);

            //function.Preprocess = x => x.Clamp(-1, 1);
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

            function.AddPiece(Piece<double>.DefaultPiece);

            //function.Preprocess = x => x.Clamp(-1, 1);
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

        private void BtnPreviewEffect_OnClick(object sender, RoutedEventArgs e)
        {
            var func = BuildFunction();

            var window = new EffectPreview(func);
            window.Show();
        }

        private void PresetIdentity_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var type = (ProcessingType) e.Parameter;
            if (type == ProcessingType.PiecewiseFunction)
            {
                Rows.Clear();
                Rows.Add(new PiecewiseFunctionRow { Expression = "x" });
            }
            else if (type == ProcessingType.PiecewisePolynomial)
            {
                Rows.Clear();
                Rows.Add(new PiecewiseFunctionRow(mode: ProcessingType.PiecewisePolynomial) { Expression = "0,1" });
            }
            else if (type == ProcessingType.Bezier)
            {
                Bezier.ClearAndSetCurves(BezierCurve.GetIdentity());
            }
        }

        private void PresetSoftClip1_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var type = (ProcessingType)e.Parameter;
            if (type == ProcessingType.PiecewiseFunction)
            {
                Rows.Clear();
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
                Rows.Clear();
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
            else if (type == ProcessingType.Bezier)
            {
                Bezier.ClearAndSetCurves(new List<BezierCurve>
                {
                    new BezierCurve
                    {
                        P0 = new Point(0, 0),
                        P1 = new Point(0.5, 0.5),
                        P2 = new Point(0.75, 2/3d),
                        P3 = new Point(1.00001, 2/3d)
                    }
                });
            }
        }

        private void PresetSoftClip2_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var type = (ProcessingType)e.Parameter;
            if (type == ProcessingType.PiecewiseFunction)
            {
                Rows.Clear();
                Rows.Add(new PiecewiseFunctionRow {ToOperator = Operator.LessOrEqualThan, To = -0.5, Expression = "-1"});
                Rows.Add(new PiecewiseFunctionRow
                {
                    From = -0.5,
                    FromOperator = Operator.LessThan,
                    ToOperator = Operator.LessThan,
                    To = 0.5,
                    Expression = "sin(pi * x / (2 * 0.5))"
                });
                Rows.Add(new PiecewiseFunctionRow {FromOperator = Operator.LessOrEqualThan, From = 0.5, Expression = "1"});
            }
            else if (type == ProcessingType.Bezier)
            {
                var c1 = new BezierCurve
                {
                    P0 = new Point(0, 0),
                    P1 = new Point(0.23, 0.57),
                    P2 = new Point(0.38, 1d),
                    P3 = new Point(0.5, 1d)
                };
                var c2 = new BezierCurve
                {
                    P0 = new Point(0.5, 1d),
                    P1 = new Point(0.6, 1d),
                    P2 = new Point(0.9, 1d),
                    P3 = new Point(1.00001, 1d),
                    Next = c1.Id
                };
                Bezier.ClearAndSetCurves(new List<BezierCurve> {c1, c2});
            }
        }

        private void CbOversampling_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (ComboBoxItem) CbOversampling.SelectedItem;
            int oversampling = int.Parse((string) item.Tag);
            Player.Oversampling = oversampling;
        }
    }
}
