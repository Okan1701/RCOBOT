namespace RCOBOT.Services.RCON
{
    public class ChatMessageEventArgs : RconEventArgs
    {
        public string Name { get; set; }
        public string Content { get; set; }
        public string ChannelType { get; set; }
    }
}