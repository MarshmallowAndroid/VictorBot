using NAudio.Wave;
using System;

namespace VictorBot.Services
{
    public class Track : IDisposable
    {
        public Track(
            WaveStream waveStream,
            string title,
            string artist,
            string album,
            byte[] image = null)
        {
            WaveStream = waveStream;
            Title = title;
            Artist = artist;
            Album = album;
            Image = image;
        }

        public WaveStream WaveStream { get; }

        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public byte[] Image { get; }

        public void Dispose()
        {
            WaveStream?.Dispose();
        }
    }
}
