using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using VictorBot.GelbooruApi;

namespace GelbooruApi
{
    public class GelbooruClient
    {
        private const string baseUrl = "https://gelbooru.com";

        HttpClient httpClient;

        public GelbooruClient()
        {
            httpClient = new HttpClient();
        }

        public async Task<Embed> GenerateEmbedAsync(GelbooruPost post, uint postCount, SocketUser user)
        {
            var postEmbed = new EmbedBuilder();

            if (post != null)
            {
                await Task.Run(() =>
                {
                    string formattedTags = "`" + post.tags.TrimStart().TrimEnd().Replace(" ", "` `") + "`";
                    string finalTags = "";

                    if (formattedTags.Length > 1024) finalTags = formattedTags.Substring(0, 1020) + "`...";
                    else finalTags = formattedTags;

                    postEmbed = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            IconUrl = user.GetAvatarUrl(),
                            Name = $"Requested by {user.Username}"
                        },
                        Title = $"{post.id}",
                        Url = $"https://gelbooru.com/index.php?page=post&s=view&id={post.id}",
                        Fields = new List<EmbedFieldBuilder>()
                        {
                            new EmbedFieldBuilder()
                            {
                                Name = "Tags",
                                Value = finalTags
                            }
                        },
                        ImageUrl = post.sample_url ?? post.preview_url,
                        Footer = new EmbedFooterBuilder()
                        {
                            Text = $"{postCount} results"
                        }
                    };
                });
            }
            else
            {
                postEmbed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = user.GetAvatarUrl(),
                        Name = $"Requested by {user.Username}"
                    },
                    Description = "No posts available."
                };
            }

            return postEmbed.Build();
        }

        public async Task<GelbooruPost> GetPostAsync(GelbooruPosts posts)
        {
            return await Task.Run(() =>
            {
                var random = new Random();

                if (posts.Count > 0)
                {
                    var randomIndex = random.Next(0, posts.Posts.Length);
                    return posts.Posts[randomIndex];
                }
                else return null;
            });
        }

        public async Task<GelbooruPosts> GetPostsAsync(string tags, int page = 0)
        {
            var serializer = new XmlSerializer(typeof(GelbooruPosts));
            var xmlString = await httpClient.GetStringAsync(baseUrl + $"/index.php?page=dapi&s=post&q=index&tags={tags}&pid={page}");
            var postList = (GelbooruPosts)serializer.Deserialize(new StringReader(xmlString));

            return postList;
        }

        public async Task<string> GetLastTagAsync(SocketCommandContext context)
        {
            return await Task.Run(() =>
            {
                var configDirectory = ".\\config";
                var configFile = configDirectory + "\\gelbooru_config.json";

                GelbooruConfig gelbooruConfig;

                if (!Directory.Exists(configDirectory)) return "";
                if (!File.Exists(configFile)) return "";

                using (var reader = new StreamReader(configFile))
                {
                    gelbooruConfig = JsonConvert.DeserializeObject<GelbooruConfig>(reader.ReadToEnd());
                }

                if (gelbooruConfig == null) return "";

                var users = gelbooruConfig.UserConfigs;
                var user = users?.FirstOrDefault(x => x.UserId == context.User.Id);
                var guilds = user?.GuildConfigs;
                var guild = guilds?.FirstOrDefault(x => x.GuildId == context.Guild.Id);
                var channels = guild?.ChannelConfigs;
                var channel = channels?.FirstOrDefault(x => x.ChannelId == context.Channel.Id);

                if (users == null) return "";
                if (user == null) return "";

                if (guilds == null) return "";
                if (guild == null) return "";

                if (channels == null) return "";
                if (channel == null) return "";

                return channel.LastGelbooruTags;
            });
        }

        public void UpdateLastTagAsync(SocketCommandContext context, string tags = "")
        {
            var configDirectory = ".\\config";
            var configFile = configDirectory + "\\gelbooru_config.json";

            GelbooruConfig gelbooruConfig;

            if (!Directory.Exists(configDirectory)) Directory.CreateDirectory(configDirectory);
            if (!File.Exists(configFile)) File.Create(configFile).Close();

            using (var reader = new StreamReader(configFile))
            {
                gelbooruConfig = JsonConvert.DeserializeObject<GelbooruConfig>(reader.ReadToEnd());
            }

            if (gelbooruConfig == null) gelbooruConfig = new GelbooruConfig();

            var users = gelbooruConfig.UserConfigs;
            var user = users?.FirstOrDefault(x => x.UserId == context.User.Id);
            var guilds = user?.GuildConfigs;
            var guild = guilds?.FirstOrDefault(x => x.GuildId == context.Guild.Id);
            var channels = guild?.ChannelConfigs;
            var channel = channels?.FirstOrDefault(x => x.ChannelId == context.Channel.Id);

            if (users == null)
            {
                users = new List<UserConfig>();
                gelbooruConfig.UserConfigs = users;
            }
            if (user == null)
            {
                user = new UserConfig() { UserId = context.User.Id };
                users.Add(user);
            }

            if (guilds == null)
            {
                guilds = new List<GuildConfig>();
                user.GuildConfigs = guilds;
            }
            if (guild == null)
            {
                guild = new GuildConfig() { GuildId = context.Guild.Id };
                user.GuildConfigs.Add(guild);
            }

            if (channels == null)
            {
                channels = new List<ChannelConfig>();
                guild.ChannelConfigs = channels;
            }
            if (channel == null)
            {
                channel = new ChannelConfig() { ChannelId = context.Channel.Id };
                guild.ChannelConfigs.Add(channel);
            }

            channel.LastGelbooruTags = tags;

            using (var writer = new StreamWriter(configFile))
            {
                writer.Write(JsonConvert.SerializeObject(gelbooruConfig));
            }
        }

        public async Task<string> GetJsonAsync(string url)
        {
            var jsonString = await httpClient.GetStringAsync(baseUrl + url);

            using (var writer = File.CreateText("test"))
            {
                writer.Write(jsonString);
            }

            return jsonString;
        }
    }
}
