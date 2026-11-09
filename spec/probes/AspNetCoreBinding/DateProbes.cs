using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;

static class DateProbes
{
    static readonly (string Member, string Text, string Bound)[] Forms =
    [
        ("Wall", "2026-10-27", "2026-10-27T00:00:00.0000000"),
        ("WallMinute", "2026-10-27T08:30", "2026-10-27T08:30:00.0000000"),
        ("WallSecond", "2026-10-27T08:30:15", "2026-10-27T08:30:15.0000000"),
        ("WallFraction", "2026-10-27T08:30:15.1234567", "2026-10-27T08:30:15.1234567"),
        ("SeenBehind", "2026-10-27T08:30:15.1234567-05:00", "2026-10-27T13:30:15.1234567Z"),
        ("SeenZero", "2026-10-27T08:30Z", "2026-10-27T08:30:00.0000000Z"),
        ("SeenAhead", "2026-10-27T08:30:15+05:30", "2026-10-27T03:00:15.0000000Z"),
        ("Day", "2026-10-27", "2026-10-27"),
        ("StartsMinute", "08:30", "08:30:00.0000000"),
        ("StartsSecond", "08:30:15", "08:30:15.0000000"),
        ("StartsFraction", "08:30:15.1234567", "08:30:15.1234567"),
        ("LengthMinute", "08:30", "08:30:00"),
        ("LengthDays", "1.02:00:00", "1.02:00:00"),
        ("LengthNegative", "-00:00:00.5", "-00:00:00.5000000"),
        ("LengthMost", "10675199.02:48:05.4775807", "10675199.02:48:05.4775807"),
        ("LengthFraction", "08:30:15.1234567", "08:30:15.1234567"),
    ];

    static readonly string[] DayAlone = ["Wall", "Day"];

    static readonly string[] QueryMembers = ["Wall", "Seen", "Day", "Starts", "Length"];

    public static async Task Run(Uri baseAddress)
    {
        using var client = new HttpClient { BaseAddress = baseAddress };
        Console.WriteLine("core-date-compares-ticks: " + Compares());
        Console.WriteLine("core-stj-date-time-forms: " + Written());
        Console.WriteLine("core-query-date-forms: " + await QueryForms(client));
        Console.WriteLine("core-form-date-forms-every-culture: " + await EveryCulture(client));
        Console.WriteLine("core-form-day-calendar: " + await DayCalendar(client));
        Console.WriteLine("core-bound-refused-values: " + await Refused(client));
        Console.WriteLine("core-bound-offset-into-datetime: " + await OffsetIntoDateTime(client));
        Console.WriteLine("core-bound-lenient-forms: " + await Lenient(client));
        Console.WriteLine("core-implicit-instant-server-clock: " + Implicit());
        Console.WriteLine("core-stj-own-date-converters: " + OwnConverters());
    }

