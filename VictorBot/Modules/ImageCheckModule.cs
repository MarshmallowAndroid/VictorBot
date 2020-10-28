using Discord.Commands;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VictorBotML.Model;

namespace VictorBot.Modules
{
    public class ImageCheckModule : ModuleBase<SocketCommandContext>
    {
        [Command("mlcheck")]
        public async Task CheckImageAsync(string url = "")
        {
            var message = Context.Message;

            string imageUrl;
            if (string.IsNullOrEmpty(url))
            {
                if (message.Attachments.Count > 0) { imageUrl = message.Attachments?.FirstOrDefault().Url; }
                else imageUrl = "";
            }
            else imageUrl = url;

            if (!string.IsNullOrEmpty(imageUrl))
            {
                using (var webClient = new WebClient())
                {
                    try
                    {
                        webClient.DownloadFile(imageUrl, "ml_image");

                        var input = new ModelInput()
                        {
                            ImageSource = "ml_image"
                        };

                        var result = ConsumeModel.Predict(input);
                        await ReplyAsync($"Result: **{result.Prediction}** ({result.Score[0] * 100}% lewd, {result.Score[1] * 100}% safe)");
                    }
                    catch (Exception)
                    {
                        await ReplyAsync("Couldn't process image.");
                    }
                }
            }
        }
    }
}
