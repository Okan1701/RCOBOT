using System;
using Discord;

namespace RCOBOT.Services.Logger
{
    public class LogEventArgs
    {
        public string Prefix { get; set; }
        public string Message { get; set; }
        public DateTime DateTime { get; set; }

        public LogEventArgs(string prefix, string message, DateTime date)
        {
            Prefix = prefix;
            Message = message;
            DateTime = date;
        }
    }
}