    static string Compares()
    {
        var wall = new DateTime(2026, 11, 9, 8, 30, 0);
        var west = new DateTimeOffset(2026, 11, 9, 10, 0, 0, TimeSpan.FromHours(-8));
        var zero = new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero);
        var first = new DateOnly(2026, 10, 27);
        var second = new DateOnly(2026, 10, 28);
        DateTime? none = null;
        return "a Utc and a Local DateTime of one wall time are == " + (DateTime.SpecifyKind(wall, DateTimeKind.Utc) == DateTime.SpecifyKind(wall, DateTimeKind.Local))
            + "; one tick later is > " + (wall.AddTicks(1) > wall)
            + "; 2026-11-09T10:00:00-08:00 == 2026-11-09T18:00:00+00:00 " + (west == zero) + ", EqualsExact " + west.EqualsExact(zero)
            + "; a DateOnly orders as its DayNumber " + ((first < second) == (first.DayNumber < second.DayNumber) && first.DayNumber + 1 == second.DayNumber)
            + "; a TimeOnly one tick later is > " + (new TimeOnly(8, 30).Add(TimeSpan.FromTicks(1)) > new TimeOnly(8, 30))
            + "; TimeSpan 1.02:00:00 > 23:00:00 " + (TimeSpan.Parse("1.02:00:00", CultureInfo.InvariantCulture) > TimeSpan.FromHours(23))
            + "; -00:00:00.5 < 0 " + (TimeSpan.FromMilliseconds(-500) < TimeSpan.Zero)
            + "; a null DateTime? == null " + (none == null) + ", < a value " + (none < wall) + ", > a value " + (none > wall) + ", != a value " + (none != wall);
    }

    static string Written()
    {
        var web = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        string Text<T>(T value) => JsonSerializer.Serialize(value, web);
        var utc = DateTime.SpecifyKind(new DateTime(2026, 11, 9, 8, 30, 0), DateTimeKind.Utc);
        var readBack = JsonSerializer.Deserialize<DateTime>(Text(utc), web);
        return "DateTimeOffset at offset zero " + Text(new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero))
            + "; DateTime with half a second " + Text(new DateTime(2026, 11, 9, 8, 30, 0).AddMilliseconds(500))
            + ", with one tick " + Text(new DateTime(2026, 11, 9, 8, 30, 0).AddTicks(1))
            + ", MinValue " + Text(DateTime.MinValue) + ", MaxValue " + Text(DateTime.MaxValue)
            + "; a Utc DateTime written " + Text(utc) + " reads back with the Ticks of its wall time " + (readBack.Ticks == utc.Ticks)
            + "; TimeOnly with one tick " + Text(new TimeOnly(8, 30).Add(TimeSpan.FromTicks(1)))
            + "; DateOnly MinValue " + Text(DateOnly.MinValue)
            + "; TimeSpan " + Text(TimeSpan.Parse("1.02:00:00", CultureInfo.InvariantCulture)) + ", " + Text(TimeSpan.FromMilliseconds(-500))
            + ", MaxValue " + Text(TimeSpan.MaxValue) + ", MinValue " + Text(TimeSpan.MinValue);
    }

    static async Task<string> QueryForms(HttpClient client)
    {
        var lines = new List<string>();
        foreach (var (member, text, bound) in Forms)
        {
            var key = QueryMembers.First(name => member.StartsWith(name, StringComparison.Ordinal));
            var answer = await client.GetStringAsync("/k/q?" + key + "=" + Uri.EscapeDataString(text));
            lines.Add(key + " " + text + (answer == key + "=" + bound ? " binds its fields" : " binds " + answer));
        }
        return string.Join("; ", lines);
    }

    static async Task<string> EveryCulture(HttpClient client)
    {
        var body = string.Join("&", Forms.Select(form => form.Member + "=" + Uri.EscapeDataString(form.Text)));
        var unswitched = new List<string>();
        var gregorianDiffers = new List<string>();
        var otherDiffers = new HashSet<string>();
        var others = 0;
        foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures).Where(culture => culture.Name.Length > 0))
        {
            var answer = await Post(client, "/k/f?culture=" + Uri.EscapeDataString(culture.Name) + "&ui-culture=de-DE", body);
            var parts = answer.Split('|');
            if (parts[0] != culture.Name)
            {
                unswitched.Add(culture.Name);
                continue;
            }
            var differs = Forms.Where(form => !parts.Contains(form.Member + "=" + form.Bound)).Select(form => form.Member).ToList();
            if (culture.Calendar is GregorianCalendar)
            {
                gregorianDiffers.AddRange(differs.Select(member => culture.Name + " " + member));
            }
            else
            {
                others++;
                otherDiffers.UnionWith(differs);
            }
        }
        return "every request bound under its own culture " + (unswitched.Count == 0)
            + "; under every culture whose calendar is Gregorian, each form binds its fields " + (gregorianDiffers.Count == 0)
            + "; under every other culture, each form but the day alone binds its fields " + (others > 0 && otherDiffers.SetEquals(DayAlone))
            + ", and the day alone binds otherwise under some " + (otherDiffers.Count > 0);
    }

    static async Task<string> DayCalendar(HttpClient client)
    {
        var alone = await Post(client, "/k/f?culture=th-TH&ui-culture=de-DE", "Wall=2026-10-27&Day=2026-10-27&WallMinute=2026-10-27T00%3A00&WallSecond=2026-10-27T00%3A00%3A00");
        var dated = await Post(client, "/k/f?culture=th-TH&ui-culture=de-DE", "Day=2026-10-27T00%3A00%3A00");
        return "under " + alone.Replace("|", ", ") + "; Day 2026-10-27T00:00:00 binds " + dated.Split('|')[1];
    }

    static async Task<string> Refused(HttpClient client)
    {
        var cases = new[]
        {
            ("Starts", "24:00"), ("Starts", "08:60"), ("Starts", "08:30:60"), ("Day", "2026-02-30"), ("Wall", "2026-02-30T08:30"),
            ("Wall", "2026-10-27T24:00"), ("Seen", "2026-10-27T08:30+15:00"), ("Seen", "0001-01-01T00:00+01:00"),
            ("Length", "24:00:00"), ("Length", "08:60:00"), ("Length", "10675199.02:48:05.4775808"),
            ("Starts", "08:30:15.12345678"), ("Wall", "2026-10-27T08:30:15.12345678"),
        };
        var lines = new List<string>();
        foreach (var (member, text) in cases)
        {
            lines.Add(member + " " + text + " binds " + await client.GetStringAsync("/k/q?" + member + "=" + Uri.EscapeDataString(text)));
        }
        return string.Join("; ", lines);
    }

    static async Task<string> OffsetIntoDateTime(HttpClient client)
    {
        var utcWall = "Wall=2026-10-27T10:00:00.0000000";
        var serverWall = TimeZoneInfo.ConvertTime(new DateTimeOffset(2026, 10, 27, 10, 0, 0, TimeSpan.Zero), TimeZoneInfo.Local).DateTime;
        var queryZulu = await client.GetStringAsync("/k/q?Wall=" + Uri.EscapeDataString("2026-10-27T10:00:00Z"));
        var queryBehind = await client.GetStringAsync("/k/q?Wall=" + Uri.EscapeDataString("2026-10-27T05:00:00-05:00"));
        var formZulu = await Post(client, "/k/f?culture=de-DE&ui-culture=de-DE", "Wall=" + Uri.EscapeDataString("2026-10-27T10:00:00Z"));
        var formBehind = await Post(client, "/k/f?culture=de-DE&ui-culture=de-DE", "Wall=" + Uri.EscapeDataString("2026-10-27T05:00:00-05:00"));
        var jsonZulu = JsonSerializer.Deserialize<DateTime>("\"2026-10-27T10:00:00Z\"");
        var jsonBehind = JsonSerializer.Deserialize<DateTime>("\"2026-10-27T05:00:00-05:00\"");
        var parsedZulu = DateTime.Parse("2026-10-27T10:00:00Z", CultureInfo.InvariantCulture);
        var parsedBehind = DateTime.Parse("2026-10-27T05:00:00-05:00", CultureInfo.InvariantCulture);
        var wall = new DateTime(2026, 10, 27, 10, 0, 0);
        var offsetless = await client.GetStringAsync("/k/q?Seen=" + Uri.EscapeDataString("2026-10-27T10:00:00"));
        var serverOffset = new DateTimeOffset(wall, TimeZoneInfo.Local.GetUtcOffset(wall)).UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff", CultureInfo.InvariantCulture) + "Z";
        return "a DateTime in a query or form data, 2026-10-27T10:00:00Z or 2026-10-27T05:00:00-05:00, binds that instant's UTC wall time "
            + (queryZulu == utcWall && queryBehind == utcWall && formZulu.EndsWith("|" + utcWall, StringComparison.Ordinal) && formBehind.EndsWith("|" + utcWall, StringComparison.Ordinal))
            + "; System.Text.Json reads the Z text as that UTC wall time, kind Utc, " + (jsonZulu.Kind == DateTimeKind.Utc && jsonZulu == new DateTime(2026, 10, 27, 10, 0, 0))
            + ", and the -05:00 text as the server clock's wall time, kind Local, " + (jsonBehind.Kind == DateTimeKind.Local && jsonBehind == serverWall)
            + "; DateTime.Parse with no style, which MVC 5's converter calls, reads each as the server clock's wall time, kind Local, " + (parsedZulu.Kind == DateTimeKind.Local && parsedZulu == serverWall && parsedBehind == serverWall)
            + "; a DateTimeOffset with no offset, 2026-10-27T10:00:00, binds at the server clock's offset for that wall time " + (offsetless == "Seen=" + serverOffset);
    }

    static async Task<string> Lenient(HttpClient client)
    {
        var cases = new[]
        {
            ("Starts", "7:00"), ("Starts", "7:00 PM"), ("Starts", "8:30:15"), ("Day", "10/27/2026"), ("Wall", "10/27/2026"),
            ("Wall", "2026-10-27 08:30"), ("Length", "5"), ("Length", "8:30"), ("Seen", "10/27/2026 08:30 -05:00"),
        };
        var lines = new List<string>();
        foreach (var (member, text) in cases)
        {
            lines.Add(member + " " + text + " binds " + await client.GetStringAsync("/k/q?" + member + "=" + Uri.EscapeDataString(text)));
        }
        return string.Join("; ", lines);
    }

    static string Implicit()
    {
        var wall = new DateTime(2026, 11, 9, 8, 30, 0);
        DateTimeOffset unspecified = wall;
        DateTimeOffset utc = DateTime.SpecifyKind(wall, DateTimeKind.Utc);
        DateTimeOffset gap = new DateTime(2026, 3, 8, 2, 30, 0);
        return "a DateTime converts to a DateTimeOffset at the server clock's offset for its wall time " + (unspecified.Offset == TimeZoneInfo.Local.GetUtcOffset(wall) && unspecified.DateTime == wall)
            + "; a Utc one at +00:00 " + (utc.Offset == TimeSpan.Zero && utc.DateTime == wall)
            + "; the conversion keeps the wall fields and reads no zone of the view " + (gap.DateTime == new DateTime(2026, 3, 8, 2, 30, 0));
    }

    static string OwnConverters()
    {
        var app = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        var converted = new JsonSerializerOptions(JsonSerializerDefaults.Web) { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };
        converted.Converters.Add(new ProbeAppDateConverter());
        return "under the app's options the contract's converter is System.Text.Json's own: DateTime " + ReferenceEquals(app.GetTypeInfo(typeof(DateTime)).Converter, JsonMetadataServices.DateTimeConverter)
            + ", DateTimeOffset " + ReferenceEquals(app.GetTypeInfo(typeof(DateTimeOffset)).Converter, JsonMetadataServices.DateTimeOffsetConverter)
            + ", DateOnly " + ReferenceEquals(app.GetTypeInfo(typeof(DateOnly)).Converter, JsonMetadataServices.DateOnlyConverter)
            + ", TimeOnly " + ReferenceEquals(app.GetTypeInfo(typeof(TimeOnly)).Converter, JsonMetadataServices.TimeOnlyConverter)
            + ", TimeSpan " + ReferenceEquals(app.GetTypeInfo(typeof(TimeSpan)).Converter, JsonMetadataServices.TimeSpanConverter)
            + "; with a DateTime converter the app adds, DateTime " + ReferenceEquals(converted.GetTypeInfo(typeof(DateTime)).Converter, JsonMetadataServices.DateTimeConverter)
            + " and the app's " + (converted.GetTypeInfo(typeof(DateTime)).Converter is ProbeAppDateConverter);
    }

    static string Wall(DateTime value) => value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff", CultureInfo.InvariantCulture);

    static async Task<string> Post(HttpClient client, string path, string body)
    {
        using var response = await client.PostAsync(path, new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"));
        return await response.Content.ReadAsStringAsync();
    }
}

