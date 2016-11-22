using System;
using NAudio.Wave;

namespace WaveShaper
{
    public class WaveSample
    {
        public WaveSample(float[] samples, WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
            Samples = samples;
        }

        public float[] Samples { get; private set; }

        public WaveFormat WaveFormat { get; private set; }

        public TimeSpan TimeSpanLength => TimeSpan.FromMilliseconds((double)(Samples.Length / (long)(WaveFormat.Channels)) * 1000.0 / (double)WaveFormat.SampleRate);
    }
}
