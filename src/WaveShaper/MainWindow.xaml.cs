using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Jace;
using WaveShaper.Annotations;

namespace WaveShaper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<PiecewiseFunctionRow> piecewiseFunctionRows = new ObservableCollection<PiecewiseFunctionRow>
        {
            new PiecewiseFunctionRow
            {
                From = -1,
                FromOperator = Operator.LessOrEqualThan,
                To = 1,
                ToOperator = Operator.LessOrEqualThan,
                Expression = "x"
            }
        };

        public MainWindow()
        {
            InitializeComponent();
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


        private void DdlProcessingType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
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

                Player.ShapingFunction = function.Calculate;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

        
}
