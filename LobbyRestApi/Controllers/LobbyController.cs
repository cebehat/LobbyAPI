
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
        [Route("/")]
        public IEnumerable<ILobbyModel> GetLobbies()
        {
            return GetLobbyList();
        }

        [HttpPost]
        [Route("/")]
        public int CreateLobby()
        {
            var lobby = new Lobby()
            {
                LobbyId = GetFirstFreeId()
            };
            LobbyWrapper wrapper = new LobbyWrapper()
            {
                HostIp = HttpContext.Connection.RemoteIpAddress,
                LastUpdate = DateTime.Now,
                Lobby = lobby
            };
            AddLobbyToCache(wrapper);
            return lobby.LobbyId;
        }

        [HttpPost]
        [Route("/KeepAlive")]
        public void KeepLobbyAlive([FromBody] int lobbyId)
        {
            var lobbies = GetLobbyWrapperList();
            lobbies.SingleOrDefault(lw => lw.Lobby.LobbyId == lobbyId).LastUpdate = DateTime.Now;
            CacheLobbyList(lobbies);
        }

        [HttpPost]
        [Route("/GetHost")]
        public string GetHostIp([FromBody] int lobbyId)
        {
            var lobbies = GetLobbyWrapperList();
            return lobbies.SingleOrDefault(lw => lw.Lobby.LobbyId == lobbyId).HostIp.ToString();
        }

        [HttpPost]
        [Route("/Delete")]
        public void DeleteLobby([FromBody] int lobbyId)
        {
            var lobbies = GetLobbyWrapperList();
            lobbies.Remove(lobbies.Single(lw => lw.Lobby.LobbyId == lobbyId));
            CacheLobbyList(lobbies);
        }




        //[HttpPost]
        //public ILobbyPostMessage Post([FromBody] ILobbyPostMessage lobbyMessage)
        //{
        //    var lobbies = GetLobbyWrapperList();
        //    var lobby = lobbyMessage.Lobby;
        //    lock (ThisLock)
        //    {
        //        switch (lobbyMessage.MessageType)
        //        {
        //            case LobbyMessageType.CREATE:
        //                lobby.LobbyId = GetFirstFreeId();
        //                lobbies.Add(new LobbyWrapper()
        //                {
        //                    LastUpdate = DateTime.Now, 
        //                    Lobby = lobby,
        //                    HostIp = Request.HttpContext.Connection.RemoteIpAddress
        //                });
        //                break;
        //            case LobbyMessageType.UPDATE:
        //                lobbies.SingleOrDefault(l => l.Lobby.LobbyId == lobby.LobbyId).Lobby.PlayerCount = lobby.PlayerCount;
        //                break;
        //            case LobbyMessageType.REMOVE:
        //                lobbies.RemoveAt(lobbies.IndexOf(lobbies.SingleOrDefault(l => l.Lobby.LobbyId == lobby.LobbyId)));
        //                break;
        //        }
        //    }
        //    CacheLobbyList(lobbies);
        //    return new LobbyPostMessage()
        //    {
        //        MessageType = LobbyMessageType.UPDATE,
        //        Lobby = lobby
        //    };
        //}

        private int GetFirstFreeId()
        {
            var list = GetLobbyWrapperList();
            int id = 1;
            if (list.Any())
            {
                var ints = list.Select(lw => lw.Lobby.LobbyId).ToArray();
                id = Enumerable.Range(0, int.MaxValue)
                    .Except(ints)
                    .FirstOrDefault();
            }
            return id;
        }

        private void AddLobbyToCache(LobbyWrapper lobbyWrapper)
        {
            var list = GetLobbyWrapperList();
            lock (ThisLock)
            {
                if (list.Any(lw => lw.Lobby.LobbyId == lobbyWrapper.Lobby.LobbyId)) return;
                list.Add(lobbyWrapper);
            }
            CacheLobbyList(list);
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
