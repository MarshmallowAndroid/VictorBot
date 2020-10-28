using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace VictorBot.GelbooruApi
{
    /// <summary>
    /// Client for interacting with the Gelbooru API
    /// </summary>
    public class GelbooruClient
    {
        private const string baseUrl = "https://gelbooru.com";

        private readonly HttpClient _httpClient;

        public GelbooruClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Embed GenerateEmbed(GelbooruPost post, uint postCount, SocketUser user)
        {
            EmbedBuilder postEmbed;

            if (post != null)
            {
                string formattedTags = "`" + post.tags.TrimStart().TrimEnd().Replace(" ", "` `") + "`";
                string finalTags;
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

        public GelbooruPost GetRandomPost(GelbooruPosts posts)
        {
            var random = new Random();

            if (posts.Count > 0)
            {
                var randomIndex = random.Next(0, posts.Posts.Length);
                return posts.Posts[randomIndex];
            }
            else return null;
        }

        public async Task<GelbooruPosts> GetPostsAsync(string tags, int page = 0)
        {
            var serializer = new XmlSerializer(typeof(GelbooruPosts));
            var xmlString = await _httpClient.GetStringAsync(baseUrl + $"/index.php?page=dapi&s=post&q=index&tags={tags}&pid={page}");
            var postList = (GelbooruPosts)serializer.Deserialize(new StringReader(xmlString));

            return postList;
        }

        public string GetLastTag(SocketCommandContext context)
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

            var userId = context.User.Id;
            var messageChannelId = context.Channel.Id;

            var users = gelbooruConfig.UserConfigs;

            if (users.ContainsKey(context.User.Id))
            {
                var userTags = users[userId].MessageChannelTags;

                if (userTags.ContainsKey(messageChannelId))
                    return userTags[messageChannelId];
            }

            return "";
        }

        public Task UpdateLastTagAsync(SocketCommandContext context, string tags = "")
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

            if (gelbooruConfig == null)
            {
                gelbooruConfig = new GelbooruConfig
                {
                    UserConfigs = new Dictionary<ulong, UserConfig>()
                };
            }

            var userId = context.User.Id;
            var messageChannelId = context.Channel.Id;

            var users = gelbooruConfig.UserConfigs;
            if (!users.ContainsKey(userId))
            {
                users.Add(userId, new UserConfig() { MessageChannelTags = new Dictionary<ulong, string>() });
            }

            var userTags = users[userId].MessageChannelTags;

            if (userTags.ContainsKey(messageChannelId))
                userTags[messageChannelId] = tags;
            else
            {
                userTags.Add(messageChannelId, tags);
            }

            using (var writer = new StreamWriter(configFile))
            {
                writer.Write(JsonConvert.SerializeObject(gelbooruConfig));
            }

            return Task.CompletedTask;
        }

        public async Task<string> GetJsonAsync(string url)
        {
            var jsonString = await _httpClient.GetStringAsync(baseUrl + url);

            using (var writer = File.CreateText("test"))
            {
                writer.Write(jsonString);
            }

            return jsonString;
        }
    }
}
