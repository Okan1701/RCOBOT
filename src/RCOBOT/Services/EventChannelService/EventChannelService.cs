using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RCOBOT.Services.Logger;
using RCOBOT.Services.RCON;
using RCOBOT.Services.RCON.Interfaces;

namespace RCOBOT.Services.EventChannelService
{
    /// <summary>
    /// Main implementation of IEventChannelService.
    /// This service is responsible for sending the messages to the various event channels
    /// Channel IDs are fetched from appsettings
    /// </summary>
    public class EventChannelService : IEventChannelService
    {
        // Public getter for the service options
        public IOptions<EventChannelOptions> Options { get; }

        // Services
        private readonly IDiscordClient discordClient;
        private readonly ILogger logger;

        // Cache variables for all channels
        private IMessageChannel systemLogChannel;
        private IMessageChannel playerJoinChannel;
        private IMessageChannel playerDisconnectChannel;
        private IMessageChannel chatMsgChannel;

        public EventChannelService(IRconMessageHandler rconMessageHandler, ILogger logger, IOptions<EventChannelOptions> options,
            IDiscordClient discordClient)
        {
            this.discordClient = discordClient;
            this.logger = logger;
            Options = options;

            // Setup events
            rconMessageHandler.PlayerConnected += (o, args) => OnPlayerConnected(o, args);
            rconMessageHandler.PlayerDisconnected += (o, args) => OnPlayerDisconnected(o, args);
            rconMessageHandler.ChatMessageReceived += (o, args) => OnChatMessageReceived(o, args);
            logger.OnLogged += (o, args) => OnLogged(o, args);
        }

        /// <summary>
        /// Listens for any logged item and sends a copy of it to the system log channel on Discord
        /// </summary>
        /// <param name="sender">Object that triggered the event</param>
        /// <param name="args">The Log Event object containing the logged data</param>
        /// <returns></returns>
        private async Task OnLogged(object sender, LogEventArgs args)
        {
            if (discordClient.ConnectionState != ConnectionState.Connected) return;

            // Fetch the channel if cache variable is null
            systemLogChannel ??=
                (IMessageChannel) await discordClient.GetChannelAsync(Options.Value.SystemLogChannelId);
            if (systemLogChannel == null)
            { // Channel doesn't exist
                logger.LogError(
                    $"System Log channel with ID {Options.Value.SystemLogChannelId} not found! Did you specify the correct ID in appsettings.json?");
                return;
            }

            await systemLogChannel.SendMessageAsync($"```[{args.DateTime}] || {args.Prefix} || {args.Message}```");
        }

        /// <summary>
        /// Sends chat messages to the correct discord channel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task OnChatMessageReceived(object sender, ChatMessageEventArgs args)
        {
            chatMsgChannel ??= (IMessageChannel) await discordClient.GetChannelAsync(Options.Value.ChatMessagesChannelId);
            if (chatMsgChannel == null)
            { // Channel doesn't exist
                logger.LogError(
                    $"Chat Message event channel with ID {Options.Value.ChatMessagesChannelId} not found! Did you specify the correct ID in appsettings.json?");
                return;
            }

            var embed = new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    Name = $"{args.ChannelType} {args.Name}",
                },
                Title = args.Content,
                Timestamp = DateTimeOffset.Now,
                Color = GetChannelTypeColor(args.ChannelType)
            };
            await chatMsgChannel.SendMessageAsync("", false, embed.Build());
            // await chatMsgChannel.SendMessageAsync($"```{GetColorPrefix(args.ChannelType)}[{DateTime.Now}] {args.ChannelType} {args.Name}: {args.Content}```");
        }

        /// <summary>
        /// Sends player disconnect event messages to discord
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task OnPlayerDisconnected(object sender, PlayerDisconnectEventArgs args)
        {
            playerDisconnectChannel ??= (IMessageChannel) await discordClient.GetChannelAsync(Options.Value.PlayerDisconnectsChannelId);
            if (playerDisconnectChannel == null)
            { // Channel doesn't exist
                logger.LogError(
                    $"Player Disconnect event channel with ID {Options.Value.PlayerDisconnectsChannelId} not found! Did you specify the correct ID in appsettings.json?");
                return;
            }
            
            var embed = new EmbedBuilder
            {
                Title = $":outbox_tray: {args.Name} has disconnected from the server!",
                Description = $"{DateTime.Now} | ID: {args.GameId}",
                Color = Color.Red
            };
            await playerDisconnectChannel.SendMessageAsync("", false, embed.Build());
            //await playerDisconnectChannel.SendMessageAsync($"```[{DateTime.Now}] Player {args.Name} has disconnected from the server (ID={args.GameId})````");
        }

        /// <summary>
        /// Sends Player join event messages to discord
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task OnPlayerConnected(object sender, PlayerConnectedEventArgs args)
        {
            playerJoinChannel ??= (IMessageChannel) await discordClient.GetChannelAsync(Options.Value.PlayerJoinsChannelId);
            if (playerJoinChannel == null)
            { // Channel doesn't exist
                logger.LogError(
                    $"Player Join event channel with ID {Options.Value.PlayerJoinsChannelId} not found! Did you specify the correct ID in appsettings.json?");
                return;
            }

            var embed = new EmbedBuilder
            {
                Title = $":inbox_tray: {args.Name} has connected to the server!",
                Description = $"{DateTime.Now} | ID: {args.GameId}",
                Color = Color.Green
            };
            await playerJoinChannel.SendMessageAsync("", false, embed.Build());
            //await playerJoinChannel.SendMessageAsync($"```[{DateTime.Now}] Player {args.Name} has connected to the server (ID={args.GameId})```");
        }

        /// <summary>
        /// Gets the correct code block string prefix for discord messages for chat events
        /// </summary>
        /// <param name="channelType">The channel type</param>
        /// <returns>Color prefix</returns>
        private string GetColorPrefix(string channelType)
        {
            Color color = GetChannelTypeColor(channelType);
            
            if (color == Color.Blue) return "ini\n";
            if (color == Color.Orange) return "http\n";
            if (color == Color.Green) return "css\n";
            
            // Default value
            return "";
        }
        
        /// <summary>
        /// Returns the correct Color enum based on the channel type
        /// </summary>
        /// <param name="channelType">The channel type</param>
        /// <returns>Color enum representing the cha type</returns>
        private Color GetChannelTypeColor(string channelType)
        {
            switch (channelType)
            {
                case "(Global)":
                    return Color.DarkerGrey;
                case "(Side)":
                    return Color.Blue;
                case "(Vehicle)":
                    return Color.Orange;
                case "(Group)":
                    return Color.Green;
                default:
                    return Color.Default;
            }
        }
    }
}