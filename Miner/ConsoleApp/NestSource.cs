using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace ConsoleApp
{
    public abstract class NestSource
    {
        private JObject _config;

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

        protected abstract List<Nest> Normalize(string data);

        protected string TranslateToSpecimenId(string id)
        {
            return _config["specimens"]?[id]?.ToString();
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
                        .ForEach(n => 
                            {
                                n.CountryId = Config["country_id"]?.ToString();
                                Console.WriteLine(
                                    JsonConvert.SerializeObject(
                                        n, 
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
                        );
                }
            }
        }
    }
}
