using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VictorBot.Services.Audio
{
    public class GuildContext
    {
        private readonly ICommandContext commandContext;

        private IAudioClient audioClient;
        private Timer timer;

        public GuildContext(ICommandContext context)
        {
            commandContext = context;

            UserStates = new List<UserState>();
        }

        public IGuild Guild => commandContext.Guild;

        public List<UserState> UserStates { get; }

        public AudioPlayer Player { get; private set; }

        public async Task JoinVoiceChannelAsync()
        {
            var voiceChannel = (commandContext.User as IGuildUser).VoiceChannel;

            if (voiceChannel != null && audioClient == null)
            {
                audioClient = await voiceChannel.ConnectAsync();
                audioClient.Disconnected += AudioClient_Disconnected;
            }

            if (timer == null)
            {
                timer = new Timer(CheckPlaying, null, -1, -1);
            }
        }

        public bool IsUserInVoiceChannel()
        {
            var voiceChannel = (commandContext.User as IGuildUser).VoiceChannel;
            if (voiceChannel == null) return false;
            return true;
        }

        public async Task PlayTrackAsync(Track track)
        {
            await JoinVoiceChannelAsync();

            if (Player == null)
            {
                Player = new AudioPlayer(audioClient.CreatePCMStream(AudioApplication.Music, 128 * 1024, 100));

                Player.TrackQueued += async (s, e) =>
                {
                    await SendAudioInfoEmbedAsync("Queuing track", e.QueuedTrack);
                };

                Player.TrackChanged += async (s, e) =>
                {
                    await SendAudioInfoEmbedAsync("Playing track", e.NewTrack);
                };

                Player.Stopped += () =>
                {
                    timer.Change(0, 30000);
                };
            }

            timer.Change(-1, -1);

            if (Player.Playing)
            {
                Console.WriteLine("Queuing file...");
                Player.Enqueue(track);
            }
            else
            {
                Console.WriteLine("Playing file...");

                Player.Enqueue(track);

                Player.BeginPlay();
                //Task.Run(() => Player.BeginPlay());
            }

            Console.WriteLine("PlayFileAsync method end.");
        }

        public async Task DisconnectAsync()
        {
            if (timer != null) await timer.DisposeAsync();

            Player.Stop();
            Player.Dispose();
            await audioClient.StopAsync();
            audioClient.Dispose();
        }

        private void CheckPlaying(object _)
        {
            if (Player != null && !Player.Playing)
            {
                Player.Stop();
                Player.Dispose();
                audioClient.StopAsync();
                audioClient.Dispose();
            }
        }

        private async Task SendAudioInfoEmbedAsync(string title, Track track)
        {
            if (track != null)
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
                    await commandContext.Channel.SendFileAsync(stream: imageStream ?? null, filename: "cover.png", embed: embed.Build());
                else
                    await commandContext.Channel.SendMessageAsync(embed: embed.Build());
            }
            else
            {
                await commandContext.Channel.SendMessageAsync("Stopped.");
            }
        }

        private Task AudioClient_Disconnected(Exception arg)
        {
            audioClient.StopAsync();
            audioClient.Dispose();

            return Task.CompletedTask;
        }
    }
}
