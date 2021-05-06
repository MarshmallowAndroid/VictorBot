namespace VictorBot.Services.Audio
{

    public class TrackFile
    {
        public TrackFile(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public string Title { get; set; }

        public string Album { get; set; }

        public string Artist { get; set; }
    }
}
