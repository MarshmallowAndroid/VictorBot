using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Text;

namespace VictorBot.Services.Audio
{
    public class LoopStream : WaveStream
    {
        private readonly WaveStream currentStream;

        private int byteMultiplier;
        private int startBytes;
        private int endBytes;

        public LoopStream(WaveStream waveStream, int start, int end)
        {
            currentStream = waveStream;
            byteMultiplier = (WaveFormat.BitsPerSample / 8) * WaveFormat.Channels;

            startBytes = start * byteMultiplier;
            endBytes = end * byteMultiplier;
        }

        public bool Loop { get; set; } = false;

        public override WaveFormat WaveFormat => currentStream.WaveFormat;

        public override long Length => currentStream.Length;

        public override long Position { get => currentStream.Position; set => currentStream.Position = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            int advanced = (int)(currentStream.Position + count);

            if (endBytes > 0)
            {
                if (advanced > endBytes && Loop)
                {
                    int byteDifference = (int)(endBytes - currentStream.Position);
                    bytesRead = currentStream.Read(buffer, offset, byteDifference);
                    Console.WriteLine("Sample difference: " + byteDifference + "\nSamples read: " + bytesRead);
                    Console.WriteLine("Ended at sample " + currentStream.Position / byteMultiplier);
                    currentStream.Position = startBytes;
                    Console.WriteLine("Restarted at sample " + currentStream.Position / byteMultiplier);
                }
            }

            bytesRead += currentStream.Read(buffer, offset + bytesRead, count - bytesRead);

            //Console.WriteLine(bytesRead + " vs. " + count);

            return bytesRead;
        }
    }
}
