using System;
using NAudio.Wave;
using WaveShaper.Utilities;

namespace WaveShaper
{
    public class ShapingSampleProvider : WaveProvider32
    {
        private readonly float[] samples;

        public static readonly Func<double, double> DefaultShapingFunction = x => x;

        public ShapingSampleProvider(WaveSample waveSample, Func<double, double> shapingFunction = null)
        {
            samples = waveSample.Samples;
            SetWaveFormat(waveSample.WaveFormat.SampleRate, waveSample.WaveFormat.Channels);
            ShapingFunction = shapingFunction ?? DefaultShapingFunction;
        }

        public int Position { get; set; }

        public Func<double, double> ShapingFunction { get; set; }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            if (Position >= samples.Length)
                return 0;

            for (int i = 0; i < sampleCount; i++)
            {
                buffer[i + offset] = (float) ShapingFunction(samples[Position]).Clamp(-1, 1);

                if (++Position >= samples.Length)
                    return i + 1;
            }

            return sampleCount;
        }
    }
}
