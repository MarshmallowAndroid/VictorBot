using Discord.Commands;
using System.Threading.Tasks;

namespace VictorBot.Services.Audio
{
    public partial class AudioService
    {
        public Task PlayPauseAsync(ICommandContext context)
        {
            var guild = GetGuildContext(context);
            if (guild.IsUserInVoiceChannel()) guild.Player.Pause();

            return Task.CompletedTask;
        }

        public Task StopAsync(ICommandContext context)
        {
            var guild = GetGuildContext(context);
            if (guild.IsUserInVoiceChannel()) guild.Player.Stop();

            return Task.CompletedTask;
        }

        public Task LoopAsync(ICommandContext context)
        {
            var guild = GetGuildContext(context);
            if (guild.IsUserInVoiceChannel()) guild.Player.Pause();

            return Task.CompletedTask;
        }

        public Task SkipAsync(ICommandContext context)
        {
            var guild = GetGuildContext(context);
            if (guild.IsUserInVoiceChannel()) guild.Player.Skip();

            return Task.CompletedTask;
        }

        public Task EarRapeAsync(ICommandContext context)
        {
            var guild = GetGuildContext(context);
            if (guild.IsUserInVoiceChannel()) guild.Player.EarRape();

            return Task.CompletedTask;
        }

        public Task SetEarRapeAmountAsync(ICommandContext context, string paramsString)
        {
            var guild = GetGuildContext(context);
            if (guild.IsUserInVoiceChannel()) guild.Player.SetEarRapeAmount(paramsString);

            return Task.CompletedTask;
        }

        public Task SeekAsync(ICommandContext context, string position)
        {
            var guild = GetGuildContext(context);
            if (guild.IsUserInVoiceChannel()) guild.Player.Seek(long.Parse(position));

            return Task.CompletedTask;
        }
    }
}
