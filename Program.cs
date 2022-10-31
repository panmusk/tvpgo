using System.Diagnostics;
using CommandLine;
using tvpgo.Json;

namespace tvpgo
{
    public class Options
    {
        [Option('r', "record", Required = false, HelpText = "record the show instead of playing it")]
        public bool Record { get; set; }

        [Option('l', "lq", Required = false, HelpText = "play in lowest quality avaliable")]
        public bool LowQuality { get; set; }
    }
    internal class Program
    {
        public static Options options;
        private static async Task Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<Options>(args).WithParsedAsync(async opts =>
            {
                options = opts;
                while (true)
                {
                    var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var channels = (await Stations.Create()).data;
                    var channel = await Prompts.ChooseChannel(channels);
                    var epg = await Epg.Create(channel);
                    var currentShow = epg.data.OrderBy(x => x.date_start).Where(x => x.date_end >= now).First();
                    var playLive = await Prompts.ChooseLive(currentShow);
                    ProgramDetails program = new ProgramDetails();
                    if (playLive)
                    {
                        program = await ProgramData.Create(channel);
                    }
                    else
                    {
                        currentShow = await Prompts.ChooseShow(epg);
                        program = await ProgramData.Create(channel, currentShow);
                    }
                    var token = await Tokenizer.Create(program.stream_url);
                    var format = token.defaultFormat;
                    Play(currentShow, format);
                }
            });
        }
        public static void Play(EpgShow show, Format format)
        {
            List<string> args = new List<string>();
            args.Add(format.url);
            args.Add("-title");
            args.Add($"\"[{show.station.name}] - {show.title}\"");
            if (options.Record)
            {
                args.Add("-o");
                args.Add($"\"{StaticTools.NormalizeFileName(show.title)}.mp4\"");
            }
            if (options.LowQuality)
            {
                args.Add("--hls-bitrate=min");
            }
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "mpv",
                Arguments = string.Join(' ', args),
                UseShellExecute = options.Record
            };
            Process.Start(startInfo);
        }
    }
}
