using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace VictorBot.Services.Audio
{
    public class EarRapeSampleProvider : ISampleProvider
    {
        private ISampleProvider _sampleProvider;

        public EarRapeSampleProvider(ISampleProvider sampleProvider)
        {
            _sampleProvider = sampleProvider;
        }

        public bool EarRapeEnabled { get; set; } = false;

        public float EarRapeAmount { get; set; } = 1f;

        public WaveFormat WaveFormat => _sampleProvider.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = _sampleProvider.Read(buffer, offset, count);

            for (int i = 0; i < count; i++)
            {
                float value = buffer[offset + i];

                if (EarRapeEnabled)
                {
                    float threshold = 1f - EarRapeAmount;
                    if (value > threshold)
                    {
                        value = 1f;
                    }
                }

                buffer[offset + i] = value;
            }

            return samplesRead;
        }
    }
}
