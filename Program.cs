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
                    ProgramDetails program = new ProgramDetails();
                    EpgShow show = new EpgShow();
                    //TODO get rid of this nasty ifology
                    if (channel.code.Equals(StaticTools.SearchCode))
                    {
                        var searchTerm = Prompts.AskSearch();
                        SearchScope scope = await Prompts.ChooseScope();
                        var searchResults = await SearchResults.Create(searchTerm, scope);
                        if (searchResults.data == null && searchResults.data.occurrenceitem.Length == 0)
                        {
                            Prompts.WriteLine($"No results for search [red]{searchTerm}[/]");
                            Thread.Sleep(1000);
                            continue;
                        }
                        show = await Prompts.ChooseFromSearchResult(searchResults.data.occurrenceitem);
                        program = await ProgramData.Create(show);
                    }
                    else
                    {
                        var epg = await Epg.Create(channel);
                        show = epg.data.OrderBy(x => x.date_start).Where(x => x.date_end >= now).First();
                        var playLive = await Prompts.ChooseLive(show);
                        if (playLive)
                        {
                            program = await ProgramData.Create(channel);
                        }
                        else
                        {
                            show = await Prompts.ChooseShow(epg);
                            program = await ProgramData.Create(channel, show);
                        }
                    }
                    if (program.stream_url == null)
                    {
                        Prompts.WriteLine("not playable");
                        Thread.Sleep(1000);
                        continue;
                    }
                    var token = await Tokenizer.Create(program.stream_url);
                    Format format;
                    if (options.Record)
                    {
                        format = await Prompts.ChooseFormat(token);
                    }
                    else
                    {
                        format = token.defaultFormat;
                    }
                    if (options.Record && format.mimeType.Equals("video/mp4"))
                    {
                        Download(show, format);
                    }
                    else
                    {
                        Play(show, format);
                    }
                }
            });
        }

        private static void Download(EpgShow show, Format format)
        {
            List<string> args = new List<string>();
            args.Add(format.url);
            args.Add("-O");
            var fileExtension = Path.GetExtension(format.url);
            args.Add($"\"{StaticTools.NormalizeFileName(show.title)}.{fileExtension}\"");
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "wget",
                Arguments = string.Join(' ', args),
                UseShellExecute = options.Record,
            };
            var process = Process.Start(startInfo);
            process.Exited += new EventHandler(ProcessExited);
        }

        public static void Play(EpgShow show, Format format)
        {
            List<string> args = new List<string>();
            args.Add(format.url);
            args.Add("-title");
            var stationName = string.Empty;
            if (show.station != null)
            {
                stationName = show.station.name;
            }
            args.Add($"\"[{stationName}] - {show.title}\"");
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
                UseShellExecute = options.Record,
            };
            var process = Process.Start(startInfo);
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(ProcessExited);
        }

        private static void ProcessExited(object? sender, EventArgs e)
        {
            if (sender is Process && ((Process)sender).ExitCode != 0)
            {
                var exitCode = ((Process)sender).ExitCode;
                var processName = ((Process)sender).ProcessName;
                Prompts.WriteLine($"[red]{processName} exited with code [/][blue]{exitCode}[/]");
                Thread.Sleep(1000);
            }
        }
    }
}
