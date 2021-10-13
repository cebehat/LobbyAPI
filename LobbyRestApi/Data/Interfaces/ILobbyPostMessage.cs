using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyRestApi.Data
{
    interface ILobbyPostMessage
    {
        public LobbyMessageType messageType { get; set; }
        public Lobby lobby { get; set; }
    }
}
