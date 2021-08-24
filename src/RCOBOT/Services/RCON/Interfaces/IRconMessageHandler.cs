using System;

namespace RCOBOT.Services.RCON.Interfaces
{
    /// <summary>
    /// Base interface for services that implement the RconMessageHandler service
    /// </summary>
    public interface IRconMessageHandler
    {
        /// <summary>
        /// Processes the provided RCON message and triggers the appropriate event
        /// </summary>
        /// <param name="message">The raw RCON message</param>
        /// <returns>The type of message</returns>
        RconMessageType HandleMessage(string message);
        
        event EventHandler<PlayerConnectedEventArgs> PlayerConnected;
        event EventHandler<PlayerDisconnectEventArgs> PlayerDisconnected;
        event EventHandler<ChatMessageEventArgs> ChatMessageReceived;
    }
}