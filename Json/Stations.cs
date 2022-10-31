using Newtonsoft.Json;

namespace tvpgo.Json
{
    public class Stations
    {
        private static readonly string STATIONS_URL = "https://tvpstream.tvp.pl/api/tvp-stream/program-tv/stations";
        public static async Task<Stations> Create()
        {
            return await StaticTools.WebDeserializeAsync<Stations>(STATIONS_URL);
        }
        public object error { get; set; }
        public Station[] data { get; set; }
    }
    public class Station
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public Image image { get; set; }
        public Image image_square { get; set; }
        public string background_color { get; set; }
    }
}
