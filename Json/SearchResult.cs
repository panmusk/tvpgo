using System.Web;
using System.Buffers;
namespace tvpgo.Json
{

    public class SearchResults
    {
        private static readonly string SEARCH_URL = "https://tvpstream.tvp.pl/api/tvp-stream/search?query={0}&scope={1}&limit=20&page={2}";
        public Error? error { get; set; }
        public Data data { get; set; }
        public static async Task<SearchResults> Create(string query, SearchScope scope, int page=1)
        {
            return await StaticTools.WebDeserializeAsync<SearchResults>(string.Format(SEARCH_URL, HttpUtility.UrlEncode(query), scope.ToString(), page));
        }
    }

    public class Data
    {
        public EpgShow[] occurrenceitem { get; set; }
    }

    public class Occurrenceitem
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
        public int plrating { get; set; }
        public string[] akpa_attributes { get; set; }
        public object tabs { get; set; }
        public string id_szarp_aud { get; set; }
        public int? vortal_id { get; set; }
        public bool violence_check { get; set; }
        public bool sex_check { get; set; }
        public bool profanity_check { get; set; }
        public bool drugs_check { get; set; }
        public float _score { get; set; }
    }
}