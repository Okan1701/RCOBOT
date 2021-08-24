using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RCOBOT.Services.EventChannelService
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the required services for handling the event channels
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options">Lambda for configuring RCON options</param>
        public static void AddEventChannels (this IServiceCollection services, Action<EventChannelOptions> options)
        {
            services.Configure(options);
            services.AddSingleton<IEventChannelService, EventChannelService>();
        }
    }
}
