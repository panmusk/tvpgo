
using Newtonsoft.Json;

namespace tvpgo.Json
{
    public class Epg
    {
        private static readonly string EPG_URL = "https://tvpstream.tvp.pl/api/tvp-stream/program-tv/index?station_code={0}";
        private static readonly string EPG_DATE_URL =  "https://tvpstream.tvp.pl/api/tvp-stream/program-tv/index?station_code={0}&date={1}";
        public static async Task<Epg> Create(Station station, DateTime? date = null)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = date == null ? new HttpRequestMessage(HttpMethod.Get, string.Format(EPG_URL, station.code)) : new HttpRequestMessage(HttpMethod.Get, string.Format(EPG_DATE_URL, station.code, date.Value.ToString("yyyy-MM-dd")));
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var epg = JsonConvert.DeserializeObject<Epg>(json);
            return epg;
        }

        public object error { get; set; }
        public EpgShow[] data { get; set; }

    }

    public class EpgShow
    {
        public string id { get; set; }
        public string record_id { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public long date_start { get; set; }
        public long date_end { get; set; }
        public int duration { get; set; }
        public string station_code { get; set; }
        public string description { get; set; }
        public string description_long { get; set; }
        public Program program { get; set; }
        public Station station { get; set; }
        public Category[] categories { get; set; }
        public string[] akpa_attributes { get; set; }
        public object tabs { get; set; }
        public string id_szarp_aud { get; set; }
        public int? vortal_id { get; set; }
    }

    public class Program
    {
        public string id { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public int? year { get; set; }
        public string lang { get; set; }
        public object cms_id { get; set; }
        public EpgImage image { get; set; }
        public Program_Type program_type { get; set; }
        public Cycle cycle { get; set; }
        public int? rating { get; set; }
    }

    public class Program_Type
    {
        public string id { get; set; }
        public string type { get; set; }
        public string title { get; set; }
    }

    public class Cycle
    {
        public string id { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public Image_Logo image_logo { get; set; }
    }

    public class Image_Logo
    {
        public string type { get; set; }
        public string title { get; set; }
        public object point_of_origin { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public object description { get; set; }
    }

    public class Station
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string code { get; set; }
        public EpgImage image { get; set; }
        public EpgImage_Square image_square { get; set; }
        public string background_color { get; set; }
    }

    public class EpgImage
    {
        public string type { get; set; }
        public string title { get; set; }
        public object point_of_origin { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public object description { get; set; }
    }

    public class EpgImage_Square
    {
        public string type { get; set; }
        public string title { get; set; }
        public object point_of_origin { get; set; }
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public object description { get; set; }
    }

    public class Category
    {
        public string id { get; set; }
        public string type { get; set; }
        public string category_type { get; set; }
        public string title { get; set; }
    }

}
