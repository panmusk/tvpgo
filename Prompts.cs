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
                .AddChoices(new EpgShow { id = StaticTools.PreviousDateId, title = "[white on red]<< Previous day[/]", akpa_attributes = new string[] { }, date_start = StaticTools.DateTimeToUnixTimeStamp(previousDate) })
                .AddChoices(
                    ordered
                )
                .AddChoices(new EpgShow { id = StaticTools.NextDateId, title = "[white on green]>> Next day[/]", akpa_attributes = new string[] { }, date_start = StaticTools.DateTimeToUnixTimeStamp(nextDate) })
                .UseConverter(x =>
                {
                    var disabled = x.akpa_attributes.Contains("catchUpDisabled");
                    var color = disabled ? "red" : "default";
                    return $"[gray]{StaticTools.UnixTimeStampToDateTime(x.date_start).ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern + " " + CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern)}[/] | [{color}]{x.title}[/]";
                })
                .HighlightStyle(Style.WithBackground(HIGHLIGHT_CLOR))
            );
            if (show.id.Equals(StaticTools.PreviousDateId))
            {
                var newEpg = await Epg.Create(epg.data.First().station, previousDate);
                show = await ChooseShow(newEpg);//recursion!
            }
            if (show.id.Equals(StaticTools.NextDateId))
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
                .AddChoices(new Station { name = "[lime]Search[/]", code = StaticTools.SearchCode })
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
        public static string AskSearch()
        {
            return AnsiConsole.Prompt(
                new TextPrompt<string>("Search in tvpgo:")
                    .PromptStyle("green")
                    .ValidationErrorMessage("[red]That's not a valid age[/]")
                    .Validate(query =>
                    {
                        return query.Length switch
                        {
                            < 3 => ValidationResult.Error("[red]Search term must be at least 3 characters long[/]"),
                            _ => ValidationResult.Success()
                        };
                    }));
        }
        public static async Task<EpgShow> ChooseFromSearchResult(IEnumerable<EpgShow> Occurrenceitems)
        {
            var occurrenceitem = AnsiConsole.Prompt(
                new SelectionPrompt<EpgShow>()
                .PageSize(20)
                .Title("Choose from search result")
                .AddChoices(
                    Occurrenceitems
                )
                .UseConverter(x =>
                {
                    var disabled = x.akpa_attributes.Contains("catchUpDisabled");
                    var color = disabled ? "red" : "default";
                    var title = StaticTools.Coalesce(new string[]{x.program.title, x.subtitle, x.title});
                    return $"[{color}]{title}[/]";
                })
                .HighlightStyle(Style.WithBackground(HIGHLIGHT_CLOR))
            );
            return occurrenceitem;
        }

        public static void WriteLine(string text)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Rule($"[yellow]{text}[/]").RuleStyle("grey").LeftAligned());
        }

        public static async Task<SearchScope> ChooseScope()
        {
            var scope = AnsiConsole.Prompt(
            new SelectionPrompt<SearchScope>()
            .PageSize(20)
            .Title("Choose search scope")
            .AddChoices(
                new[] { SearchScope.bestresults, SearchScope.programtv, SearchScope.vodprogrammesandepisodes, SearchScope.vodepisodes }
            )
            .UseConverter(x => x.ToString())
            .HighlightStyle(Style.WithBackground(HIGHLIGHT_CLOR))
        );
            return scope;
        }
    }
}
