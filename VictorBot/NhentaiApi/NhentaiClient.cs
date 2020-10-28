using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VictorBot.NhentaiApi
{
    public class NhentaiClient
    {
        private const string nhBase = "https://nhentai.net";
        private const string thumbnailCdn = "https://t.nhentai.net";
        private const string imageCdn = "https://i.nhentai.net";

        private readonly HttpClient _httpClient;

        public NhentaiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<NhentaiGallery> GetGalleryAsync(string galleryId)
        {
            var jsonString = "";

            try
            {
                jsonString = await _httpClient.GetStringAsync(nhBase + $"/api/gallery/{galleryId}");
            }
            catch (HttpRequestException) { }

            return JsonConvert.DeserializeObject<NhentaiGallery>(jsonString);
        }

        public async Task<NhentaiGallery> GetRandomGalleryAsync()
        {
            var gallery = new NhentaiGallery();

            var htmlString = "";

            try
            {
                htmlString = await _httpClient.GetStringAsync("https://nhentai.net/random");
            }
            catch (HttpRequestException) { }

            var beginIndex = htmlString.LastIndexOf("JSON.parse(\"");
            var pass1 = htmlString.Substring(beginIndex, htmlString.Length - beginIndex);
            var endIndex = pass1.LastIndexOf("\");");
            var pass2 = pass1.Substring(0, endIndex);

            var escaped = Regex.Unescape(pass2);
            var jsonString = escaped.Substring(12, escaped.Length - 12);

            gallery = JsonConvert.DeserializeObject<NhentaiGallery>(jsonString);

            return gallery;
        }

        public async Task<NhentaiGallery> GetRandomResultAsync(string query)
        {
            NhentaiSearchResult result = new NhentaiSearchResult();
            int randomIndex = 0;

            try
            {
                var random = new Random();

                var jsonString = await _httpClient.GetStringAsync(nhBase + $"/api/galleries/search?query={query}");
                result = JsonConvert.DeserializeObject<NhentaiSearchResult>(jsonString);

                int randomPage = 0;
                if (result.Num_Pages > 0)
                {
                    randomPage = random.Next(1, result.Num_Pages);

                    jsonString = await _httpClient.GetStringAsync(nhBase + $"/api/galleries/search?query={query}&page={randomPage}");
                    result = JsonConvert.DeserializeObject<NhentaiSearchResult>(jsonString);

                    randomIndex = random.Next(0, result.Result.Length - 1);
                }
                else return null;
            }
            catch (WebException) { }

            return result.Result[randomIndex];
        }

        public string GetImageUrl(NhentaiGallery gallery, NhImageType imageType, int page = 1)
        {
            if (imageType == NhImageType.Cover)
                return $"{thumbnailCdn}/galleries/{gallery.Media_Id}/cover.{gallery.Images.Cover.ImageFormat}";
            else
                return $"{imageCdn}/galleries/{gallery.Media_Id}/{page}.{gallery.Images.Pages[page - 1].ImageFormat}";
        }

        public Embed GetGalleryEmbed(NhentaiGallery gallery, SocketUser user)
        {
            EmbedBuilder galleryEmbedBuilder;

            if (gallery != null && gallery.Id > 0)
            {
                var languagesStringBuilder = new StringBuilder();
                var parodiesStringBuilder = new StringBuilder();
                var charactersStringBuilder = new StringBuilder();
                var tagsStringBuilder = new StringBuilder();
                var artistsStringBuilder = new StringBuilder();
                var groupsStringBuilder = new StringBuilder();

                foreach (var tag in gallery.Tags)
                {
                    if (tag.TagType == TagType.Language) languagesStringBuilder.Append($"`{tag.Name}` ");
                    else if (tag.TagType == TagType.Parody) parodiesStringBuilder.Append($"`{tag.Name}` ");
                    else if (tag.TagType == TagType.Character) charactersStringBuilder.Append($"`{tag.Name}` ");
                    else if (tag.TagType == TagType.Tag) tagsStringBuilder.Append($"`{tag.Name}` ");
                    else if (tag.TagType == TagType.Artist) artistsStringBuilder.Append($"`{tag.Name}` ");
                    else if (tag.TagType == TagType.Group) groupsStringBuilder.Append($"`{tag.Name}` ");
                }

                var fields = new List<EmbedFieldBuilder>();

                if (languagesStringBuilder.Length > 0)
                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Languages",
                        Value = languagesStringBuilder.ToString()
                    });
                if (parodiesStringBuilder.Length > 0)
                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Parodies",
                        Value = parodiesStringBuilder.ToString()
                    });
                if (charactersStringBuilder.Length > 0)
                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Characters",
                        Value = charactersStringBuilder.ToString()
                    });
                if (tagsStringBuilder.Length > 0)
                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Tags",
                        Value = tagsStringBuilder.ToString()
                    });
                if (artistsStringBuilder.Length > 0)
                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Artists",
                        Value = artistsStringBuilder.ToString()
                    });
                if (groupsStringBuilder.Length > 0)
                    fields.Add(new EmbedFieldBuilder()
                    {
                        Name = "Groups",
                        Value = groupsStringBuilder.ToString()
                    });

                galleryEmbedBuilder = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = user.GetAvatarUrl(),
                        Name = $"Requested by {user.Username}"
                    },
                    Title = $"[{gallery.Id}] {gallery.Title.Pretty}",
                    Url = $"http://nhentai.net/g/{gallery.Id}",
                    Fields = fields,
                    ImageUrl = GetImageUrl(gallery, NhImageType.Cover),
                    Footer = new EmbedFooterBuilder() { Text = $"{gallery.Num_Pages} pages" }
                };
            }
            else
            {
                galleryEmbedBuilder = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = user.GetAvatarUrl(),
                        Name = $"Requested by {user.Username}"
                    },

                    Description = "Gallery doesn't exist.",
                };
            }

            return galleryEmbedBuilder.Build();
        }

        public enum NhImageType
        {
            Cover,
            Page
        }
    }
}
