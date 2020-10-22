using CSCore;
using CSCore.Tags.ID3;
using System.Drawing;

namespace VictorBot.Services
{
    public class Track
    {
        public Track(
            IWaveSource waveSource,
            string title = "Untitled",
            string artist = "Unknown Artist",
            string album = "Unknown Album",
            byte[] image = null)
        {
            WaveSource = waveSource;
            Title = title;
            Artist = artist;
            Album = album;
            Image = image;
        }

        public IWaveSource WaveSource { get; }

        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public byte[] Image { get; }
    }
}
