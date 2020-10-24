using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VictorBot;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using NhentaiApi;
using Discord;
using System.IO;

namespace VictorBot.Modules
{
    [Name("NHentai")]
    public class NhentaiModule : ModuleBase<SocketCommandContext>
    {
        public NhentaiClient NhClient { get; set; }

        [Command("nhentai")]
        [Alias("nh")]
        [Summary("Retrieves a gallery from the given query.")]
        public async Task GetNhGalleryAsync(string query = "", int page = -1)
        {
            if (!((ITextChannel)Context.Channel).IsNsfw)
            {
                await ReplyAsync("no.");
                return;
            }

            //var galleryId = Regex.Match(query, @"\d{1,}").Value;

            var validNumber = uint.TryParse(query, out uint galleryId);

            if (!validNumber)
            {
                if (query != string.Empty)
                {
                    await ReplyAsync(embed: NhClient.GetGalleryEmbed(await NhClient.GetRandomResultAsync(query), Context.User)); 
                }
                else
                {
                    await ReplyAsync("Retrieving random doujin...");
                    await ReplyAsync(embed: NhClient.GetGalleryEmbed(await NhClient.GetRandomGalleryAsync(), Context.User));
                }
            }
            else
            {
                if (page == 0)
                    await ReplyAsync(embed: NhClient.GetGalleryEmbed(await NhClient.GetGalleryAsync(galleryId.ToString()), Context.User));
                else
                    await ReplyAsync(
                        NhClient.GetImageUrl(
                            await NhClient.GetGalleryAsync(galleryId.ToString()), NhentaiClient.NhImageType.Page, page));
            }
        }

        private bool IsMixed(string inputString)
        {
            bool hasLetter = false;
            bool hasDigit = false;
            foreach (var character in inputString)
            {
                if (!hasLetter) hasLetter = char.IsLetter(character);
                if (!hasDigit) hasDigit = char.IsDigit(character);
            }
            return hasLetter & hasDigit;
        }

        //[Command("search")]
        //[Summary("NHentai search query.")]
        //public async Task SearchGalleriesAsync([Remainder] string query)
        //{
        //    var searchResult = await NhClient.GetRandomResultAsync(query);

        //    await ReplyAsync($"There are approximately {searchResult.Num_Pages * searchResult.Per_Page} available galleries that match.");
        //}
    }
}
