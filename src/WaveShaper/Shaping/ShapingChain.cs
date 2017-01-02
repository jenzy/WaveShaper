using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace WaveShaper.Shaping
{
    public class ShapingChain
    {
        private int overSampling = 1;
        private float r = 1.0f;

        public ShapingChain(float[] inputSamples, WaveFormat inputWaveFormat, Func<double, double> shapingFunction = null)
        {
            Input = new ArraySampleProvider(inputSamples, inputWaveFormat);
            Shaper = new ShapingProvider(Input, shapingFunction);
            Output = Shaper;
        }

        public ArraySampleProvider Input { get; }

        public ShapingProvider Shaper { get; }

        public ISampleProvider Output { get; private set; }

        public int OverSampling
        {
            get { return overSampling; }
            set
            {
                if (overSampling == value)
                    return;

                overSampling = value;
                InitOverSampling(value, R);
            }
        }

        public float R
        {
            get { return r; }
            set
            {
                if (overSampling == value)
                    return;

                r = value;
                InitOverSampling(OverSampling, value);
            }
        }

        private void InitOverSampling(int oversampling, float r)
        {
            if (oversampling <= 0)
                throw new ArgumentOutOfRangeException(nameof(oversampling), oversampling, @"Oversampling cannot be negative 0.");

            if (oversampling == 1)
            {
                Shaper.Input = Input;
                Output = Shaper;
            }
            else
            {
                int originalSampleRate = Input.WaveFormat.SampleRate;
                int newSampleRate = originalSampleRate*oversampling;
                float cutoffFrequency = originalSampleRate/2f;

                var upsampler = new WdlResamplingSampleProvider(Input, newSampleRate);
                Shaper.Input = upsampler;
                var lpf = new LowPassFilterProvider(Shaper, cutoffFrequency, r);
                var downsampler = new WdlResamplingSampleProvider(lpf, Input.WaveFormat.SampleRate);
                Output = downsampler;
            }
        }

    }
}
