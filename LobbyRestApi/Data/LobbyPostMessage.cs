﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LobbyRestApi.Data
{
    public class LobbyPostMessage : ILobbyPostMessage
    {
        public LobbyMessageType messageType { get; set; }
        public Lobby lobby { get; set; }
    }

    public enum LobbyMessageType{
        CREATE = 1,
        UPDATE = 2,
        REMOVE = 3
    }
}
