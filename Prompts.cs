using System.Globalization;
using Spectre.Console;
using tvpgo.Json;

namespace tvpgo
{
    public static class Prompts
    {
        public static readonly Color HIGHLIGHT_CLOR = Color.Blue;
        public static readonly double now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static async Task<EpgShow> ChooseShow(Epg epg)
        {
            var currentDate = StaticTools.UnixTimeStampToDateTime(epg.data.First().date_start);
            var previousDate = currentDate.AddDays(-1);
            var nextDate = currentDate.AddDays(1);
            var ordered = epg.data.Where(x => x.date_start < now && x.duration > 0).OrderBy(x => x.date_start);
            var show = AnsiConsole.Prompt(
                new SelectionPrompt<EpgShow>()
                .PageSize(20)
                .Title($"[lime]{epg.data.First().station.name}[/] | Choose show")
                .AddChoices(new EpgShow { id = "previousDate", title = "[white on red]<< Previous day[/]", akpa_attributes = new string[] { }, date_start = StaticTools.DateTimeToUnixTimeStamp(previousDate) })
                .AddChoices(
                    ordered
                )
                .AddChoices(new EpgShow { id = "nextDate", title = "[white on green]>> Next day[/]", akpa_attributes = new string[] { }, date_start = StaticTools.DateTimeToUnixTimeStamp(nextDate) })
                .UseConverter(x =>
                {
                    var disabled = x.akpa_attributes.Contains("catchUpDisabled");
                    var color = disabled ? "red" : "default";
                    return $"[gray]{StaticTools.UnixTimeStampToDateTime(x.date_start).ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern)}[/] | [{color}]{x.title}[/]";
                })
                .HighlightStyle(Style.WithBackground(HIGHLIGHT_CLOR))
            );
            if (show.id.Equals("previousDate"))
            {
                var newEpg = await Epg.Create(epg.data.First().station, previousDate);
                show = await ChooseShow(newEpg);//recursion!
            }
            if (show.id.Equals("nextDate"))
            {
                var newEpg = await Epg.Create(epg.data.First().station, nextDate);
                show = await ChooseShow(newEpg);//recursion!
            }
            return show;
        }
        public static async Task<Station> ChooseChannel(IEnumerable<Station> stations)
        {
            var stationList = stations;
            var channel = AnsiConsole.Prompt(
                new SelectionPrompt<Station>()
                .PageSize(20)
                .Title("Choose channel")
                .AddChoices(
                    stationList
                )
                .UseConverter(x => $"{x.name}")
                .HighlightStyle(Style.WithBackground(HIGHLIGHT_CLOR))
            );
            return channel;
        }
        public static async Task<bool> ChooseLive(EpgShow show)
        {
            var live = AnsiConsole.Prompt(
                new SelectionPrompt<Tuple<string, bool>>()
                .PageSize(20)
                .Title("Play live or replay a show?")
                .AddChoices(
                    Tuple.Create($"Play channel live ([lime]{show.title}[/])", true),
                    Tuple.Create("Choose show to replay", false)
                )
                .UseConverter(x => x.Item1)
                .HighlightStyle(Style.WithBackground(HIGHLIGHT_CLOR))
            );
            return live.Item2;
        }

    }
}
