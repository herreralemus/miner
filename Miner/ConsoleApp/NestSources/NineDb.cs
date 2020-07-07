using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ConsoleApp.config.maps
{
    public class NineDb : NestSource
    {
        protected override List<Nest> Normalize(string data)
        {
            return ((JArray)JObject.Parse(data)["spots"]).ToList()
                .Where(n => n["genre"]?.ToString() == "nest")
                .Select
                (
                    n => new Nest
                    {
                        SpecimenId = TranslateToSpecimenId(n["nest"]?["data_id"]?.ToString() ?? ""),
                        Latitude = double.Parse(n["lat"].ToString()),
                        Longitude = double.Parse(n["lng"].ToString())
                    }
                )
                .ToList();
        }
    }
}
