using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Jace;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using WaveShaper.Annotations;
using WaveShaper.Utilities;

namespace WaveShaper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private PlotModel shapingFunctionPlot;

        private ObservableCollection<PiecewiseFunctionRow> piecewiseFunctionRows = new ObservableCollection<PiecewiseFunctionRow>
        {
            new PiecewiseFunctionRow
            {
                FromOperator = Operator.LessOrEqualThan,
                ToOperator = Operator.LessOrEqualThan,
                Expression = "x"
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
                                   ExtraGridlines = new[] {0d},
                                   ExtraGridlineColor = OxyColors.LightGray,
                                   Title = "f(x)"
            });
            shapingFunctionPlot.Axes.Add(new LinearAxis
                               {
                                   Position = AxisPosition.Bottom,
                                   Minimum = -1.1,
                                   Maximum = 1.1,
                                   ExtraGridlines = new[] {0d},
                                   ExtraGridlineColor = OxyColors.LightGray,
                                   Title = "x"
            });
        }

        public ObservableCollection<PiecewiseFunctionRow> PiecewiseFunctionRows
        {
            get { return piecewiseFunctionRows; }
            set
            {
                if (Equals(value, piecewiseFunctionRows)) return;
                piecewiseFunctionRows = value;
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

            switch ((ProcessingType) DdlProcessingType.SelectedValue)
            {
                case ProcessingType.NoProcessing:
                    TabControl.SelectedItem = TabNone;
                    break;

                case ProcessingType.PiecewiseFunction:
                    TabControl.SelectedItem = TabTable;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BtnApply_OnClick(object sender, RoutedEventArgs e)
        {
            var mode = (ProcessingType) DdlProcessingType.SelectedValue;

            if (mode == ProcessingType.NoProcessing)
            {
                Player.ShapingFunction = ShapingSampleProvider.DefaultShapingFunction;
            }
            else if (mode == ProcessingType.PiecewiseFunction)
            {
                var items = PiecewiseFunctionRows;

                var function = new PiecewiseFunction<double>();
                var engine = new CalculationEngine();
                foreach (PiecewiseFunctionRow item in items)
                {
                    var piece = new Piece<double>()
                    {
                        Condition = item.GetCondition(),
                        Function = (Func<double, double>) engine.Formula(item.Expression)
                                                                .Parameter("x", DataType.FloatingPoint)
                                                                .Result(DataType.FloatingPoint).Build()
                    };
                    function.AddPiece(piece);
                }

                function.Preprocess = x => x.Clamp(-1, 1);
                Player.ShapingFunction = function.Calculate;
            }

            PlotShapingFunction(Player.ShapingFunction);
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
            switch (param)
            {
                case "Identity":
                    PiecewiseFunctionRows.Clear();
                    PiecewiseFunctionRows.Add(new PiecewiseFunctionRow
                    {
                        FromOperator = Operator.LessOrEqualThan,
                        ToOperator = Operator.LessOrEqualThan,
                        Expression = "x"
                    });
                    break;

                case "SoftClipping":
                    PiecewiseFunctionRows.Clear();
                    PiecewiseFunctionRows.Add(new PiecewiseFunctionRow {ToOperator = Operator.LessOrEqualThan, To = -1, Expression = "-2/3"});
                    PiecewiseFunctionRows.Add(new PiecewiseFunctionRow
                    {
                        From = -1,
                        FromOperator = Operator.LessThan,
                        ToOperator = Operator.LessThan,
                        To = 1,
                        Expression = "x - (x^3)/3"
                    });
                    PiecewiseFunctionRows.Add(new PiecewiseFunctionRow {FromOperator = Operator.LessOrEqualThan, From = 1, Expression = "2/3"});
                    break;
            }

        }
    }
}
