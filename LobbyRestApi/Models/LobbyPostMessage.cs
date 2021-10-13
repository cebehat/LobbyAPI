using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LobbyModels;
using LobbyModels.Interfaces;

namespace LobbyRestApi.Models
{
    public class LobbyPostMessage : ILobbyPostMessage
    {
        public LobbyMessageType MessageType { get; set; }
        public ILobbyModel Lobby { get; set; }
    }

    
}
