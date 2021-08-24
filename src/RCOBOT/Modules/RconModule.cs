using System;
using System.Threading.Tasks;
using Discord.Commands;
using RCOBOT.Attributes;
using RCOBOT.Services.Logger;
using RCOBOT.Services.RCON.Interfaces;

namespace RCOBOT.Modules
{
    public class RconModule : ModuleBase<ICommandContext>
    {
        private readonly IRconService rcon;
        private static readonly string prefix;

        public RconModule(IRconService rcon)
        {
            this.rcon = rcon;
        }

        [Command("exec", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [CommandChannel]
        public async Task ExecCommand([Remainder] string cmd)
        {
            var message = await rcon.ExecuteCommandAsync(cmd, Context);
            var embed = new Discord.EmbedBuilder
            {
                Title = cmd.Split(" ")[0],
                Description = message
            };

            await ReplyAsync("", false, embed.Build());
        }
    }
}