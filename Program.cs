using System.Linq;
using System;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using CommandLine;
using tvpgo.Json;
using Spectre.Console;
namespace tvpgo
{
    public class Options
    {
        [Option('s', "station", HelpText = "three-letter station, code, possible station codes")]
        public string StationCode { get; set; }
        [Option('b', "begin", Required = false)]
        public string Begin { get; set; }

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
        public static readonly long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        private static async Task Main(string[] args)
        {
            var channels = await GetChannels();
            var channel = await GetChannel(channels);
            var epg = await GetEpg(channel);
            var show = epg.data.OrderBy(x=>x.date_start).Where(x=> x.date_end >= now).First();
            var live = await GetLive(show);
            if (live)
            {
                var program = await GetProgram(channel);
                var token = await GetToken(program.stream_url);
                var format = GetFormat(token);
                Play(show, format);
            }else{
                show = await GetShow(epg);
                var program = await GetProgram(channel, show);
                var token = await GetToken(program.stream_url);
                Format format = GetFormat(token);
                Play(show, format);
            }
        }

        private static Format GetFormat(Tokenizer token)
        {
            Format format = new Format();
            foreach (var f in token.formats)
            {
                if (f.mimeType.Equals("application/x-mpegurl"))
                {
                    format = f;
                    break;
                }
            }
            return format;
        }

        private static async Task<IEnumerable<Channel>> GetChannels()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, STATIONS_URL);
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var stations = JsonConvert.DeserializeObject<Channels>(json);
            IEnumerable<Channel> Stations = stations.data.Select(x => new Channel { Code = x.code, Name = x.name });
            return Stations;
        }
        private static async Task<ProgramDetails> GetProgram(Channel station)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, string.Format(PROGRAM_URL, station.Code));
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var program = JsonConvert.DeserializeObject<ProgramData>(json);
            return program.data;
        }
        private static async Task<ProgramDetails> GetProgram(Channel station, EpgShow show)
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
        private static async Task<Epg> GetEpg(Channel station)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, string.Format(EPG_URL, station.Code));
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var epg = JsonConvert.DeserializeObject<Epg>(json);
            return epg;
        }

        public static async Task<EpgShow> GetShow(Epg epg)
        {
            var ordered = epg.data.Where(x=> x.date_start < now).OrderBy(x => x.date_start).Select(x=> Tuple.Create(x.record_id, x.title, UnixTimeStampToDateTime(x.date_start).ToString()));
            var programs = AnsiConsole.Prompt(
                new SelectionPrompt<Tuple<string, string, string>>()
                .PageSize(20)
                .Title("Choose program")
                .AddChoices(
                    ordered
                )
            );
            var show = epg.data.SingleOrDefault(x=> x.record_id.Equals(programs.Item1));
            return show;
        }
        public static async Task<Channel> GetChannel(IEnumerable<Channel> stations)
        {
            var stationList = stations.Select(x=> Tuple.Create(x.Name, x.Code));
            var programs = AnsiConsole.Prompt(
                new SelectionPrompt<Tuple<string, string>>()
                .PageSize(20)
                .Title("Choose channel")
                .AddChoices(
                    stationList
                )
                .HighlightStyle(Style.WithBackground(Color.Aqua))
            );
            var channel = stations.SingleOrDefault(x=> x.Code.Equals(programs.Item2));
            return channel;
        }
        public static async Task<bool> GetLive(EpgShow show)
        {
            var live = AnsiConsole.Prompt(
                new SelectionPrompt<Tuple<string, bool>>()
                .PageSize(20)
                .Title("Choose channel")
                .AddChoices(
                    Tuple.Create($"Play live stream ({show.title})", true),
                    Tuple.Create("Select program to play", false)
                )
                .HighlightStyle(Style.WithBackground(Color.Aqua))
            );
            return live.Item2;
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
    internal class Channel
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
