using LobbyApiInterfacer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace LobbyApiInterfacer
{
    public class RequestHandler
    {
        private string _apiUri;
        private HttpClient client = new HttpClient();

        public RequestHandler(string apiUri)
        {
            _apiUri = apiUri;
        }

        public IEnumerable<LobbyModel> GetLobbies()
        {
            var response = client.GetAsync(_apiUri).Result;
            var resultString = response.Content.ReadAsStringAsync().Result;
            JArray array = (JArray)JsonConvert.DeserializeObject(resultString);
            var lobbies = array.ToObject<List<LobbyModel>>();
            return lobbies;
        }

        public int Create()
        {
            var response = client.PostAsync(_apiUri, null).Result;
            var resultString = response.Content.ReadAsStringAsync().Result;
            int id = 0;
            int.TryParse(resultString, out id);
            return id;
        }

        public void KeepAlive()
        {

        }

        public string GetHost(int lobbyId)
        {
            var content = new StringContent(JsonConvert.SerializeObject(lobbyId), Encoding.UTF8, "application/json");
            var response = client.PostAsync(_apiUri + "/GetHost", content).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        public void Delete(int lobbyId)
        {
            var content = new StringContent(JsonConvert.SerializeObject(lobbyId), Encoding.UTF8, "application/json");
            var response = client.PostAsync(_apiUri + "/Delete", content).Result;
        }
    }
}
