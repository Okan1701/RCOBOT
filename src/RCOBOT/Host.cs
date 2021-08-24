using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Discord;
using System.Net.Http;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using RCOBOT.Services;
using RCOBOT.Services.EventChannelService;
using RCOBOT.Services.Logger;
using RCOBOT.Services.RCON.Extensions;
using RCOBOT.Services.RCON.Interfaces;

namespace RCOBOT
{
    public class Host
    {
        /// <summary>
        /// Builds all the required services and starts the discord client
        /// </summary>
        public async Task RunAsync()
        {
            // Build configuration file
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            // Create the service provider and get the required services
            var serviceProvider = ConfigureServices(new ServiceCollection(), config);
            var client = (DiscordSocketClient) serviceProvider.GetRequiredService<IDiscordClient>();
            var logger = serviceProvider.GetRequiredService<ILogger>();
            var rcon = serviceProvider.GetRequiredService<IRconService>();
            serviceProvider.GetRequiredService<IEventChannelService>();

            // Setup logging
            client.Log += logger.Log;
            serviceProvider.GetRequiredService<CommandService>().Log += logger.Log;

            // Connect to discord
            await client.LoginAsync(TokenType.Bot, config.GetSection("DiscordToken").Value);
            await client.StartAsync();
            await client.SetActivityAsync(new Game(config.GetSection("PlayingStatus").Value));

            rcon.Connect();

            // Here we initialize the logic required to register our commands.
            await serviceProvider.GetRequiredService<CommandHandler>().InstallCommandsAsync();

            // Keep app running
            await Task.Delay(Timeout.Infinite);
        }

        /// <summary>
        /// This method is for registering all the application services
        /// </summary>
        /// <param name="services">The service container for registering services</param>
        /// <param name="config">Current application configuration</param>
        /// <returns>The service provider that is built from the container</returns>
        private IServiceProvider ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddScoped(_ => config);

            services.AddEventChannels(options => config.GetSection("EventChannels").Bind(options));
            services.AddArmaRcon(options => config.GetSection("RconConnection").Bind(options));

            services.AddScoped<ILogger, Logger>();
            services.AddSingleton<IDiscordClient, DiscordSocketClient>();
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandler>();
            services.AddSingleton<HttpClient>();

            // Built and return the service provider
            return services.BuildServiceProvider();
        }
    }
}