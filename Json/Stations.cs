using Newtonsoft.Json;

namespace tvpgo.Json
{
    public class Stations
    {
        private static readonly string STATIONS_URL = "https://tvpstream.tvp.pl/api/tvp-stream/program-tv/stations";
        public static async Task<Stations> Create()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, STATIONS_URL);
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var stations = JsonConvert.DeserializeObject<Stations>(json);
            return stations;
        }
        public object error { get; set; }
        public Station[] data { get; set; }
    }

    public class Datum
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public Image image { get; set; }
        public Image_Square image_square { get; set; }
        public string background_color { get; set; }
    }

    public class Image
    {
        public string type { get; set; }
        public string title { get; set; }
        public object point_of_origin { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public object description { get; set; }
    }

    public class Image_Square
    {
        public string type { get; set; }
        public string title { get; set; }
        public object point_of_origin { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public object description { get; set; }
    }

}
