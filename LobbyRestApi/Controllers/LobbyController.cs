
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LobbyModels;
using LobbyModels.Interfaces;
using LobbyRestApi.Models;

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
        public IEnumerable<ILobbyModel> GetLobbies()
        {
            return GetLobbyList();
        }

        [HttpPost]
        public ILobbyPostMessage Post([FromBody] ILobbyPostMessage lobbyMessage)
        {
            var lobbies = GetLobbyWrapperList();
            var lobby = lobbyMessage.Lobby;
            lock (ThisLock)
            {
                switch (lobbyMessage.MessageType)
                {
                    case LobbyMessageType.CREATE:
                        lobby.LobbyId = GetFirstFreeId();
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
                }
            }
            CacheLobbyList(lobbies);
            return new LobbyPostMessage()
            {
                MessageType = LobbyMessageType.UPDATE,
                Lobby = lobby
            };
        }

        private int GetFirstFreeId()
        {
            if (GetLobbyWrapperList().Any())
            {
                var ints = GetLobbyWrapperList().Select(lw => lw.Lobby.LobbyId).ToArray();
                int? firstAvailable = Enumerable.Range(0, int.MaxValue)
                    .Except(ints)
                    .FirstOrDefault();
                return firstAvailable.Value;
            }

            return 1;
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

        private List<ILobbyModel> GetLobbyList()
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
            public ILobbyModel Lobby { get; set; }
            public IPAddress HostIp { get; set; }
            public string Region { get; set; } = "EUROPE";
        }
    }
}
