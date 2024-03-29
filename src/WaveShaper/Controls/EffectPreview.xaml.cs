﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using JetBrains.Annotations;
using OxyPlot;
using OxyPlot.Series;
using WaveShaper.Core.PiecewiseFunctions;

namespace WaveShaper.Controls
{
    /// <summary>
    /// Interaction logic for EffectPreview.xaml
    /// </summary>
    public partial class EffectPreview : Window, INotifyPropertyChanged
    {
        private PlotModel plotModel = new PlotModel();
        private readonly Func<double, double> shapingFunction;

        public EffectPreview(Func<double, double> shapingFunction)
        {
            this.shapingFunction = shapingFunction;
            InitializeComponent();
            Plot();
        }

        public PlotModel PlotModel
        {
            get => plotModel;
            set
            {
                if (Equals(value, plotModel)) return;
                plotModel = value;
                OnPropertyChanged();
            }
        }

        private void Plot()
        {
            try
            {
                Func<double, double> func = Math.Sin;
                const int @from = -1;
                const int to = 10;
                const int steps = 1000;

                PlotModel.Series.Clear();
                PlotModel.Series.Add(new FunctionSeries(func, from, to, steps, "s(t)") {Color = OxyColor.FromRgb(0, 0, 255)});
                PlotModel.Series.Add(new FunctionSeries(x => shapingFunction(func(x)), from, to, steps, "f(s(t))") { Color = OxyColor.FromRgb(255, 0, 0) });
                PlotModel.InvalidatePlot(true);
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
    }
}
