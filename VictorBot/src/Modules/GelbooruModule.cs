using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GelbooruApi;

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
            var tags = await GelbooruClient.GetLastTagAsync(Context);
            var posts = await GelbooruClient.GetPostsAsync(tags);
            var post = await GelbooruClient.GetPostAsync(posts);

            await ReplyAsync(embed: await GelbooruClient.GenerateEmbedAsync(post, posts.Count, Context.User));
        }

        [Command("tags")]
        public async Task GetImageFromTagAsync(string tags = "", int page = 0)
        {
            GelbooruClient.UpdateLastTagAsync(Context, tags);

            var posts = await GelbooruClient.GetPostsAsync(tags);
            var post = await GelbooruClient.GetPostAsync(posts);

            await ReplyAsync(embed: await GelbooruClient.GenerateEmbedAsync(post, posts.Count, Context.User));
        }

        [Command("lasttag")]
        [Alias("lt")]
        public async Task ReplyLastTagAsync()
        {
            var tags = await GelbooruClient.GetLastTagAsync(Context);

            await ReplyAsync(tags);
        }
    }
}
