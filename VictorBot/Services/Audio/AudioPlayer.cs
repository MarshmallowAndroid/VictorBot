using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace VictorBot.Services.Audio
{
    public class AudioPlayer : IDisposable
    {
        private WaveStream currentStream;
        private ISampleProvider earRapeProvider;
        private ISampleProvider resampleProvider;
        private IWaveProvider waveProvider;
        private Stream destinationStream;

        private bool paused = false;
        private bool skipRequested = false;
        private bool stopRequested = false;

        private object lockObject = new object();

        public AudioPlayer(Stream outputStream)
        {
            AudioQueue = new Queue<Track>();
            destinationStream = outputStream;
        }

        public bool Playing { get; private set; } = false;

        public bool Loop { get => ((LoopStream)currentStream).Loop; set => ((LoopStream)currentStream).Loop = value; }

        public Queue<Track> AudioQueue { get; }

        public void Enqueue(Track track)
        {
            TrackQueuedEventArgs eventArgs = new(track);
            OnTrackQueued(eventArgs);

            AudioQueue.Enqueue(track);

            if (currentStream is null) Dequeue();
        }

        private void Dequeue()
        {
            currentStream?.Dispose();
            currentStream = AudioQueue.Dequeue().WaveStream;
            resampleProvider = new WdlResamplingSampleProvider(currentStream.ToSampleProvider(), 48000);
            earRapeProvider = new EarRapeSampleProvider(resampleProvider);
            waveProvider = earRapeProvider.ToWaveProvider16();
        }

        public void SetEarRapeAmount(string amount)
        {
            var earRape = earRapeProvider as EarRapeSampleProvider;
            earRape.EarRapeAmount = float.Parse(amount);
        }

        public void EarRape()
        {
            var earRape = earRapeProvider as EarRapeSampleProvider;
            earRape.EarRapeEnabled = !earRape.EarRapeEnabled;
        }

        public void Pause()
        {
            if (Playing) paused = !paused;
        }

        public void Stop()
        {
            if (Playing)
            {
                lock (lockObject)
                {
                    stopRequested = true;
                }
            }
        }

        public void Seek(long position)
        {
            currentStream.Position = position;
        }

        public void Skip()
        {
            if (Playing) skipRequested = true;
        }

        public void ChangeStream(Stream destinationStream)
        {
            if (Playing)
            {
                this.destinationStream.Dispose();
                this.destinationStream = destinationStream;
            }
        }

        public void BeginPlay(int bufferSize = 81920)
        {
            Task.Run(() => Play(bufferSize));
        }

        private void Play(int bufferSize = 81920)
        {
            byte[] buffer = new byte[bufferSize];

            Playing = true;

            while (!stopRequested)
            {
                int totalReadBytes = 0;

                lock (lockObject)
                {
                    if (!paused)
                    {
                        while (totalReadBytes < bufferSize)
                        {
                            int readBytes = waveProvider.Read(buffer, totalReadBytes, bufferSize - totalReadBytes);

                            if (readBytes == 0)
                            {
                                if (AudioQueue.Count > 0)
                                {
                                    OnTrackChanged(new TrackChangedEventArgs(TrackChangedReason.Ended, AudioQueue.Peek()));
                                    Dequeue();
                                }
                                else break;
                            }
                            totalReadBytes += readBytes;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < bufferSize; i++)
                        {
                            buffer[i] = 0;
                        }
                    }

                    if (stopRequested) break;
                    if (totalReadBytes > 0) destinationStream.Write(buffer, 0, totalReadBytes);

                    if (skipRequested)
                    {
                        skipRequested = false;

                        if (AudioQueue.Count > 0)
                        {
                            OnTrackChanged(new TrackChangedEventArgs(TrackChangedReason.Skipped, AudioQueue.Peek()));
                            Dequeue();
                        }
                        else currentStream.Position = currentStream.Length - 1;
                    }
                }
            }

            OnStopped();
            Playing = false;
            stopRequested = false;

            Dispose();
        }

        public void Dispose()
        {
            currentStream?.Dispose();
            currentStream = null;
            //destinationStream?.Dispose();
            AudioQueue.Clear();
            GC.SuppressFinalize(this);
        }

        protected virtual void OnTrackChanged(TrackChangedEventArgs e) => TrackChanged?.Invoke(this, e);
        protected virtual void OnTrackQueued(TrackQueuedEventArgs e) => TrackQueued?.Invoke(this, e);
        protected virtual void OnStopped() => Stopped?.Invoke();

        public event Action Stopped;
        public event EventHandler<TrackQueuedEventArgs> TrackQueued;
        public event EventHandler<TrackChangedEventArgs> TrackChanged;
    }

    public class TrackQueuedEventArgs : EventArgs
    {
        public TrackQueuedEventArgs(Track queuedTrack)
        {
            QueuedTrack = queuedTrack;
        }

        public Track QueuedTrack { get; }
    }

    public class TrackChangedEventArgs : EventArgs
    {
        public TrackChangedEventArgs(TrackChangedReason reason, Track newTrack)
        {
            Reason = reason;
            NewTrack = newTrack;
        }

        public TrackChangedReason Reason { get; }

        public Track NewTrack { get; }
    }

    public enum TrackChangedReason
    {
        Skipped,
        Stopped,
        Ended
    }
}
