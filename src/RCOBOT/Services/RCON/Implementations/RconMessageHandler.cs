using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using RCOBOT.Services.RCON.Interfaces;

namespace RCOBOT.Services.RCON.Implementations
{
    /// <summary>
    /// Main implementation of IRconMessageHandler. This handler is to be used for Arma 3 BE messages
    /// </summary>
    public class RconMessageHandler : IRconMessageHandler
    {
        // Prefixes used to identify the various type of chat messages
        private readonly string[] chatMessagePrefixes = {
            "(Side)",
            "(Group)",
            "(Direct)",
            "(Vehicle)",
            "(Global)"
        };

        // Events that get triggered by HandleMessage
        public event EventHandler<PlayerConnectedEventArgs> PlayerConnected;
        public event EventHandler<PlayerDisconnectEventArgs> PlayerDisconnected;
        public event EventHandler<ChatMessageEventArgs> ChatMessageReceived;

        /// <summary>
        /// Processes an incoming BE message from an Arma3 server
        /// </summary>
        /// <param name="message">The raw BE message</param>
        /// <returns></returns>
        public RconMessageType HandleMessage(string message)
        {
            // Player disconnect message
            if (message.StartsWith("Player #") && message.EndsWith("disconnected"))
            {
                PlayerDisconnected?.Invoke(this, GetDisconnectedEventArgs(message));
                return RconMessageType.PlayerDisconnected;
            }
            
            // Player connect message
            if (message.StartsWith("Player #") && message.EndsWith("connected"))
            {
                PlayerConnected?.Invoke(this, GetJoinedEventArgs(message));
                return RconMessageType.PlayerConnected;
            }

            // Check if it is a chat message by checking all the prefixes
            foreach (var prefix in chatMessagePrefixes)
            {
                if (message.StartsWith(prefix))
                {
                    ChatMessageReceived?.Invoke(this, GetChatMessageEventArgs(message));
                    return RconMessageType.ChatMessageReceived;
                }
            }

            // Return unknown if message was not identified
            return RconMessageType.Unknown;
        }

        /// <summary>
        /// Extract details from the chat message and creates an event object from that data
        /// </summary>
        /// <param name="message">The raw chat message</param>
        /// <returns>Event object containing details of the chat message</returns>
        private ChatMessageEventArgs GetChatMessageEventArgs(string message)
        {
            string[] splitStr = message.Split(":");
            // Headers are the channel type, followed by name
            string[] headers = splitStr[0].Split(" ");
            var channelType = headers[0];
            var name = string.Join(" ", headers.Skip(1));

            return new ChatMessageEventArgs
            {
                Content = string.Join(":", splitStr.Skip(1)),
                Message = message,
                Name = name,
                ChannelType = channelType,
                EventTime = DateTime.Now
            };
        }

        /// <summary>
        /// Create a player joined event object containing data from the raw message
        /// </summary>
        /// <param name="message">The raw join message</param>
        /// <param name="ignoreSuffixCount">How many words at the END of message should be ignored</param>
        /// <returns>Event object containing the joined player info</returns>
        private PlayerConnectedEventArgs GetJoinedEventArgs(string message, int ignoreSuffixCount = 2)
        {
            string name = "";
            int id = int.Parse(message.Substring(8, 1));
            
            // Extract player name
            string[] splitStr = message.Split(" ");
            // We skip the first 2 words since those are not part of the name
            for (int i = 2; i < (splitStr.Length - ignoreSuffixCount); i++)
            { // Add white space if needed for multi word names
                name += i == 2 ? splitStr[i] : $" {splitStr[i]}";
            }

            return new PlayerConnectedEventArgs
            {
                Message = message,
                Name = name,
                GameId = id,
                EventTime = DateTime.Now
            };
        }

        /// <summary>
        /// Create a player disconnect event object containing data from the raw message
        /// </summary>
        /// <param name="message">The raw disconnect message</param>
        /// <returns>Event object containing the disconnected player info</returns>
        private PlayerDisconnectEventArgs GetDisconnectedEventArgs(string message)
        {
            // With the current code base, the event object for both joining and leaving is identical
            // So we'll reuse the GetJoinedEventArgs method and then turn that result into a PlayerDisconnectEventArgs event
            // since the data will be identical
            var joinArgs = GetJoinedEventArgs(message, 1);
            return new PlayerDisconnectEventArgs
            {
                EventTime = joinArgs.EventTime,
                GameId = joinArgs.GameId,
                Message = joinArgs.Message,
                Name = joinArgs.Name
            };
        }
    }
}