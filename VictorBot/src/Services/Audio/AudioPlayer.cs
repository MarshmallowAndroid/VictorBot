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
        private readonly Stream _destinationStream;

        private bool playing = false;
        private bool paused = false;
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
            AudioQueue = new Queue<Track>();

            _destinationStream = destinationStream;
        }

        public bool Loop { get; set; }

        public Queue<Track> AudioQueue { get; }

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

            AudioQueue.Enqueue(track);

            if (!playing) currentSource = AudioQueue.Dequeue().WaveSource;
        }

        public void SetEarRapeParams(string paramsString)
        {
            string[] splitString = paramsString.Split(' ');

            var distortion = currentSource as DmoDistortionEffect;

            distortion.Edge = float.Parse(splitString[0]);
            distortion.Gain = int.Parse(splitString[1]);
            distortion.PostEQBandwidth = int.Parse(splitString[2]);
            distortion.PostEQCenterFrequency = int.Parse(splitString[3]);
            distortion.PreLowpassCutoff = int.Parse(splitString[4]);
        }

        public void EarRape()
        {
            var distortion = currentSource as DmoDistortionEffect;
            distortion.IsEnabled = !distortion.IsEnabled;
        }

        public void PlayPause()
        {
            paused = !paused;
        }

        public void Stop()
        {
            stopRequested = true;
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

                        currentSource = AudioQueue.Dequeue().WaveSource;
                    }
                    else break;
                }

                if (!paused)
                {
                    try
                    {
                        int readBytes = currentSource.Read(buffer, 0, buffer.Length);
                        if (readBytes > 0)
                        {
                            _destinationStream.Write(buffer, 0, buffer.Length);

                            Console.WriteLine("Position: " + currentSource.Position + ", Length: " + currentSource.Length);

                            playing = true;
                        }
                        else
                        {
                            if (Loop)
                            {
                                currentSource.Position = 0;
                            }
                            else if (AudioQueue.Count > 0)
                            {
                                TrackChangedEventArgs eventArgs = new TrackChangedEventArgs(TrackChangedReason.Ended, AudioQueue.Peek());
                                OnTrackChanged(eventArgs);

                                currentSource.Dispose();
                                currentSource = AudioQueue.Dequeue().WaveSource;
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
            currentSource?.Dispose();
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
