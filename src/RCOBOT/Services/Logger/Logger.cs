using System;
using System.Threading.Tasks;
using Discord;

namespace RCOBOT.Services.Logger
{
    public class Logger : ILogger
    {
        public event EventHandler<LogEventArgs> OnLogged;

        public void LogInformation(string message) => Log("Info", message);

        public void LogError(Exception exception) => Log("Exception",
            exception.Message + Environment.NewLine + exception.StackTrace);

        public void LogError(string message) => Log("Error", message);

        public Task Log(string prefix, string message)
        {
            var dateTime = DateTime.Now;
            Console.WriteLine($"[{dateTime}] || {prefix} || {message}");
            OnLogged?.Invoke(this, new LogEventArgs(prefix, message, dateTime));
            return Task.CompletedTask;
        }

        public Task Log(LogMessage log) => Log(log.Severity.ToString(), log.Message != null ? log.Message : log.Exception.Message);
    }
}