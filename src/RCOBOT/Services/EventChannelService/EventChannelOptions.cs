namespace RCOBOT.Services.EventChannelService
{
    public class EventChannelOptions
    {
        public ulong SystemLogChannelId { get; set; }
        public ulong ChatMessagesChannelId { get; set; }
        public ulong PlayerJoinsChannelId { get; set; }
        public ulong PlayerDisconnectsChannelId { get; set; }
    }
}