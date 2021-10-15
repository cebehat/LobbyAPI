using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LobbyModels.Interfaces;

namespace LobbyRestApi.Models
{
    public class Lobby : ILobbyModel
    {
        public int LobbyId { get; set; }
        public int PlayerCount { get; set; } = 1;
    }
}
