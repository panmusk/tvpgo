using System.Text.RegularExpressions;
using System.Globalization;
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
        [Option('r', "record", Required = true,  HelpText = "Save stream to a file indstead of playing")]
        public bool Record { get; set; }
    }
    internal class Program
    {
        public static readonly string STATIONS_URL = "https://tvpstream.tvp.pl/api/tvp-stream/program-tv/stations";
        public static readonly string PROGRAM_URL = "https://tvpstream.tvp.pl/api/tvp-stream/stream/data?station_code={0}";
        public static readonly string EPG_URL = "https://tvpstream.tvp.pl/api/tvp-stream/program-tv/index?station_code={0}";
        public static readonly string EPG_DATE_URL =  "https://tvpstream.tvp.pl/api/tvp-stream/program-tv/index?station_code={0}&date={1}";
        public static readonly string REPLAY_URL = "https://tvpstream.tvp.pl/api/tvp-stream/stream/data?station_code={0}&record_id={1}";
        public static readonly long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        public static readonly Color HIGHLIGHT_CLOR = Color.Blue;
        public static bool Record { get; set; }
        private static async Task Main(string[] args)
        {
            if (args.Length != 0 && args.Contains("-r"))
            {
                Record = true;
            }
            var channels = await GetChannels();
            var channel = await GetChannel(channels);
            var epg = await GetEpg(channel);
            var currentShow = epg.data.OrderBy(x=>x.date_start).Where(x=> x.date_end >= now).First();
            var live = await GetLive(currentShow);
            Format format = new Format();
            ProgramDetails program = new ProgramDetails();
            if (live)
            {
                program = await GetProgram(channel);
            }else{
                currentShow = await GetShow(epg);
                program = await GetProgram(channel, currentShow);
            }
                var token = await GetToken(program.stream_url);
                format = GetFormat(token);
                Play(currentShow, format);
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

        private static async Task<IEnumerable<Station>> GetChannels()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, STATIONS_URL);
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var stations = JsonConvert.DeserializeObject<Stations>(json);
            var Stations = stations.data;
            return Stations;
        }
        private static async Task<ProgramDetails> GetProgram(Station station)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, string.Format(PROGRAM_URL, station.code));
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var program = JsonConvert.DeserializeObject<ProgramData>(json);
            return program.data;
        }
        private static async Task<ProgramDetails> GetProgram(Station station, EpgShow show)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = new HttpRequestMessage(HttpMethod.Get, string.Format(REPLAY_URL, station.code, show.record_id));
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
        private static async Task<Epg> GetEpg(Station station, DateTime? date = null)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage m = date == null ? new HttpRequestMessage(HttpMethod.Get, string.Format(EPG_URL, station.code)) : new HttpRequestMessage(HttpMethod.Get, string.Format(EPG_DATE_URL, station.code, date.Value.ToString("yyyy-MM-dd")));
            var r = client.Send(m);
            string json = await r.Content.ReadAsStringAsync();
            var epg = JsonConvert.DeserializeObject<Epg>(json);
            return epg;
        }

        public static async Task<EpgShow> GetShow(Epg epg)
        {
            var currentDate = UnixTimeStampToDateTime(epg.data.First().date_start);
            var previousDate = currentDate.AddDays(-1);
            var nextDate = currentDate.AddDays(1);
            var ordered = epg.data.Where(x=> x.date_start < now && x.duration > 0).OrderBy(x => x.date_start);//.Select(x=> Tuple.Create(x.record_id, x.title, UnixTimeStampToDateTime(x.date_start).ToString()));
            var show = AnsiConsole.Prompt(
                new SelectionPrompt<EpgShow>()
                .PageSize(20)
                .Title("Choose show")
                .AddChoices(new EpgShow{id="previousDate", title = "[white on red]<< Previous day[/]", akpa_attributes = new string[]{}, date_start = DateTimeToUnixTimeStamp(previousDate)})
                .AddChoices(
                    ordered
                )
                .AddChoices(new EpgShow{id="nextDate", title = "[white on green]>> Next day[/]", akpa_attributes = new string[]{}, date_start = DateTimeToUnixTimeStamp(nextDate)})
                .UseConverter(x=>{
                    var disabled = x.akpa_attributes.Contains("catchUpDisabled");
                    var color = disabled ? "red": "default";
                    return $"[gray]{UnixTimeStampToDateTime(x.date_start).ToString()}[/] | [{color}]{x.title}[/]";
                    })
                .HighlightStyle(Style.WithBackground(HIGHLIGHT_CLOR))
            );
            if (show.id.Equals("previousDate"))
            {
                var newEpg = await GetEpg(epg.data.First().station, previousDate);
                show = await GetShow(newEpg);//recursion!
            }
            if (show.id.Equals("nextDate"))
            {
                var newEpg = await GetEpg(epg.data.First().station, nextDate);
                show = await GetShow(newEpg);//recursion!
            }
            return show;
        }
        public static async Task<Station> GetChannel(IEnumerable<Station> stations)
        {
            var stationList = stations;
            var channel = AnsiConsole.Prompt(
                new SelectionPrompt<Station>()
                .PageSize(20)
                .Title("Choose channel")
                .AddChoices(
                    stationList
                )
                .UseConverter(x=> $"{x.name}")
                .HighlightStyle(Style.WithBackground(HIGHLIGHT_CLOR))
            );
            return channel;
        }
        public static async Task<bool> GetLive(EpgShow show)
        {
            var live = AnsiConsole.Prompt(
                new SelectionPrompt<Tuple<string, bool>>()
                .PageSize(20)
                .Title("Play live or replay a show?")
                .AddChoices(
                    Tuple.Create($"Play channel live ([lime]{show.title}[/])", true),
                    Tuple.Create("Choose show to replay", false)
                )
                .UseConverter(x=>x.Item1)
                .HighlightStyle(Style.WithBackground(HIGHLIGHT_CLOR))
            );
            return live.Item2;
        }
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
        public static long DateTimeToUnixTimeStamp(DateTime date)
        {
            return (long)date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
        public static void Play(EpgShow show, Format format)
        {
            List<string> args = new List<string>();
            args.Add(format.url);
            args.Add("-title");
            args.Add($"\"[{show.station.name}] - {show.title}\"");
            if(Record)
            {
                args.Add("-o");
                args.Add(Regex.Replace(show.title, "[^A-Za-z0-9]" ,"") + ".mp4");
            }
            ProcessStartInfo startInfo = new ProcessStartInfo(){
                FileName = "mpv",
                Arguments = string.Join(' ', args),
                UseShellExecute = Record
            };
            Process.Start(startInfo);
        }
    }
}
