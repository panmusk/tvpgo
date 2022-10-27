using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using CommandLine;
using tvpgo.Json;

namespace tvpgo
{
    public class Options
    {
        [Option('s', "station", Required = true, HelpText = "three-letter station, code, possible station codes")]
        public string StationCode { get; set; }
        [Option('b', "begin", Required = false)]
        public string Begin { get; set; }

        [Option('e', Required = false)]
        public bool Epg { get; set; }
        [Option('r', Required = false)]
        public string ReocordId { get; set; }
        [Option('p', Required = false)]
        public bool Play { get; set; }
    }
    internal class Program
    {
        public static readonly string STATIONS_URL = "https://tvpstream.tvp.pl/api/tvp-stream/program-tv/stations";
        public static readonly string PROGRAM_URL = "https://tvpstream.tvp.pl/api/tvp-stream/stream/data?station_code={0}";
        public static readonly string EPG_URL = "https://tvpstream.tvp.pl/api/tvp-stream/program-tv/index?station_code={0}";
        public static readonly string REPLAY_URL = "https://tvpstream.tvp.pl/api/tvp-stream/stream/data?station_code={0}&record_id={1}";
        private static async Task Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsedAsync(async o =>
            {
                var station = (await GetStations()).SingleOrDefault(x => x.Code.Equals(o.StationCode, StringComparison.InvariantCultureIgnoreCase));
                if (!o.Epg)
                {
                    string url;
                    ProgramDetails program;
                    EpgShow show =  new EpgShow();
                    if (string.IsNullOrEmpty(o.ReocordId))
                    {
                        program = await GetProgram(station);
                    }else{
                        var epg = await GetEpg(station);
                        show = epg.data.SingleOrDefault(x=> x.record_id.Equals(o.ReocordId));
                        program = await GetProgram(station, show);
                    }
                    var token = await GetToken(program.stream_url);
                    var formats = token.formats;
                    Format format = new Format();
                    foreach (var f in formats)
                    {
                        if (f.mimeType.Equals("application/x-mpegurl"))
                        {
                            format = f;
                            break;
                        }
                    }
                    url = format.url;
                    if (o.Play)
                    {
                        Play(show, format);
                    }else{
                        System.Console.WriteLine(format.url);
                    }
                }
                else
                {
                    var epg = await GetEpg(station);
                    var epglist = await ListEpg(epg);
                    System.Console.WriteLine(epglist);
                }
            });
            Parser.Default.ParseArguments<Options>(args).WithNotParsedAsync(async o =>
            {
                string stations = await ListStations();
                System.Console.WriteLine(stations);
            });
        }
        public static async Task<string> ListStations()
        {
            var stations = await GetStations();
            StringBuilder sb = new StringBuilder();
            foreach (var station in stations)
            {
                sb.AppendLine($"{station.Code}\t{station.Name}");
            }
            return sb.ToString();
        }

        private static async Task<IEnumerable<Station>> GetStations()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, STATIONS_URL);
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var stations = JsonConvert.DeserializeObject<Stations>(json);
            IEnumerable<Station> Stations = stations.data.Select(x => new Station { Code = x.code, Name = x.name });
            return Stations;
        }
        private static async Task<ProgramDetails> GetProgram(Station station)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, string.Format(PROGRAM_URL, station.Code));
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var program = JsonConvert.DeserializeObject<ProgramData>(json);
            return program.data;
        }
        private static async Task<ProgramDetails> GetProgram(Station station, EpgShow show)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, string.Format(REPLAY_URL, station.Code, show.record_id));
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var program = JsonConvert.DeserializeObject<ProgramData>(json);
            return program.data;
        }
        private static async Task<Tokenizer> GetToken(string url)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, url);
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var token = JsonConvert.DeserializeObject<Tokenizer>(json);
            return token;
        }
        private static async Task<Epg> GetEpg(Station station)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, string.Format(EPG_URL, station.Code));
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var epg = JsonConvert.DeserializeObject<Epg>(json);
            return epg;
        }

        public static async Task<string> ListEpg(Epg epg)
        {
            //var ordered = epg.data.OrderBy(x => x.date_start).Select(x=> new{ID = x.record_id, Title = x.title, StartTime = UnixTimeStampToDateTime(x.date_start)});
            var ordered = epg.data.OrderBy(x => x.date_start).Select(x=> Tuple.Create(x.record_id, x.title, UnixTimeStampToDateTime(x.date_start).ToString()));
            // StringBuilder sb = new StringBuilder();
            // foreach (var show in ordered)
            // {
            //     sb.AppendLine($"{show.record_id}\t{show.title}\t{UnixTimeStampToDateTime(show.date_start)}");
            // }
            // return sb.ToString();
            return ordered.ToStringTable(new[] {"Id", "Title", "Start Time"}, x=> x.Item1, x=>x.Item2, x=>x.Item3);
        }
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        public static void Play(EpgShow show, Format format)
        {
            List<string> args = new List<string>();
            args.Add(format.url);
            args.Add("-title");
            args.Add($"\"{show.title}\"");
            ProcessStartInfo startInfo = new ProcessStartInfo(){
                FileName = "mpv",
                Arguments = string.Join(' ', args)
            };
            Process.Start(startInfo);
        }
    }
    internal class Station
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
