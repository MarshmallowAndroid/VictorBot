using Discord.Commands;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBot.Modules
{
    [Name("Help")]
    [Summary("Provides help for commands.")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        public CommandService CommandService { get; set; }

        [Command("help")]
        [Summary("Shows the summary and usage of a command.")]
        public async Task GetHelpAsync([Summary("The command to get help with.")] string commandName = "")
        {
            if (commandName == string.Empty)
            {
                var commandList = new StringBuilder();

                commandList.Append("Available modules:\n\n");

                int commandIndex = 1;
                foreach (var modules in CommandService.Modules)
                {
                    commandList.Append($"**{commandIndex++}.** `{modules.Name}` - {modules.Summary}\n");
                }

                await ReplyAsync(commandList.ToString());
            }
            else
            {
                CommandInfo matchingCommand;
                if ((matchingCommand = CommandService.Commands.FirstOrDefault(x => x.Name == commandName)) != null)
                {
                    await ReplyAsync("too lazy to implement this, check again next time lol");
                }
            }
        }
    }
}
