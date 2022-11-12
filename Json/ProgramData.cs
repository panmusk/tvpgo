using Newtonsoft.Json;

namespace tvpgo.Json
{
    public class ProgramData
    {
        private static readonly string PROGRAM_URL = "https://tvpstream.tvp.pl/api/tvp-stream/stream/data?station_code={0}";
        private static readonly string REPLAY_URL = "https://tvpstream.tvp.pl/api/tvp-stream/stream/data?station_code={0}&record_id={1}";
        private static readonly string OCCURRENCE_URL = "https://tvpstream.tvp.pl/api/tvp-stream/stream/data?id={0}";
        public Error error { get; set; }
        public ProgramDetails data { get; set; }
        public static async Task<ProgramDetails> Create(Station station, EpgShow show = null)
        {
            var url = show == null ? string.Format(PROGRAM_URL, station.code) : string.Format(REPLAY_URL, station.code, show.record_id);
            var programDetails = await StaticTools.WebDeserializeAsync<ProgramData>(url);
            return programDetails.data;
        }
        public static async Task<ProgramDetails> Create(EpgShow occurrenceitem)
        {
            var url = string.Format(OCCURRENCE_URL, occurrenceitem.id);
            var programDetails = await StaticTools.WebDeserializeAsync<ProgramData>(url);
            return programDetails.data;
        }
    }
    public class ProgramDetails
    {
        public string type { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public Image image { get; set; }
        public string station_code { get; set; }
        public object record_id { get; set; }
        public string stream_url { get; set; }
        public bool violence_check { get; set; }
        public bool sex_check { get; set; }
        public bool profanity_check { get; set; }
        public bool drugs_check { get; set; }
        public object plrating { get; set; }
    }

}
