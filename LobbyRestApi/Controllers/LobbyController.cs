using LobbyRestApi.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LobbyRestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LobbyController : ControllerBase
    {
        private static object ThisLock = new object();

        private readonly ILogger<LobbyController> _logger;
        private IMemoryCache _cache;
        private readonly string listKey = "lobbies";
        public LobbyController(ILogger<LobbyController> logger, IMemoryCache cache)
        {
            _logger = logger;
            _cache = cache;
        }

        [HttpGet]
        public IEnumerable<Lobby> GetLobbies()
        {
            return GetLobbyList();
        }

        [HttpPost]
        public LobbyPostMessage Post([FromBody] LobbyPostMessage lobbyMessage)
        {
            var lobbies = GetLobbyWrapperList();
            var lobby = lobbyMessage.lobby;
            lock (ThisLock)
            {
                
                switch (lobbyMessage.messageType)
                {
                    case LobbyMessageType.CREATE:
                        //if (lobbies.Any(l => l.Lobby.LobbyId == lobby.LobbyId)) return lobbies.Single(l => l.Lobby.LobbyId == lobby.LobbyId);
                        //lobby.LobbyId = 
                        lobbies.Add(new LobbyWrapper()
                        {
                            LastUpdate = DateTime.Now, 
                            Lobby = lobby,
                            HostIp = Request.HttpContext.Connection.RemoteIpAddress
                        });
                        break;
                    case LobbyMessageType.UPDATE:
                        lobbies.SingleOrDefault(l => l.Lobby.LobbyId == lobby.LobbyId).Lobby.PlayerCount = lobby.PlayerCount;
                        break;
                    case LobbyMessageType.REMOVE:
                        lobbies.RemoveAt(lobbies.IndexOf(lobbies.SingleOrDefault(l => l.Lobby.LobbyId == lobby.LobbyId)));
                        break;
                    default:
                        break;
                }
            }
            CacheLobbyList(lobbies);
            return new LobbyPostMessage()
            {
                messageType = LobbyMessageType.UPDATE,
                lobby = lobby
            };
        }

        private int GetFirstFreeId()
        {
            if (GetLobbyWrapperList().Any())
            {

            }
            else
            {
                return 1;
            }
        }

        [HttpPost("Alive/{lobbyId}")]
        public void KeepLobbyAlive(int lobbyId)
        {
            var lobbies = GetLobbyWrapperList();
            lobbies.SingleOrDefault(lw => lw.Lobby.LobbyId == lobbyId).LastUpdate = DateTime.Now;
            CacheLobbyList(lobbies);
        }

        [HttpPost("{lobbyId}")]
        public string GetHostIp(int lobbyId)
        {
            var lobbies = GetLobbyWrapperList();
            return lobbies.SingleOrDefault(lw => lw.Lobby.LobbyId == lobbyId).HostIp.ToString();
        }

        private List<LobbyWrapper> GetLobbyWrapperList()
        {
            var lobbies = new List<LobbyWrapper>();
            lock (ThisLock)
            {
                lobbies = _cache.GetOrCreate(listKey, cacheEntry =>
                {
                    return new List<LobbyWrapper>();
                });

            }
            return lobbies;
        }

        private List<Lobby> GetLobbyList()
        {
            return GetLobbyWrapperList().Select(lw => lw.Lobby).ToList();
        }

        private void CacheLobbyList(List<LobbyWrapper> lobbies)
        {
            lock (ThisLock)
            {
                _cache.Set(listKey, lobbies);
            }
        }

        private class LobbyWrapper
        {
            public DateTime LastUpdate { get; set; }
            public Lobby Lobby { get; set; }
            public IPAddress HostIp { get; set; }
            public string Region { get; set; } = "EUROPE";
        }
    }
}
