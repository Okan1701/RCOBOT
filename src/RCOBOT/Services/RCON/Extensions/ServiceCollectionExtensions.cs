using System;
using BattleNET;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RCOBOT.Services.RCON.Implementations;
using RCOBOT.Services.RCON.Interfaces;

namespace RCOBOT.Services.RCON.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the required services and options for Arma 3 BE RCON operations
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options">Lambda for configuring RCON options</param>
        public static void AddArmaRcon(this IServiceCollection services, Action<RconConnectionOptions> options)
        {
            services.Configure(options);
            services.AddSingleton<IRconService, BattleNetRconService>();
            services.AddSingleton<IRconMessageHandler, RconMessageHandler>();
        }
    }
}