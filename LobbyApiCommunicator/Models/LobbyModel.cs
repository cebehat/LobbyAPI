using LobbyModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyApiCommunicator.Models
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
