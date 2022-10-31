using System.Diagnostics;
using tvpgo.Json;

namespace tvpgo
{
    internal class Program
    {
        public static bool Record { get; set; }
        private static async Task Main(string[] args)
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (args.Length != 0 && args.Contains("-r"))
            {
                Record = true;
            }
            var channels = (await Stations.Create()).data;
            var channel = await Prompts.ChooseChannel(channels);
            var epg = await Epg.Create(channel);
            var currentShow = epg.data.OrderBy(x=>x.date_start).Where(x=> x.date_end >= now).First();
            var playLive = await Prompts.ChooseLive(currentShow);
            Format format = new Format();
            ProgramDetails program = new ProgramDetails();
            if (playLive)
            {
                program = await ProgramData.Create(channel);
            }else{
                currentShow = await Prompts.ChooseShow(epg);
                program = await ProgramData.Create(channel, currentShow);
            }
                var token = await Tokenizer.Create(program.stream_url);
                format = GetFormat(token);
                Play(currentShow, format);
        }

        private static Format GetFormat(Tokenizer token) => token.formats.First(x => x.mimeType.Equals("application/x-mpegurl"));
        public static void Play(EpgShow show, Format format)
        {
            List<string> args = new List<string>();
            args.Add(format.url);
            args.Add("-title");
            args.Add($"\"[{show.station.name}] - {show.title}\"");
            if(Record)
            {
                args.Add("-o");
                args.Add($"\"{StaticTools.NormalizeFileName(show.title)}.mp4\"");
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
