namespace RCOBOT.Services.RCON
{
    public class PlayerConnectedEventArgs : RconEventArgs
    {
        public string Name { get; set; }
        public int GameId { get; set; } 
    }
}