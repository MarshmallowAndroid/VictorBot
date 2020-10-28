using Discord.Commands;
using System.Threading.Tasks;
using VictorBot.GelbooruApi;

namespace VictorBot.Modules
{
    [Name("Booru")]
    [Group("booru")]
    [Alias("br")]
    public class GelbooruModule : ModuleBase<SocketCommandContext>
    {
        public GelbooruClient GelbooruClient { get; set; }

        [Command]
        public async Task RepeatTag()
        {
            var tags = GelbooruClient.GetLastTag(Context);
            var posts = await GelbooruClient.GetPostsAsync(tags);
            var post = GelbooruClient.GetRandomPost(posts);

            await ReplyAsync(embed: GelbooruClient.GenerateEmbed(post, posts.Count, Context.User));
        }

        [Command("tags")]
        public async Task GetImageFromTagAsync(string tags = "", int page = 0)
        {
            var posts = await GelbooruClient.GetPostsAsync(tags);
            var post = GelbooruClient.GetRandomPost(posts);

            await ReplyAsync(embed: GelbooruClient.GenerateEmbed(post, posts.Count, Context.User));

            await GelbooruClient.UpdateLastTagAsync(Context, tags);
        }

        [Command("lasttag")]
        [Alias("lt")]
        public async Task ReplyLastTagAsync() => await ReplyAsync(GelbooruClient.GetLastTag(Context));
    }
}
