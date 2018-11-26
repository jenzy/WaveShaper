using System;
using NAudio.Wave;

namespace WaveShaper.Core.Samples
{
    public class WaveSample
    {
        public WaveSample(float[] samples, WaveFormat waveFormat)
        {
            WaveFormat = waveFormat;
            Samples = samples;
        }

        public float[] Samples { get; }

        public WaveFormat WaveFormat { get; }

        // ReSharper disable once PossibleLossOfFraction
        public TimeSpan TimeSpanLength => TimeSpan.FromMilliseconds((Samples.Length / (long)WaveFormat.Channels) * 1000.0 / WaveFormat.SampleRate);
    }
}
