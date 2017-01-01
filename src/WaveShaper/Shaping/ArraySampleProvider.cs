using NAudio.Wave;

namespace WaveShaper.Shaping
{
    public class ArraySampleProvider : WaveProvider32
    {
        private readonly float[] samples;

        public ArraySampleProvider(float[] samples, WaveFormat waveFormat)
        {
            this.samples = samples;
            SetWaveFormat(waveFormat.SampleRate, waveFormat.Channels);
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
