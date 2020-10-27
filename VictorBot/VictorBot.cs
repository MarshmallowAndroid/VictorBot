using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GelbooruApi;
using Microsoft.Extensions.DependencyInjection;
using NhentaiApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VictorBot.Services;

namespace VictorBot
{
    class VictorBot
    {
        public async Task MainAsync()
        {
            using (ServiceProvider serviceProvider = ConfigureServiceProviders())
            {
                DiscordSocketClient client = serviceProvider.GetRequiredService<DiscordSocketClient>();
                client.Log += LogAsync;

                serviceProvider.GetRequiredService<CommandService>().Log += LogAsync;

                await client.LoginAsync(TokenType.Bot, await new StreamReader("..\\..\\..\\token.txt").ReadToEndAsync());
                await client.StartAsync();

                await serviceProvider.GetRequiredService<CommandHandler>().InitializeAsync();

                await Task.Delay(-1);
            }
        }

        private ServiceProvider ConfigureServiceProviders()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton(new CommandService(
                    new CommandServiceConfig()
                    {
                        DefaultRunMode = RunMode.Async,
                        ThrowOnError = true
                    }))
                .AddSingleton<CommandHandler>()
                .AddSingleton<HttpClient>()
                .AddSingleton<NhentaiClient>()
                .AddSingleton<GelbooruClient>()
                .AddSingleton<AudioService>()
                .BuildServiceProvider();
        }

        private Task LogAsync(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        static void Main(string[] args) => new VictorBot().MainAsync().GetAwaiter().GetResult();
    }
}
