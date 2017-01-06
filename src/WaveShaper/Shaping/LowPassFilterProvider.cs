using NAudio.Dsp;
using NAudio.Wave;

namespace WaveShaper.Shaping
{
    public class LowPassFilterProvider : WaveProvider32
    {
        private readonly ISampleProvider source;
        private readonly BiQuadFilter[] filters;
        private readonly float cutoffFrequency;
        private readonly float q;

        public LowPassFilterProvider(ISampleProvider source, float cutoffFrequency, float q)
        {
            this.source = source;
            this.cutoffFrequency = cutoffFrequency;
            this.q = q;
            this.filters = new BiQuadFilter[source.WaveFormat.Channels];
            SetWaveFormat(source.WaveFormat.SampleRate, source.WaveFormat.Channels);

            InitFilters();
        }

        public override int Read(float[] buffer, int offset, int sampleCount)
        {
            int samplesRead = source.Read(buffer, offset, sampleCount);

            int channels = source.WaveFormat.Channels;
            for (int i = 0; i < samplesRead; i++)
                buffer[offset + i] = filters[(i % channels)].Transform(buffer[offset + i]);

            return samplesRead;
        }

        private void InitFilters()
        {
            for (int channel = 0; channel < source.WaveFormat.Channels; channel++)
            {
                if (filters[channel] == null)
                    filters[channel] = BiQuadFilter.LowPassFilter(source.WaveFormat.SampleRate, cutoffFrequency, q);
                else
                    filters[channel].SetLowPassFilter(source.WaveFormat.SampleRate, cutoffFrequency, q);
            }
        }
    }
}
