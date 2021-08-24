using System;
using System.Threading.Tasks;
using Discord;

namespace RCOBOT.Services.Logger
{
    public interface ILogger
    {
        public void LogInformation(string message);
        public void LogError(Exception exception);
        public void LogError(string message);
        public Task Log(string prefix, string message);

        public Task Log(LogMessage log);

        event EventHandler<LogEventArgs> OnLogged;
    }
}