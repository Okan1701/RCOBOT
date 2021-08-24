using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BattleNET;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RCOBOT.Services.Logger;
using RCOBOT.Services.RCON.Interfaces;

namespace RCOBOT.Services.RCON.Implementations
{
    /// <summary>
    /// Implementation of IRconService that uses the BattleNET library to create RCON connections.
    /// To be used for Arma 3 BE servers
    /// </summary>
    public class BattleNetRconService : IRconService
    {
        // The BattleNET client
        private BattlEyeClient client;
        private readonly ILogger logger;

        // Message handler service to use for processing incoming messages
        private readonly IRconMessageHandler rconMessageHandler;

        /// <summary>
        /// Dictionary used by ExecuteCommandAsync. It is used to store messages that belong to a command that was executed
        /// Integer key is the Packet ID of the message
        /// </summary>
        private readonly ConcurrentDictionary<int, string> pendingCommandResults;

        /// <summary>
        /// Check if underlying BattleNET client is connected
        /// </summary>
        public bool IsConnected => client.Connected;

        public IOptions<RconConnectionOptions> Options { get; }

        public BattleNetRconService(ILogger logger, IRconMessageHandler msgHandler, IOptions<RconConnectionOptions> options)
        {
            this.logger = logger;
            rconMessageHandler = msgHandler;
            Options = options;
            pendingCommandResults = new ConcurrentDictionary<int, string>();
            logger.LogInformation("RCON service created!");
        }

        /// <summary>
        /// Main event handler for incoming messages
        /// </summary>
        /// <param name="args"></param>
        private void ClientOnBattlEyeMessageReceived(BattlEyeMessageEventArgs args)
        {
            // First check if incoming message is a reply to a command
            // that was executed via ExecuteCommandAsync
            foreach (var keyValue in pendingCommandResults)
            {
                if (keyValue.Key == args.Id)
                {
                    // Store the message in the dictionary so that the active ExecuteCommandAsync van view it
                    pendingCommandResults[keyValue.Key] = args.Message;
                    return;
                }
            }

            // If not a pending message, we hand it over to the message handler
            rconMessageHandler.HandleMessage(args.Message);
            //logger.LogInformation($"RCON MESSAGE RECEIVED (ID={args.Id}){Environment.NewLine}{args.Message}");
        }

        private void ClientOnBattlEyeDisconnected(BattlEyeDisconnectEventArgs args)
        {
            logger.LogError("BattleNET RCON client has disconnected!");
        }

        private void ClientOnBattlEyeConnected(BattlEyeConnectEventArgs args)
        {
            logger.LogInformation("BattleNET RCON client connected!");
        }

        /// <summary>
        /// Creates a new BattleNET client instance with the correct credentials and establishes a connection
        /// </summary>
        public void Connect()
        {
            // Setup client and events
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Create credentials
            var credentials = new BattlEyeLoginCredentials
            {
                Host = Dns.GetHostAddresses(Options.Value.Host)[0],
                Password = Options.Value.Password,
                Port = Options.Value.Port
            };

            // Setup client
            client = new BattlEyeClient(credentials);
            client.ReconnectOnPacketLoss = true;
            client.BattlEyeConnected += ClientOnBattlEyeConnected;
            client.BattlEyeDisconnected += ClientOnBattlEyeDisconnected;
            client.BattlEyeMessageReceived += ClientOnBattlEyeMessageReceived;

            // Create the connection
            BattlEyeConnectionResult res = client.Connect();

            // Print to console if an error occured
            if (res != BattlEyeConnectionResult.Success)
            {
                logger.LogError(
                    $"Failed to establish RCON connection for host: {credentials.Host} ({res})");
            }
        }

        /// <inheritdoc cref="IRconService"/>>
        // TODO: Method will always timeout on commands that do not receive a reply
        public Task<string> ExecuteCommandAsync(string command, ICommandContext context)
        {
            return Task.Run(() =>
            {
                int id = client.SendCommand(command);
                pendingCommandResults[id] = "";

                // Wait until we received a reply
                // If no message is received after 60 seconds, we abort to prevent infinite loop
                var timer = new Stopwatch();
                timer.Start();
                while (pendingCommandResults[id] == "")
                {
                    if (timer.ElapsedMilliseconds > 30000)
                    {
                        // Abort and log to console
                        logger.LogError($"Command {context.Message.Content} timed out! (packet = {id})");
                        throw new TimeoutException($"No reply received from BE server! (packet = {id})");
                    }
                }

                timer.Stop();
                return pendingCommandResults[id];
            });
        }
    }
}