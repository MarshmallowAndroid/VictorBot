using NAudio.Wave;

namespace VictorBot.Services.Audio
{
    public class LoopStream : WaveStream
    {
        private readonly WaveStream sourceStream;

        private int byteMultiplier;
        private int startBytes;
        private int endBytes;

        public LoopStream(WaveStream waveStream, int start = 0, int end = 0)
        {
            sourceStream = waveStream;
            byteMultiplier = (WaveFormat.BitsPerSample / 8) * WaveFormat.Channels;

            startBytes = start * byteMultiplier;
            endBytes = end * byteMultiplier;
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
                if (advanced >= endBytes && Loop)
                {
                    if (endBytes > startBytes)
                    {
                        int byteDifference = (int)(endBytes - sourceStream.Position);

                        totalBytesRead += sourceStream.Read(buffer, offset, byteDifference);
                        //Console.WriteLine("Byte difference: " + byteDifference + "\nBytes read: " + bytesRead);
                        //Console.WriteLine("Ended at sample " + currentStream.Position / byteMultiplier);
                        sourceStream.Position = startBytes;
                        //Console.WriteLine("Restarted at sample " + currentStream.Position / byteMultiplier);
                    }
                }

                int bytesRead;

                bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (Loop) Position = startBytes;
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
