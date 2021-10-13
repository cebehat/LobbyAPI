using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyModels.Interfaces
{
    public interface ILobbyModel
    {
        int LobbyId { get; set; }
        int PlayerCount { get; set; }
    }
}
