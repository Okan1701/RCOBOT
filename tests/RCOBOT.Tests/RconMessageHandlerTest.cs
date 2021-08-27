using RCOBOT.Services.RCON;
using RCOBOT.Services.RCON.Implementations;
using Xunit;

namespace RCOBOT.Tests
{
    public class RconMessageHandlerTest
    {
        [Theory]
        [InlineData("Player #6 nadam (22.26.99.99:2304) connected",RconMessageType.PlayerConnected)]
        [InlineData("Player #6 nadam (22.26.99.99:2304) disconnected",RconMessageType.PlayerDisconnected)]
        [InlineData("(Global) Helle Duval: imagine cheating in arma tho [REDACTED] :)", RconMessageType.ChatMessageReceived)]
        [InlineData("(Side) Altay: Welcome to the /server/ arma3!!!", RconMessageType.ChatMessageReceived)]
        [InlineData("(Random) random message that makes no sense", RconMessageType.Unknown)]
        public void HandleMessage_AllMsgTypes_ReturnCorrectType(string message, RconMessageType expectedType)
        {
            var handler = new RconMessageHandler();

            var type = handler.HandleMessage(message);
            
            Assert.Equal(expectedType, type);
        }

        [Theory]
        [InlineData("Player #5 UltimateAntic (22.26.99.99:2304) connected", "UltimateAntic", 5)]
        [InlineData("Player #6 Multi Word Name (22.26.99.99:2304) connected", "Multi Word Name", 6)]
        public void HandleMessage_PlayerJoin_CorrectEventArgs(string message, string expectedName, int expectedId)
        {
            var handler = new RconMessageHandler();
            handler.PlayerConnected += (sender, args) =>
            {
                Assert.Equal(expectedName, args.Name);
                Assert.Equal(expectedId, args.GameId);
            };

            handler.HandleMessage(message);
        }
        
        [Theory]
        [InlineData("Player #5 UltimateAntic disconnected", "UltimateAntic", 5)]
        [InlineData("Player #6 Multi Word Name disconnected", "Multi Word Name", 6)]
        public void HandleMessage_PlayerDisconnect_CorrectEventArgs(string message, string expectedName, int expectedId)
        {
            var handler = new RconMessageHandler();
            handler.PlayerDisconnected += (sender, args) =>
            {
                Assert.Equal(expectedName, args.Name);
                Assert.Equal(expectedId, args.GameId);
            };

            handler.HandleMessage(message);
        }
        
        [Theory]
        [InlineData("(Global) Helle Duval: imagine cheating in arma tho [REDACTED] :)", "Helle Duval", "(Global)", " imagine cheating in arma tho [REDACTED] :)")]
        [InlineData("(Side) Altay: Welcome to the /server/ arma3!!!", "Altay", "(Side)", " Welcome to the /server/ arma3!!!")]
        public void HandleMessage_ChatMessage_CorrectEventArgs(string message, string expectedName, string expectedChannelType, string expectedContent)
        {
            var handler = new RconMessageHandler();
            handler.ChatMessageReceived += (sender, args) =>
            {
                Assert.Equal(expectedName, args.Name);
                Assert.Equal(expectedChannelType, args.ChannelType);
                Assert.Equal(expectedContent, args.Content);
            };

            handler.HandleMessage(message);
        }
    }
}