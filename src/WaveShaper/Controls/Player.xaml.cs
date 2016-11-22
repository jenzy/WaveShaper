using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using NAudio.Utils;
using NAudio.Wave;

namespace WaveShaper.Controls
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : UserControl
    {
        private WaveSample inputSamples;
        private WaveOut waveOut;
        private Prov samplesProvider;

        public Player()
        {
            InitializeComponent();
            SetButtonPlay(true);
            SetLabelTime(TimeSpan.Zero, TimeSpan.Zero);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (waveOut == null)
                return;

            SetLabelTime(waveOut.GetPositionTimeSpan(), inputSamples.TimeSpanLength);
        }

        private void BtnOpenFile_OnClick(object sender, RoutedEventArgs e)
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut = null;
            }

            var openFileDialog = new OpenFileDialog { Filter = "Audio Files|*.mp3;*.wav;*.wmp|All files (*.*)|*.*" };
            if (openFileDialog.ShowDialog() != true)
                return;

            var afr = new AudioFileReader(openFileDialog.FileName);
            var samples = new float[afr.Length / (afr.WaveFormat.BitsPerSample / 8)];

            const int bufSize = 1024;
            int offset = 0;
            while (true)
            {
                int sampleCount = Math.Min(bufSize, samples.Length - offset);
                int read = afr.Read(samples, offset, sampleCount);
                offset += read;
                if (read <= 0)
                    break;
            }

            inputSamples = new WaveSample(samples, afr.WaveFormat);
            LblFileTitle.Content = openFileDialog.FileName;
            SetLabelTime(TimeSpan.Zero, inputSamples.TimeSpanLength);
        }

        private void BtnPlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (BtnPlay.IsChecked == null || inputSamples == null)
                return;

            if (waveOut == null)
            {
                samplesProvider = new Prov(inputSamples);
                waveOut = new WaveOut();
                waveOut.Init(samplesProvider);
            }

            if (BtnPlay.IsChecked.Value)
                waveOut.Play();
            else
                waveOut.Pause();

            SetButtonPlay(!BtnPlay.IsChecked.Value);
        }

        private void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            if (waveOut == null)
                return;

            waveOut.Stop();
            samplesProvider.Position = 0;
            SetButtonPlay(true);
        }

        private void SetButtonPlay(bool play)
        {
            var bc = new BrushConverter();
            if (!play)
            {
                BtnPlay.Content = "❚❚";
                BtnPlay.Foreground = (Brush)bc.ConvertFrom("#FFB40000");
            }
            else
            {
                BtnPlay.Content = "▶";
                BtnPlay.Foreground = (Brush)bc.ConvertFrom("#FF008C00");
            }
        }

        private void SetLabelTime(TimeSpan current, TimeSpan total)
        {
            LblTime.Content = $"{current:mm\\:ss} / {total:mm\\:ss}";
        }
    }
}
