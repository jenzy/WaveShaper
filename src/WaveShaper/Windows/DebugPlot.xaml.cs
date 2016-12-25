using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using OxyPlot;
using WaveShaper.Annotations;

namespace WaveShaper.Windows
{
    /// <summary>
    /// Interaction logic for EffectPreview.xaml
    /// </summary>
    public partial class DebugPlot : Window, INotifyPropertyChanged
    {
        private PlotModel plotModel;

        public DebugPlot(PlotModel model)
        {
            InitializeComponent();
            PlotModel = model;
        }

        public PlotModel PlotModel
        {
            get { return plotModel; }
            set
            {
                if (Equals(value, plotModel)) return;
                plotModel = value;
                OnPropertyChanged();
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

    }
}
