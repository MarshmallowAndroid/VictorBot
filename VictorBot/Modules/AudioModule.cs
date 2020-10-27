using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using VictorBot.Services;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using VictorBot.Services.Audio;
using System.IO;

namespace VictorBot.Modules
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        public AudioService AudioService { get; set; }

        [Command("join")]
        public async Task JoinVoiceChannelAsync()
        {
            await AudioService.JoinVoiceChannelAsync(Context);
        }

        [Command("localplay")]
        [Alias("lp", "lplay")]
        public async Task PlayAsync(string path)
        {
            await AudioService.PlayFileAsync(path, Context);
        }

        [Command("quickplay")]
        [Alias("qp")]
        public async Task QuickPlay(string fileName)
        {
            string musicDirectory = @"C:\Users\jacob\Desktop\BotTestMusic\";

            string[] files = Directory.GetFiles(musicDirectory, fileName + ".*");

            if (files.Length > 0) await PlayAsync(files[0]);
            else await ReplyAsync("No match.");
        }

        [Command("ytplay")]
        [Alias("yt")]
        public async Task YouTubePlay(string url)
        {
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
            var video = await youtube.Videos.GetAsync(url);
            var audioStreamInfo = streamManifest.GetAudioOnly().WithHighestBitrate();

            if (audioStreamInfo != null)
            {
                await AudioService.PlayYouTubeAsync(video, audioStreamInfo, Context);
            }
        }

        [Command("loop")]
        public async Task LoopAsync()
        {
            await AudioService.LoopAsync(Context);
        }

        [Command("playpause")]
        [Alias("pp", "toggleplay", "tp")]
        public async Task PauseAsync()
        {
            await AudioService.PlayPauseAsync(Context);
        }

        [Command("stop")]
        public async Task StopAsync()
        {
            await AudioService.StopAsync(Context);
        }

        [Command("skip")]
        public async Task SkipAsync()
        {
            await AudioService.SkipAsync(Context);
        }

        [Command("earrape")]
        [Alias("er")]
        public async Task EarRapeAsync()
        {
            await AudioService.EarRapeAsync(Context);
        }

        [Command("setearrapeamount")]
        [Alias("sera")]
        public async Task SetEarRapeAmountAsync([Remainder] string paramsString)
        {
            await AudioService.SetEarRapeAmountAsync(Context, paramsString);
        }

        [Command("seek")]
        public async Task SeekAsync([Remainder] string position)
        {
            await AudioService.SeekAsync(Context, position);
        }

        [Command("disconnect")]
        [Alias("dc")]
        public async Task DisconnectAsync()
        {
            await ((IGuildUser)Context.User).VoiceChannel.DisconnectAsync();
        }
    }
}
