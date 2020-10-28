using Discord;
using Discord.Commands;
using System.Threading.Tasks;

namespace VictorBot.Services.Audio
{
    public partial class AudioService
    {
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

        public Task SetEarRapeAmountAsync(SocketCommandContext context, string paramsString)
        {
            if (IsInVoiceChannel(context))
            {
                var voiceChannelId = ((IGuildUser)context.User).VoiceChannel.Id;
                Players[voiceChannelId].SetEarRapeAmount(paramsString);
            }
            return Task.CompletedTask;
        }

        public Task SeekAsync(SocketCommandContext context, string position)
        {
            if (IsInVoiceChannel(context))
            {
                var voiceChannelId = ((IGuildUser)context.User).VoiceChannel.Id;
                Players[voiceChannelId].Seek(long.Parse(position));
            }
            return Task.CompletedTask;
        }
    }
}
