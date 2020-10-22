using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace VictorBot.Services.Audio
{
    public static class Util
    {
        public static Image ResizeImage(Image sourceImage, int width, int height)
        {
            return new Bitmap(sourceImage, width, height);
        }

        public static byte[] ReadToEnd(Stream stream)
        {
            byte[] readBuffer = new byte[81920];
            byte[] buffer = new byte[stream.Length];

            long remaining = stream.Length;

            stream.Position = 0;

            while (remaining > 0)
            {
                int readBytes = stream.Read(readBuffer, 0, Math.Min(buffer.Length, (int)remaining));
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, readBytes);

                remaining -= readBytes;
            }

            return buffer;
        }
    }
}
