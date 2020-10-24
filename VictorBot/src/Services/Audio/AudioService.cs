using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSCore;
using CSCore.Codecs;
using VictorBot.Services.Audio;
using CSCore.Tags.ID3;
using CSCore.Streams.Effects;
using Discord.WebSocket;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;
using TagLib;
using System.Timers;
using Timer = System.Threading.Timer;
using CSCore.Codecs.WAV;
using Discord.Audio.Streams;

namespace VictorBot.Services
{
    public partial class AudioService
    {
        public AudioService()
        {
            AudioClients = new Dictionary<ulong, IAudioClient>();
            Players = new Dictionary<ulong, AudioPlayer>();
            Timers = new Dictionary<ulong, Timer>();
        }

        public Track CreateTrackFromFile(string path)
        {
            var taglib = TagLib.File.Create(path);

            string title = taglib.Tag.Title;
            string artist = taglib.Tag.Performers[0];
            string album = taglib.Tag.Album;
            byte[] image = null;

            if (taglib.Tag.Pictures.Length > 0) image = taglib.Tag.Pictures[0].Data.ToArray();

            return new Track(
                new DmoDistortionEffect(CodecFactory.Instance.GetCodec(path).ChangeSampleRate(48000))
                {
                    IsEnabled = false,
                },
                title,
                artist,
                album,
                image);
        }

        public Track CreateTrackFromYouTubeStream(Video video, IStreamInfo streamInfo)
        {
            return new Track(
                new DmoDistortionEffect(CodecFactory.Instance.GetCodec(new Uri(streamInfo.Url)).ChangeSampleRate(48000))
                {
                    IsEnabled = false,
                    //Gain = 0.0f,
                    //PostEQBandwidth = 7200,
                    //PostEQCenterFrequency = 4800
                },
                video.Title,
                video.Author);
        }

