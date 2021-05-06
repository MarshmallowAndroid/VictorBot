using NAudio.Wave;

namespace VictorBot.Services.Audio
{
    public class LoopStream : WaveStream
    {
        private readonly WaveStream sourceStream;

        private int byteMultiplier;
        private int startPosition;
        private int endPosition;

        public LoopStream(WaveStream waveStream, int start = 0, int end = 0)
        {
            sourceStream = waveStream;
            byteMultiplier = (WaveFormat.BitsPerSample / 8) * WaveFormat.Channels;

            startPosition = start * byteMultiplier;
            endPosition = end * byteMultiplier;
        }

        public bool Loop { get; set; } = false;

        public override WaveFormat WaveFormat => sourceStream.WaveFormat;

        public override long Length => sourceStream.Length;

        public override long Position { get => sourceStream.Position; set => sourceStream.Position = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;
            int advanced = (int)(sourceStream.Position + count);

            while (totalBytesRead < count)
            {
                if (advanced >= endPosition && Loop)
                {
                    if (endPosition > startPosition)
                    {
                        int byteDifference = (int)(endPosition - sourceStream.Position);

                        if (byteDifference > 0) totalBytesRead += sourceStream.Read(buffer, offset, byteDifference);
                        sourceStream.Position = startPosition;
                    }
                }

                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (Loop) Position = startPosition;
                    else break;
                }

                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        protected override void Dispose(bool disposing)
        {
            sourceStream?.Dispose();
            base.Dispose(disposing);
        }
    }
}