public sealed class ProbeAppDateConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.GetDateTime();

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
}

public sealed class ProbeDateQuery
{
    public DateTime? Wall { get; set; }

    public DateTimeOffset? Seen { get; set; }

    public DateOnly? Day { get; set; }

    public TimeOnly? Starts { get; set; }

    public TimeSpan? Length { get; set; }
}

public sealed class ProbeDateForm
{
    public DateTime? Wall { get; set; }

    public DateTime? WallMinute { get; set; }

    public DateTime? WallSecond { get; set; }

    public DateTime? WallFraction { get; set; }

    public DateTimeOffset? SeenBehind { get; set; }

    public DateTimeOffset? SeenZero { get; set; }

    public DateTimeOffset? SeenAhead { get; set; }

    public DateOnly? Day { get; set; }

    public TimeOnly? StartsMinute { get; set; }

    public TimeOnly? StartsSecond { get; set; }

    public TimeOnly? StartsFraction { get; set; }

    public TimeSpan? LengthMinute { get; set; }

    public TimeSpan? LengthDays { get; set; }

    public TimeSpan? LengthNegative { get; set; }

    public TimeSpan? LengthMost { get; set; }

    public TimeSpan? LengthFraction { get; set; }
}

[Route("k")]
public sealed class DateProbeController : ControllerBase
{
    [HttpGet("q")]
    public string Query([FromQuery] ProbeDateQuery query)
        => ModelState.IsValid
            ? string.Join("|", new[] { Bound(nameof(query.Wall), query.Wall), Bound(nameof(query.Seen), query.Seen), Bound(nameof(query.Day), query.Day), Bound(nameof(query.Starts), query.Starts), Bound(nameof(query.Length), query.Length) }.Where(part => part.Length > 0))
            : "refused";

