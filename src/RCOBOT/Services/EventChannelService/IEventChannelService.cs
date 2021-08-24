using Microsoft.Extensions.Options;

namespace RCOBOT.Services.EventChannelService
{
    public interface IEventChannelService
    {
        public IOptions<EventChannelOptions> Options { get; }
    }
}