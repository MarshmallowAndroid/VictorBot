using Discord;
using Discord.Audio;
using Discord.Commands;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TagLib.Ogg;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using Timer = System.Threading.Timer;

namespace VictorBot.Services.Audio
{
    public partial class AudioService
    {
        public AudioService()
        {
            AudioClients = new Dictionary<ulong, IAudioClient>();
            Players = new Dictionary<ulong, AudioPlayer>();
            Timers = new Dictionary<ulong, Timer>();
        }

        public Dictionary<ulong, IAudioClient> AudioClients { get; set; }

        public Dictionary<ulong, AudioPlayer> Players { get; set; }

        public Dictionary<ulong, Timer> Timers { get; set; }

        public Track CreateTrackFromFile(string path)
        {
            var taglib = TagLib.File.Create(path);

            string title = taglib.Tag.Title ?? "Unknown Title";
            string artist = taglib.Tag.Performers.FirstOrDefault() ?? "Unknown Artist";
            string album = taglib.Tag.Album ?? "Unknown Album";
            byte[] image = null;

            if (taglib.Tag.Pictures.Length > 0) image = taglib.Tag.Pictures.First().Data.ToArray();

            int start = 0;
            int end = 0;

            if (Path.GetExtension(path) == ".ogg")
            {
                var comments = taglib.GetTag(TagLib.TagTypes.Xiph) as XiphComment;

                start = int.Parse(comments.GetFirstField("LOOPSTART"));
                end = int.Parse(comments.GetFirstField("LOOPLENGTH")) + start;

                return new Track(
                    new LoopStream(new VorbisWaveReader(path),
                        start, end),
                    title,
                    artist,
                    album,
                    image);
            }
            else
            {
                string loopFile = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + ".txt";
                if (System.IO.File.Exists(loopFile))
                {
                    using var file = System.IO.File.OpenText(loopFile);
                    start = int.Parse(file.ReadLine());
                    end = int.Parse(file.ReadLine());
                }

                return new Track(
                    new LoopStream(new AudioFileReader(path),
                        start, end),
                    title,
                    artist,
                    album,
                    image);
            }
        }

        public Track CreateTrackFromYouTubeStream(Video video, IStreamInfo streamInfo) =>
            new Track(
                new LoopStream(new MediaFoundationReader(streamInfo.Url)),
                video.Title,
                video.Author,
                "YouTube");

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
                using var pcmStream = audioClient.CreatePCMStream(AudioApplication.Music, 128 * 1024, 16, 100);
                //Players.Add(voiceChannelId, new AudioPlayer(new OpusEncodeStream(pcmStream, 128 * 1024, AudioApplication.Music, 100)));
                Players.Add(voiceChannelId, new AudioPlayer(pcmStream));

                using (var player = Players[voiceChannelId])
                {
                    timer.Change(Timeout.Infinite, Timeout.Infinite);

                    player.TrackQueued += async (s, e) =>
                    {
                        //await SendAudioInfoEmbedAsync("Queuing track", e.QueuedTrack, context);
                    };

                    player.TrackChanged += async (s, e) =>
                    {
                        //await SendAudioInfoEmbedAsync("Playing track", e.NewTrack, context);
                    };

                    await audioClient.SetSpeakingAsync(true);
                    player.Enqueue(track);
                    await player.PlayAsync();
                    await pcmStream.FlushAsync();
                    await audioClient.SetSpeakingAsync(false);
                }

                Players.Remove(voiceChannelId);

                timer.Change(30000, Timeout.Infinite);
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
                        await audioPlayer.PlayAsync();
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
    }
}
