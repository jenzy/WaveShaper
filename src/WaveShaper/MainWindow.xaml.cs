using System.Windows;
using NAudio.Wave;

namespace WaveShaper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WaveSample inputSamples;
        private WaveOut waveOut;


        public MainWindow()
        {
            InitializeComponent();
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

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            if (Position >= samples.Length)
                return 0;

            for (int i = 0; i < sampleCount; i++)
            {
                buffer[i + offset] = samples[Position];

                if (++Position >= samples.Length)
                    return i + 1;
            }

            return sampleCount;
        }
    }
}
