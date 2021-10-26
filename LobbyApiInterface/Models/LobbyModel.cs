using LobbyModels.Interfaces;

namespace LobbyApiInterface.Models
{
    public class LobbyModel : ILobbyModel
    {
        public int LobbyId { get; set; }
        public int PlayerCount { get; set; }

        public override string ToString()
        {
            return string.Format("Lobby - id:{0} - with {1} players", LobbyId, PlayerCount);
        }
    }
}
