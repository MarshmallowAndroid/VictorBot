using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VictorBot.Services;

namespace VictorBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            _services = services;
            _client = services.GetRequiredService<DiscordSocketClient>();
            _commandService = services.GetRequiredService<CommandService>();

            _client.MessageReceived += MessageReceivedAsync;
            _commandService.CommandExecuted += CommandCompletedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage socketMessage)
        {
            var message = (SocketUserMessage)socketMessage;
            if (message == null) return;

            var context = new SocketCommandContext(_client, message);

            HandleMessage(message, context, _client);

            int argPos = 0;
            if (!message.HasCharPrefix('.', ref argPos)) return;
            if (message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;
            if (message.Author.IsBot) return;

            if (context.IsPrivate)
                LogHelper.Log($"User {message.Author.Username} invoked command: \"{message.Content}\" in DMs");
            else
                LogHelper.Log($"User {message.Author.Username} invoked command: \"{message.Content}\" in {context.Guild.Name} #{message.Channel.Name}");

            await _commandService.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandCompletedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified) return;
            if (result.IsSuccess) return;

            await context.Channel.SendMessageAsync("Error: " + result.ErrorReason);
        }

        public async void HandleMessage(
            SocketMessage message,
            SocketCommandContext context,
            IDiscordClient client)
        {
            var messageChannel = context.Channel;

            if (messageChannel.Name == "vb-safe-images")
            {
                if (message.Content.Contains("https://twitter.com/"))
                {
                    await context.Channel.SendMessageAsync("i can't get images from fucking twitter links");
                }
                else if (message.Attachments.Count > 0)
                {
                    var attachment = message.Attachments.First();

                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFileAsync(new Uri(attachment.Url), @"C:\Users\jacob\Desktop\ml_training\safe\" + attachment.Filename);
                    }
                }
            }
            else if (messageChannel.Name == "vb-lewd-images")
            {
                if (message.Content.Contains("https://twitter.com/"))
                {
                    await context.Channel.SendMessageAsync("i can't get images from fucking twitter links");
                }
                else if (message.Attachments.Count > 0)
                {
                    var attachment = message.Attachments.First();

                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFileAsync(new Uri(attachment.Url), @"C:\Users\jacob\Desktop\ml_training\lewd\" + attachment.Filename);
                    }
                }
            }

            var botMention = message.MentionedUsers.FirstOrDefault(x => x.Id == client.CurrentUser.Id);
            if (botMention == null) return;

            var content = message.Content;
            if (content.Contains("can you breathe"))
            {
                var audioService = _services.GetRequiredService<AudioService>();

                var voiceChannel = ((IGuildUser)context.User).VoiceChannel;
                if (voiceChannel == null)
                {
                    await context.Channel.SendMessageAsync("no i can't breathe");
                    return;
                }

                var channelId = voiceChannel.Id;
                var audioClients = audioService.AudioClients;

                IAudioClient audioClient;
                if (!audioClients.ContainsKey(channelId))
                {
                    audioClient = await voiceChannel.ConnectAsync();
                    audioClients.Add(channelId, audioClient);
                }
                else audioClient = audioClients[channelId];

                await audioService.PlayFileAsync(@"C:\Users\jacob\Desktop\breathe.mp3", context);

            }
            else if (content.Contains("kill yourself"))
            {
                await context.Channel.SendMessageAsync("Shutting down.");
                await client.StopAsync();
            }
        }

        public async Task<ulong> JoinVoiceChannelAsync(SocketCommandContext context, AudioService audioService)
        {
            var channel = ((IGuildUser)context.User).VoiceChannel;
            if (channel == null) return 0;

            var channelId = channel.Id;
            var audioClients = audioService.AudioClients;

            IAudioClient audioClient;
            if (!audioClients.ContainsKey(channelId))
            {
                audioClient = await channel.ConnectAsync();
                audioClients.Add(channelId, audioClient);
            }

            return channelId;
        }
    }
}