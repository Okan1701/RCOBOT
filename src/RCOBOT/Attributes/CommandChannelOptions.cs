using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace RCOBOT.Attributes
{
    public class CommandChannelOptions
    {
        public ulong ChannelId { get; set; }
        public Dictionary<string, ulong[]> Permissions { get; set; }
    }
}