    [HttpPost("f")]
    public string Form([FromForm] ProbeDateForm form)
        => CultureInfo.CurrentCulture.Name + "|" + string.Join("|", new[]
        {
            Bound(nameof(form.Wall), form.Wall), Bound(nameof(form.WallMinute), form.WallMinute), Bound(nameof(form.WallSecond), form.WallSecond),
            Bound(nameof(form.WallFraction), form.WallFraction), Bound(nameof(form.SeenBehind), form.SeenBehind), Bound(nameof(form.SeenZero), form.SeenZero),
            Bound(nameof(form.SeenAhead), form.SeenAhead), Bound(nameof(form.Day), form.Day), Bound(nameof(form.StartsMinute), form.StartsMinute),
            Bound(nameof(form.StartsSecond), form.StartsSecond), Bound(nameof(form.StartsFraction), form.StartsFraction), Bound(nameof(form.LengthMinute), form.LengthMinute),
            Bound(nameof(form.LengthDays), form.LengthDays), Bound(nameof(form.LengthNegative), form.LengthNegative), Bound(nameof(form.LengthMost), form.LengthMost),
            Bound(nameof(form.LengthFraction), form.LengthFraction),
        }.Where(part => part.Length > 0)) + Refusals();

    string Refusals()
        => ModelState.IsValid ? "" : "|refused " + string.Join(",", ModelState.Where(entry => entry.Value!.Errors.Count > 0).Select(entry => entry.Key));

    static string Bound(string member, DateTime? value)
        => value is { } wall ? member + "=" + wall.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff", CultureInfo.InvariantCulture) : "";

    static string Bound(string member, DateTimeOffset? value)
        => value is { } seen ? member + "=" + seen.UtcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff", CultureInfo.InvariantCulture) + "Z" : "";

    static string Bound(string member, DateOnly? value)
        => value is { } day ? member + "=" + day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "";

    static string Bound(string member, TimeOnly? value)
        => value is { } starts ? member + "=" + starts.ToString("HH:mm:ss.fffffff", CultureInfo.InvariantCulture) : "";

    static string Bound(string member, TimeSpan? value)
        => value is { } length ? member + "=" + length.ToString("c", CultureInfo.InvariantCulture) : "";
}
