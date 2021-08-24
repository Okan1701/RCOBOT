using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RCOBOT.Attributes
{
    public class CommandChannelAttribute : PreconditionAttribute
    {
        private CommandChannelOptions options;
        
        public CommandChannelAttribute()
        {
            options = null;
        }
        
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // Get appsettings options
            options ??= services.GetRequiredService<IConfiguration>().GetSection("CommandChannel")
                .Get<CommandChannelOptions>();
            
            // Verify the command is used in the proper channel
            if (context.Channel.Id != options.ChannelId) return PreconditionResult.FromError("Wrong Channel");

            // Verify calling user has the correct role
            var requiredRoles = options.Permissions.Where(x => x.Key == command.Name).Select(x => x.Value).FirstOrDefault();
            var user = (IGuildUser)context.User;
            if (!user.RoleIds.Where(x => requiredRoles.Contains(x)).Any()) 
            {
                return PreconditionResult.FromError("Insufficent Permissions");
            }


            return PreconditionResult.FromSuccess();
        }
    }
}