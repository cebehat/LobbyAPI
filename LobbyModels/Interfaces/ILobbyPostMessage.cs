using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyModels.Interfaces
{
    public interface ILobbyPostMessage
    {
        LobbyMessageType MessageType { get; set; }
        ILobbyModel Lobby { get; set; }
    }
}
