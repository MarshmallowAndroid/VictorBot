using Discord;
using Discord.Commands;
using NAudio.Vorbis;
using NAudio.Wave;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using TLFile = TagLib.File;

namespace VictorBot.Services.Audio
{
    public partial class AudioService
    {
        private readonly List<GuildContext> guilds;

        public AudioService()
        {
            guilds = new List<GuildContext>();
        }

        public static Track CreateTrackFromFile(string path)
        {
            var taglib = TLFile.Create(path);

            string title = taglib.Tag.Title ?? "Unknown Title";
            string artist = taglib.Tag.Performers.FirstOrDefault() ?? "Unknown Artist";
            string album = taglib.Tag.Album ?? "Unknown Album";
            byte[] image = null;

            if (taglib.Tag.Pictures.Length > 0) image = taglib.Tag.Pictures.First().Data.ToArray();

            int start = 0;
            int end = 0;

            if (Path.GetExtension(path) == ".ogg")
            {
                var comments = taglib.GetTag(TagLib.TagTypes.Xiph) as TagLib.Ogg.XiphComment;

                start = int.Parse(comments.GetFirstField("LOOPSTART"));
                end = int.Parse(comments.GetFirstField("LOOPLENGTH")) + start;

                return new Track(
                    new LoopStream(new VorbisWaveReader(path),
                        start, end),
                    title,
                    artist,
                    album,
                    image);
            }
            else
            {
                string loopFile = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + ".txt";
                if (File.Exists(loopFile))
                {
                    using var file = File.OpenText(loopFile);
                    start = int.Parse(file.ReadLine());
                    end = int.Parse(file.ReadLine());
                }

                return new Track(
                    new LoopStream(new MediaFoundationReader(path),
                        start, end),
                    title,
                    artist,
                    album,
                    image);
            }
        }

        public static Track CreateTrackFromYouTubeStream(Video video, IStreamInfo streamInfo) =>
            new(new LoopStream(
                new MediaFoundationReader(streamInfo.Url)),
                video.Title,
                video.Author.Title,
                "YouTube");

        private GuildContext GetGuildContext(ICommandContext commandContext)
        {
            var guild = guilds.Where(x => x.Guild == commandContext.Guild).FirstOrDefault();

            if (guild == null)
            {
                var newGuild = new GuildContext(commandContext);
                guilds.Add(newGuild);

                return newGuild;
            }
            else return guild;
        }

        public async Task JoinVoiceChannelAsync(ICommandContext context)
        {
            var guild = GetGuildContext(context);
            await guild.JoinVoiceChannelAsync();
        }

        public async Task SearchAndPlayFileAsync(string query, ICommandContext context)
        {
            var guild = GetGuildContext(context);
            var userState = guild.UserStates.Where(u => u.User == context.User).FirstOrDefault();

            if (userState == null)
            {
                userState = new UserState(context.User);
                guild.UserStates.Add(userState);
            }

            if (!int.TryParse(query, out int chosen))
            {
                var results = LocalSearch(query);
                int resultsLength = results.Length;

                if (resultsLength > 0)
                {
                    var list = new List<EmbedFieldBuilder>();
                    for (int i = 0; i < resultsLength; i++)
                    {
                        var result = results[i];

                        list.Add(new EmbedFieldBuilder()
                        {
                            Name = $"{i + 1}. " + result.Title + " - " + result.Artist,
                            Value = result.Album
                        });
                    }

                    var embed = new EmbedBuilder()
                    {
                        Title = "Choose a result",
                        Fields = list
                    };

                    userState.Results = results;
                    userState.IsChoosing = true;

                    await context.Channel.SendMessageAsync(embed: embed.Build());
                }
                else
                {
                    var embed = new EmbedBuilder()
                    {
                        Title = "Error",
                        Description = "No results found."
                    };

                    await context.Channel.SendMessageAsync(embed: embed.Build());
                }
            }
            else
            {
                var results = userState.Results;

                if (userState.IsChoosing)
                {
                    if (chosen > 0 && chosen <= results.Length)
                    {
                        userState.IsChoosing = false;
                        await PlayFileAsync(results[chosen - 1].Path, context);
                    }
                    else
                        await context.Channel.SendMessageAsync($"Invalid input.");
                }
                //else await context.Channel.SendMessageAsync($"User {context.User.Username} is not selecting.");
            }
        }

        public async Task PlayFileAsync(string path, ICommandContext context)
        {
            var guild = GetGuildContext(context);
            var track = CreateTrackFromFile(path);
            await guild.PlayTrackAsync(track);
        }

        public async Task PlayYouTubeAsync(Video video, IStreamInfo streamInfo, ICommandContext context)
        {
            var guild = GetGuildContext(context);
            var track = CreateTrackFromYouTubeStream(video, streamInfo);
            await guild.PlayTrackAsync(track);
        }

        public async Task DisconnectAsync(ICommandContext context)
        {
            var guild = GetGuildContext(context);
            await guild.DisconnectAsync();
            guilds.Remove(guild);
        }

        static TrackFile[] LocalSearch(string query)
        {
            var songs = new List<TrackFile>();
            using var fileStream = File.OpenRead("cache");
            using var decompressor = new DeflateStream(fileStream, CompressionMode.Decompress, true);

            using var headerReader = new BinaryReader(fileStream);
            using var dataReader = new BinaryReader(decompressor);

            headerReader.ReadChars(4);

            int length = headerReader.ReadInt32();

            for (int i = 0; i < length; i++)
            {
                songs.Add(new TrackFile(dataReader.ReadString())
                {
                    Title = dataReader.ReadString(),
                    Album = dataReader.ReadString(),
                    Artist = dataReader.ReadString()
                });
            }

            var results = new List<TrackFile>();

            foreach (var song in songs)
            {
                int match = 0;

                string[] queryWords = query.Split(' ');
                string[] songWords = song.Title.Split(' ');

                foreach (var queryWord in queryWords)
                {
                    foreach (var songWord in songWords)
                    {
                        if (songWord.ToLower() == queryWord.ToLower()) match++;
                    }
                }

                if (queryWords.Length > 1)
                {
                    if (match > 1) results.Add(song);
                }
                else if (match > 0) results.Add(song);
            }

            if (results.Count == 0)
            {
                foreach (var song in songs)
                {
                    int match = 0;

                    string[] queryWords = query.Split(' ');

                    foreach (var queryWord in queryWords)
                    {
                        if (song.Title.ToLower().Contains(queryWord.ToLower())) match++;
                    }

                    if (queryWords.Length > 1)
                    {
                        if (match > 1) results.Add(song);
                    }
                    else if (match > 0) results.Add(song);
                }
            }

            return results.ToArray();
        }
    }
}
