﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using NAudio.Utils;
using NAudio.Wave;
using WaveShaper.Shaping;
using WaveShaper.Utilities;

namespace WaveShaper.Controls
{
    /// <summary>
    /// Interaction logic for Player.xaml
    /// </summary>
    public partial class Player : UserControl
    {
        private WaveSample inputSamples;
        private string currentFilename;
        private WaveOut waveOut;
        //private ShapingSampleProvider samplesProvider;
        private Func<double, double> shapingFunction;

        public Player()
        {
            InitializeComponent();
            SetButtonPlay(true);
            SetLabelTime(TimeSpan.Zero, TimeSpan.Zero);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public ShapingChain Chain { get; private set; }

        public Func<double, double> ShapingFunction
        {
            get { return Chain?.Shaper.ShapingFunction ?? shapingFunction ?? ShapingProvider.DefaultShapingFunction; }
            set
            {
                shapingFunction = value;
                if (Chain != null)
                    Chain.Shaper.ShapingFunction = value;
            }
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
                Chain = null;
            }

            var openFileDialog = new OpenFileDialog { Filter = "Audio Files|*.mp3;*.wav;*.wmp|All files (*.*)|*.*" };
            if (openFileDialog.ShowDialog() != true)
                return;

            using (new WaitCursor())
            {
                var afr = new AudioFileReader(openFileDialog.FileName);
                var samples = new float[afr.Length / (afr.WaveFormat.BitsPerSample / 8)];

                const int bufSize = 1024 * 1024;
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
                currentFilename = openFileDialog.FileName;
                LblFileTitle.Content = openFileDialog.FileName;
                SetLabelTime(TimeSpan.Zero, inputSamples.TimeSpanLength);

                Chain = new ShapingChain(samples, afr.WaveFormat, shapingFunction);
                Chain.OverSampling = 4;

                waveOut = new WaveOut();
                waveOut.PlaybackStopped += WaveOutOnPlaybackStopped;
                waveOut.Init(Chain.Output);
            }
        }

        private void WaveOutOnPlaybackStopped(object sender, StoppedEventArgs stoppedEventArgs)
        {
            BtnStop_OnClick(BtnStop, null);
        }

        private void BtnPlay_OnClick(object sender, RoutedEventArgs e)
        {
            if (BtnPlay.IsChecked == null)
                return;

            if (Chain == null)
            {
                BtnOpenFile_OnClick(BtnOpenFile, e);
            }

            if (BtnPlay.IsChecked.Value)
                waveOut.Play();
            else
                waveOut.Pause();

            SetButtonPlay(!BtnPlay.IsChecked.Value);
        }

        private void BtnSaveFile_OnClick(object sender, RoutedEventArgs e)
        {
            if (Chain == null)
                return;

            BtnStop_OnClick(BtnStop, e);

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = Path.GetFileNameWithoutExtension(currentFilename),
                DefaultExt = Path.GetExtension(currentFilename),
                Filter = "Wave Audio (.wav)|*.wav"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            using (new WaitCursor())
            {
                WaveFileWriter.CreateWaveFile(saveFileDialog.FileName, Chain.Output.ToWaveProvider());
            }
        }

        private void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            if (waveOut == null)
                return;

            waveOut.Stop();
            Chain.Input.Position = 0;
            SetButtonPlay(true);
            BtnPlay.IsChecked = false;
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
