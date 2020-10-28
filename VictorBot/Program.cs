using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using VictorBot.GelbooruApi;
using VictorBot.NhentaiApi;
using VictorBot.Services.Audio;

namespace VictorBot
{
    class Program
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
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig() { LogLevel = LogSeverity.Debug }))
                .AddSingleton(new CommandService(
                    new CommandServiceConfig()
                    {
                        LogLevel = LogSeverity.Debug,
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

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();
    }
}
