using Geohash;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ConsoleApp
{
    public abstract class NestSource
    {
        private Dictionary<string, Nest> nests = new Dictionary<string, Nest>();
        private JObject _config;
        private readonly string _seasonId = "1593043200000";
        private readonly string _authorId = "y9ikY8FCVhUOtxGftsPkC12BusO2";

        protected JObject Config => _config ?? 
            (
                _config = JObject.Parse
                (
                    File.ReadAllText
                    (
                        Path.Combine
                        (
                            Directory.GetCurrentDirectory(),
                            "NestSources",
                            GetType().Name + ".json"
                        )
                    )
                )
            );

        protected abstract IEnumerable<Nest> Normalize(string data);

        protected string TranslateToSpecimenId(string id)
        {
            var newId = _config["specimens"]?[id]?.ToString();
            if(newId == null)
                Console.WriteLine($"ERROR: Id \"{id}\" was not translated");
            return newId;
        }

        protected void Add(Nest nest)
        {
            var hasher = new Geohasher();
            var id = _seasonId + "|" + _authorId + "|" + nest.SpecimenId + "|" + hasher.Encode(nest.Latitude, nest.Longitude, 7);
            nests.TryAdd(id, nest);
        }

        protected void Print()
        {
            Console.WriteLine(
                JsonConvert.SerializeObject(
                    nests,
                    new JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new SnakeCaseNamingStrategy(),
                        },
                        Formatting = Formatting.Indented
                    }
                )
            );
        }

        public void Mine()
        {
            foreach (var request in (JObject)Config["requests"])
            {
                var url = Config["url"].ToString();
                foreach (var param in (JObject)request.Value)
                {
                    url = url.Replace("{" + param.Key + "}", param.Value.ToString());
                }
                using (var client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    client.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    Normalize(client.DownloadString(url))
                        .Where(n => n.SpecimenId != null)
                        .ToList()
                        .ForEach(
                            n => {
                                n.CountryId = Config["country_id"]?.ToString();
                                Add(n); 
                            }
                        );
                }
            }
            Print();
        }
    }
}
