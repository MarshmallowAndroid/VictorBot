using CSCore;
using CSCore.DMO.Effects;
using CSCore.Streams;
using CSCore.Streams.Effects;
using CSCore.Tags.ID3;
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
        private IWaveSource currentSource;
        private readonly Queue<Track> queue;
        private readonly Stream destinationStream;

        private readonly object lockObject = new object();

        private bool playing = false;
        private bool paused = false;
        private bool loop = false;
        private bool stopRequested = false;
        private bool skipRequested = false;

        //public AudioPlayer(IWaveSource sourceStream)
        //{
        //    queue = new Queue<IWaveSource>();
        //    queue.Enqueue(sourceStream);

        //    currentStream = queue.Dequeue();
        //}

        public AudioPlayer(Stream destinationStream)
        {
            queue = new Queue<Track>();

            this.destinationStream = destinationStream;
        }

        public void Enqueue(Track track)
        {
            //queue.Enqueue(waveSource);
            //queue.Enqueue(new DmoDistortionEffect(WaveSource)
            //{
            //    IsEnabled = false,
            //    Gain = 0.0f,
            //    PostEQBandwidth = 7200,
            //    PostEQCenterFrequency = 4800
            //});

            //new DmoDistortionEffect(WaveSource)
            //{
            //    IsEnabled = false,
            //    Gain = 0.0f,
            //    PostEQBandwidth = 7200,
            //    PostEQCenterFrequency = 4800
            //}

            TrackQueuedEventArgs eventArgs = new TrackQueuedEventArgs(track);
            OnTrackQueued(eventArgs);

            queue.Enqueue(track);

            if (!playing) currentSource = queue.Dequeue().WaveSource;
        }

        public void SetEarRapeParams(string paramsString)
        {
            lock (lockObject)
            {
                string[] splitString = paramsString.Split(' ');

                var distortion = currentSource as DmoDistortionEffect;

                distortion.Edge = float.Parse(splitString[0]);
                distortion.Gain = int.Parse(splitString[1]);
                distortion.PostEQBandwidth = int.Parse(splitString[2]);
                distortion.PostEQCenterFrequency = int.Parse(splitString[3]);
                distortion.PreLowpassCutoff = int.Parse(splitString[4]);
            }
        }

        public void EarRape()
        {
            lock (lockObject)
            {
                var distortion = currentSource as DmoDistortionEffect;
                distortion.IsEnabled = !distortion.IsEnabled;
            }
        }

        public void PlayPause()
        {
            lock (lockObject) paused = !paused;
        }

        public void Stop()
        {
            lock (lockObject) stopRequested = true;
        }

        public void Loop()
        {
            lock (lockObject) loop = !loop;
        }

        public void Skip()
        {
            lock (lockObject) skipRequested = true;
        }

        public Queue<Track> AudioQueue
        {
            get { lock (lockObject) { return queue; } }
        }

        public async Task BeginPlayAsync(int bufferSize = 81920)
        {
            await Task.Run(() =>
            {
                byte[] buffer = new byte[bufferSize];
                int readBytes = 0;

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

                        if (queue.Count > 0)
                        {
                            TrackChangedEventArgs eventArgs = new TrackChangedEventArgs(TrackChangedReason.Skipped, queue.Peek());
                            OnTrackChanged(eventArgs);

                            currentSource = queue.Dequeue().WaveSource;
                        }
                        else break;
                    }

                    if (!paused)
                    {
                        try
                        {
                            readBytes = currentSource.Read(buffer, 0, buffer.Length);

                            if (readBytes > 0)
                            {
                                destinationStream.Write(buffer, 0, buffer.Length);

                                Console.WriteLine("Position: " + currentSource.Position + ", Length: " + currentSource.Length);

                                playing = true;
                            }
                            else
                            {
                                if (loop)
                                {
                                    currentSource.Position = 0;
                                }
                                else if (queue.Count > 0)
                                {
                                    TrackChangedEventArgs eventArgs = new TrackChangedEventArgs(TrackChangedReason.Ended, queue.Peek());
                                    OnTrackChanged(eventArgs);

                                    currentSource.Dispose();
                                    currentSource = queue.Dequeue().WaveSource;
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
            });
        }

        public void Dispose()
        {
            currentSource?.Dispose();
            queue.Clear();
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
