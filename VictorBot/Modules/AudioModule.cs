using Discord.Commands;
using System.Threading.Tasks;
using VictorBot.Services.Audio;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace VictorBot.Modules
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        public AudioService AudioService { get; set; }

        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinVoiceChannelAsync()
        {
            await AudioService.JoinVoiceChannelAsync(Context);
        }

        #region Legacy functions
        [Command("directplay", RunMode = RunMode.Async)]
        [Alias("dp", "dplay")]
        public async Task DirectPlayAsync(string path)
        {
            await AudioService.PlayFileAsync(path, Context);
        }

        //[Command("quickplay", RunMode = RunMode.Async)]
        //[Alias("qp")]
        //public async Task QuickPlayAsync(string fileName)
        //{
        //    string musicDirectory = @"C:\Users\jacob\Desktop\TestMusic\";

        //    string[] files = Directory.GetFiles(musicDirectory, fileName + ".*");

        //    if (files.Length > 0) await PlayAsync(files[0]);
        //    else await ReplyAsync("No match.");
        //}

        //[Command("imasplay", RunMode = RunMode.Async)]
        //[Alias("ip")]
        //public async Task ImasPlayAsync(string fileName)
        //{
        //    string musicDirectory = @"C:\Users\jacob\Desktop\TestMusic\imasgamebgm";

        //    string[] files = Directory.GetFiles(musicDirectory, fileName + ".*");

        //    if (files.Length > 0) await PlayAsync(files[0]);
        //    else await ReplyAsync("No match.");
        //}
        #endregion

        [Command("localplay", RunMode = RunMode.Async)]
        [Alias("lp", "lplay")]
        public async Task LocalPlayAsync([Remainder] string query)
        {
            await AudioService.SearchAndPlayFileAsync(query, Context);
        }

        [Command("ytplay", RunMode = RunMode.Async)]
        [Alias("yt")]
        public async Task YouTubePlayAsync(string url)
        {
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
            var video = await youtube.Videos.GetAsync(url);
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

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
            await AudioService.DisconnectAsync(Context);
        }
    }
}
