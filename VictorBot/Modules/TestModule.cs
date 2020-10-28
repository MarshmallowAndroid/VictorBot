using Discord.Commands;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VictorBot.Modules
{
    public class TestModule : ModuleBase<SocketCommandContext>
    {
        [Command("pyramid")]
        [Summary("Generates a pyramid. Input number specifies height. Default is 8.")]
        public Task PyramidAsync(int max = 8)
        {
            var resultBuilder = new StringBuilder();

            resultBuilder.Append("```\n");

            for (int i = 1; i <= max; i++)
            {
                for (int j = 1; j <= max - i; j++)
                    resultBuilder.Append(" ");

                for (int j = 1; j <= (i * 2) - 1; j++)
                {
                    if (j == (i * 2) - 1 || j == 1)
                        resultBuilder.Append("*");
                    else if (i == max)
                        resultBuilder.Append("*");
                    else
                        resultBuilder.Append(" ");
                }

                resultBuilder.Append("\n");
            }

            resultBuilder.Append("```");

            return ReplyAsync(resultBuilder.ToString());
        }

        [Command("faketask")]
        [Summary("Fake a task for a specified time.")]
        public Task LongTask(int milliseconds)
        {
            ReplyAsync("Task will complete after " + milliseconds / 1000 + " seconds.");
            Thread.Sleep(milliseconds);
            return ReplyAsync("Task completed.");
        }

        [Command("shuffle")]
        public Task ShuffleList([Remainder] string items)
        {
            StringBuilder finalList = new StringBuilder();

            var itemList = items.Split(' ');

            Random random = new Random();
            var shuffled = itemList.OrderBy(x => random.Next()).ToArray();

            foreach (var item in shuffled)
            {
                finalList.AppendLine(item);
            }

            return ReplyAsync(finalList.ToString());
        }
    }
}
