namespace RCOBOT.Services.RCON
{
    /// <summary>
    /// Enum for all the generic types of RCON message types
    /// </summary>
    public enum RconMessageType
    {
        PlayerConnected,
        PlayerDisconnected,
        ChatMessageReceived,
        Unknown
    }
}