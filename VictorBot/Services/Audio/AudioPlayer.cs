using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBot.Services.Audio
{
    public class AudioPlayer : IDisposable
    {
        private WaveStream currentStream;
        private ISampleProvider earRapeProvider;
        private ISampleProvider resampleProvider;
        private IWaveProvider waveProvider;
        private readonly Stream destinationStream;

        private bool playing = false;
        private bool paused = false;
        private bool stopRequested = false;
        private bool skipRequested = false;

        public AudioPlayer(Stream outputStream)
        {
            AudioQueue = new Queue<Track>();

            destinationStream = outputStream;
        }

        public bool Loop { get => ((LoopStream)currentStream).Loop; set => ((LoopStream)currentStream).Loop = value; }

        public Queue<Track> AudioQueue { get; }

        public void Enqueue(Track track)
        {
            TrackQueuedEventArgs eventArgs = new TrackQueuedEventArgs(track);
            OnTrackQueued(eventArgs);

            AudioQueue.Enqueue(track);

            if (!playing)
            {
                Dequeue();
            }
        }

        private void Dequeue()
        {
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

        public void PlayPause()
        {
            paused = !paused;
        }

        public void Stop()
        {
            stopRequested = true;
        }

        public void Seek(long position)
        {
            currentStream.Position = position;
        }

        public void Skip()
        {
            skipRequested = true;
        }

        public Task BeginPlay(int bufferSize = 81920)
        {
            byte[] buffer = new byte[bufferSize];

            while (true)
            {
                if (stopRequested)
                {
                    Dispose();

                    stopRequested = false;
                    break;
                }

                if (skipRequested)
                {
                    skipRequested = false;

                    if (AudioQueue.Count > 0)
                    {
                        TrackChangedEventArgs eventArgs = new TrackChangedEventArgs(TrackChangedReason.Skipped, AudioQueue.Peek());
                        OnTrackChanged(eventArgs);

                        Dequeue();
                    }
                    else break;
                }

                if (!paused)
                {
                    try
                    {
                        int readBytes = waveProvider.Read(buffer, 0, buffer.Length);
                        if (readBytes > 0)
                        {
                            destinationStream.Write(buffer, 0, buffer.Length);
                            playing = true;
                        }
                        else
                        {
                            if (Loop)
                            {
                                currentStream.Position = 0;
                            }
                            else if (AudioQueue.Count > 0)
                            {
                                TrackChangedEventArgs eventArgs = new TrackChangedEventArgs(TrackChangedReason.Ended, AudioQueue.Peek());
                                OnTrackChanged(eventArgs);

                                currentStream.Dispose();
                                Dequeue();
                            }
                            else break;
                        }
                    }
                    catch (Exception)
                    {
                        playing = false;
                    }
                }

            }

            playing = false;

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            currentStream?.Dispose();
            AudioQueue.Clear();
        }

        protected virtual void OnTrackChanged(TrackChangedEventArgs e) => TrackChanged?.Invoke(this, e);
        protected virtual void OnTrackQueued(TrackQueuedEventArgs e) => TrackQueued?.Invoke(this, e);

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
        Ended
    }
}
