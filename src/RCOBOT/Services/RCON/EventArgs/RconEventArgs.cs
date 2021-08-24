using System;

namespace RCOBOT.Services.RCON
{
    public class RconEventArgs
    {
        public string Message { get; set; }
        public DateTime EventTime { get; set; }
    }
}