        public Task PlayPauseAsync(SocketCommandContext context)
        {
            if (IsInVoiceChannel(context))
            {
                var voiceChannelId = ((IGuildUser)context.User).VoiceChannel.Id;
                Players[voiceChannelId].PlayPause();
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(SocketCommandContext context)
        {
            if (IsInVoiceChannel(context))
            {
                var voiceChannelId = ((IGuildUser)context.User).VoiceChannel.Id;
                Players[voiceChannelId].Stop();
            }
            return Task.CompletedTask;
        }

        public Task LoopAsync(SocketCommandContext context)
        {
            if (IsInVoiceChannel(context))
            {
                var voiceChannelId = ((IGuildUser)context.User).VoiceChannel.Id;
                Players[voiceChannelId].Loop = !Players[voiceChannelId].Loop;
            }
            return Task.CompletedTask;
        }

        public Task SkipAsync(SocketCommandContext context)
        {
            if (IsInVoiceChannel(context))
            {
                var voiceChannelId = ((IGuildUser)context.User).VoiceChannel.Id;
                Players[voiceChannelId].Skip();
            }
            return Task.CompletedTask;
        }

        public Task EarRapeAsync(SocketCommandContext context)
        {
            if (IsInVoiceChannel(context))
            {
                var voiceChannelId = ((IGuildUser)context.User).VoiceChannel.Id;
                Players[voiceChannelId].EarRape();
            }
            return Task.CompletedTask;
        }

        public Task SetEarRapeParamsAsync(SocketCommandContext context, string paramsString)
        {
            if (IsInVoiceChannel(context))
            {
                var voiceChannelId = ((IGuildUser)context.User).VoiceChannel.Id;
                Players[voiceChannelId].SetEarRapeParams(paramsString);
            }
            return Task.CompletedTask;
        }

        public async Task<ulong> JoinVoiceChannelAsync(SocketCommandContext context)
        {
            var userVoiceChannel = ((IGuildUser)context.User).VoiceChannel;
            if (userVoiceChannel == null) return 0;

            var voiceChannelId = userVoiceChannel.Id;

            IAudioClient audioClient;
            if (!AudioClients.ContainsKey(voiceChannelId))
            {
                audioClient = await userVoiceChannel.ConnectAsync();

                audioClient.Disconnected += (e) =>
                {
                    AudioClients.Remove(voiceChannelId);
                    return Task.CompletedTask;
                };

                AudioClients.Add(voiceChannelId, audioClient);

                if (!Timers.ContainsKey(voiceChannelId))
                    Timers.Add(voiceChannelId, new Timer(CheckPlaying, voiceChannelId, Timeout.Infinite, Timeout.Infinite));
            }

            return voiceChannelId;
        }

        public bool IsInVoiceChannel(SocketCommandContext context)
        {
            var userVoiceChannel = ((IGuildUser)context.User).VoiceChannel;
            if (userVoiceChannel == null) return false;

            var voiceChannelId = userVoiceChannel.Id;

            if (!AudioClients.ContainsKey(voiceChannelId))
            {
                return false;
            }

            return true;
        }

        private void CheckPlaying(object state)
        {
            var voiceChannelId = (ulong)state;

            if (!Players.ContainsKey(voiceChannelId))
            {
                if (AudioClients.ContainsKey(voiceChannelId))
                {
                    AudioClients[voiceChannelId].StopAsync();
                }
            }
        }

        public async Task PlayFileAsync(string path, SocketCommandContext context)
        {
            var voiceChannelId = await JoinVoiceChannelAsync(context);
            var audioClient = AudioClients[voiceChannelId];
            var timer = Timers[voiceChannelId];

            var track = CreateTrackFromFile(path);

            if (!Players.ContainsKey(voiceChannelId))
            {
                Console.WriteLine("PlayFileAsync: Playing file...");

                //using (var pcmStream = audioClient.CreateOpusStream())
                using (var pcmStream = audioClient.CreatePCMStream(AudioApplication.Mixed, 128 * 1024, 16, 100))
                {
                    //Players.Add(voiceChannelId, new AudioPlayer(new OpusEncodeStream(pcmStream, 128 * 1024, AudioApplication.Music, 100)));
                    Players.Add(voiceChannelId, new AudioPlayer(pcmStream));

                    using (var player = Players[voiceChannelId])
                    {
                        timer.Change(Timeout.Infinite, Timeout.Infinite);

                        player.TrackQueued += async (s, e) =>
                        {
                            await SendAudioInfoEmbedAsync("Queuing track", e.QueuedTrack, context);
                        };

                        player.TrackChanged += async (s, e) =>
                        {
                            await SendAudioInfoEmbedAsync("Playing track", e.NewTrack, context);
                        };

                        await audioClient.SetSpeakingAsync(true);
                        player.Enqueue(track);
                        await player.BeginPlay();
                        await audioClient.SetSpeakingAsync(false);
                    }

                    Players.Remove(voiceChannelId);

                    timer.Change(30000, Timeout.Infinite);
                }
            }
            else
            {
                Console.WriteLine("PlayFileAsync: Queuing file...");

                Players[voiceChannelId].Enqueue(track);
            }

            Console.WriteLine("PlayFileAsync: Done.");
        }

        public async Task PlayYouTubeAsync(Video video, IStreamInfo streamInfo, SocketCommandContext context)
        {
            var voiceChannelId = await JoinVoiceChannelAsync(context);
            var audioClient = AudioClients[voiceChannelId];

            var waveSource = CreateTrackFromYouTubeStream(video, streamInfo);

            if (!Players.ContainsKey(voiceChannelId))
            {
                using (var pcmStream = audioClient.CreatePCMStream(AudioApplication.Music, 128 * 1024))
                {
                    Players.Add(voiceChannelId, new AudioPlayer(pcmStream));

                    using (var audioPlayer = Players[voiceChannelId])
                    {
                        await audioClient.SetSpeakingAsync(true);
                        audioPlayer.Enqueue(waveSource);
                        await audioPlayer.BeginPlay();
                        await audioClient.SetSpeakingAsync(false);
                    }
                }

                Players.Remove(voiceChannelId);
            }
            else
            {
                Players[voiceChannelId].Enqueue(waveSource);
            }

            Console.WriteLine("PlayUrlAsync: Done.");
        }

        public async Task SendAudioInfoEmbedAsync(string title, Track track, SocketCommandContext context)
        {
            var image = track.Image;
            Stream imageStream = null;
            if (image != null) imageStream = new MemoryStream(image);

            var embed = new EmbedBuilder()
            {
                Title = title,
                Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        IsInline = true,
                        Name = "Title",
                        Value = track.Title
                    },
                    new EmbedFieldBuilder()
                    {
                        IsInline = true,
                        Name = "Artist",
                        Value = track.Artist
                    },
                    new EmbedFieldBuilder()
                    {
                        IsInline = false,
                        Name = "Album",
                        Value = track.Album
                    },
                },
                ThumbnailUrl = imageStream != null ? "attachment://cover.png" : ""
            };

            if (imageStream != null)
                await context.Channel.SendFileAsync(stream: imageStream ?? null, filename: "cover.png", embed: embed.Build());
            else
                await context.Channel.SendMessageAsync(embed: embed.Build());
        }

        public Dictionary<ulong, IAudioClient> AudioClients { get; set; }

        public Dictionary<ulong, AudioPlayer> Players { get; set; }

        public Dictionary<ulong, Timer> Timers { get; set; }
    }
}
