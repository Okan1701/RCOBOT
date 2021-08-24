using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RCOBOT
{
    class Program
    {
        // Main entry point
        // Creates a new instance of Host and runs it until app is terminated
        public static async Task Main(string[] args) => await new Host().RunAsync();

    }
}
