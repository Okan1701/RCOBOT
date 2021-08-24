using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace RCOBOT.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient client;
        private readonly CommandService commands;
        private readonly IServiceProvider services;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(IDiscordClient client, CommandService commands, IServiceProvider services)
        {
            this.commands = commands;
            this.client = (DiscordSocketClient)client;
            this.services = services;
            commands.CommandExecuted += OnCommandExecuted;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            client.MessageReceived += HandleCommandAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them. Starting from Discord.NET 2.0, a
            // service provider is required to be passed into the
            // module registration method to inject the 
            // required dependencies.
            //
            // If you do not use Dependency Injection, pass null.
            // See Dependency Injection guide for more information.
            await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                services: services);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 4;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasStringPrefix("!rcon ", ref argPos) ||
                  message.HasMentionPrefix(client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.
            var res = await commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: services);
            
            //if (!res.IsSuccess) OnCommandError(res, context);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
        {
            if (!result.IsSuccess) 
            {
                var builder = new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "Failed to execute command",
                    Description = $"{context.Message.Content}\n{result.ErrorReason}"
                };
                await context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        private void OnCommandError(IResult result, ICommandContext context)
        {
            var builder = new EmbedBuilder
            {
                Color = Color.Red,
                Title = "Failed to execute command",
                Description = $"{context.Message.Content}\n{result.ErrorReason}"
            };
            context.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}