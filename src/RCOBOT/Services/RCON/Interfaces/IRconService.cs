using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace RCOBOT.Services.RCON.Interfaces
{
    /// <summary>
    /// Interface for the base RCON service that is responsible for maintaining the RCON connection and receiving messages
    /// </summary>
    public interface IRconService
    {
        /// <summary>
        /// Property for checking if RCON connection is active
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Method for creating the connection to the remote server
        /// </summary>
        void Connect();

        /// <summary>
        /// Execute an rcon command by executing it on the remote server and awaiting a reply
        /// </summary>
        /// <param name="command">The raw command query to execute</param>
        /// <param name="context">The discord message context that wants to execute the command</param>
        /// <returns></returns>
        Task<string> ExecuteCommandAsync(string command, ICommandContext context);
    }
}