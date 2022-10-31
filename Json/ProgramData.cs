using Newtonsoft.Json;

namespace tvpgo.Json
{
    public class ProgramData
    {
        private static readonly string PROGRAM_URL = "https://tvpstream.tvp.pl/api/tvp-stream/stream/data?station_code={0}";
        private static readonly string REPLAY_URL = "https://tvpstream.tvp.pl/api/tvp-stream/stream/data?station_code={0}&record_id={1}";
        public object error { get; set; }
        public ProgramDetails data { get; set; }
        public static async Task<ProgramDetails> Create(Station station, EpgShow show = null)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = show == null ? new HttpRequestMessage(HttpMethod.Get, string.Format(PROGRAM_URL, station.code)) : new HttpRequestMessage(HttpMethod.Get, string.Format(REPLAY_URL, station.code, show.record_id));
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var program = JsonConvert.DeserializeObject<ProgramData>(json);
            return program.data;
        }
    }
    public class ProgramDetails
    {
        public string type { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public ProgramImage image { get; set; }
        public string station_code { get; set; }
        public object record_id { get; set; }
        public string stream_url { get; set; }
        public bool violence_check { get; set; }
        public bool sex_check { get; set; }
        public bool profanity_check { get; set; }
        public bool drugs_check { get; set; }
        public object plrating { get; set; }
    }

    public class ProgramImage
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
