using System;
using NAudio.Wave;

namespace WaveShaper.Shaping
{
    public class ShapingProvider : WaveProvider32
    {
        public static readonly Func<double, double> DefaultShapingFunction = x => x;
        private ISampleProvider input;

        public ShapingProvider(ISampleProvider input, Func<double, double> shapingFunction = null)
        {
            this.Input = input;
            this.ShapingFunction = shapingFunction ?? DefaultShapingFunction;
        }

        public ISampleProvider Input
        {
            get { return input; }
            set
            {
                input = value;
                SetWaveFormat(value.WaveFormat.SampleRate, value.WaveFormat.Channels);
            }
        }

        public Func<double, double> ShapingFunction { get; set; }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            int samplesRead = Input.Read(buffer, offset, sampleCount);

            for (int i = 0; i < samplesRead; i++)
                buffer[i + offset] = (float) ShapingFunction(buffer[i + offset]);//.Clamp(-1, 1);

            return samplesRead;
        }
    }
}
