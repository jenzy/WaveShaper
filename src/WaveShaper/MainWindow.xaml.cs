using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Jace;
using NAudio.Wave;
using WaveShaper.Annotations;

namespace WaveShaper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Item> items = new ObservableCollection<Item>
        {
            new Item {From = -1, FromOperator = Operator.LessOrEqualThan, To = 1, ToOperator = Operator.LessOrEqualThan, Expression = "x"}
        };

        public ObservableCollection<Item> Items
        {
            get { return items; }
            set
            {
                if (Equals(value, items)) return;
                items = value;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }


        private void DdlProcessingType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var a = DdlProcessingType.SelectedValue;
            //TODO

            //var calc = (Func<double, double>) new CalculationEngine().Formula("x+1")
            //                                  .Parameter("x", DataType.FloatingPoint)
            //                                  .Result(DataType.FloatingPoint)
            //                                  .Build();

            //var b = calc(5);

            //if(dataGrid != null)
            //    dataGrid.ItemsSource = Items;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            var mode = (ProcessingType) DdlProcessingType.SelectedValue;


            if (mode == ProcessingType.Piecewise)
            {
                var items = Items;

                var function = new PiecewiseFunction<double>();
                var engine = new CalculationEngine();
                foreach (Item item in items)
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
    }

        class Prov : WaveProvider32
    {
        private readonly float[] samples;

        public Prov(WaveSample waveSample)
        {
            samples = waveSample.Samples;
            SetWaveFormat(waveSample.WaveFormat.SampleRate, waveSample.WaveFormat.Channels);
        }

        public int Position { get; set; }

        public Func<double, double> ShapingFunction { get; set; } = d => d;

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            if (Position >= samples.Length)
                return 0;

            for (int i = 0; i < sampleCount; i++)
            {
                buffer[i + offset] = (float) ShapingFunction(samples[Position]);

                if (++Position >= samples.Length)
                    return i + 1;
            }

            return sampleCount;
        }
    }
}
