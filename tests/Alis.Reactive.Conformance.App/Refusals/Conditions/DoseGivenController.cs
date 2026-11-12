using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class DoseGivenController : Controller
{
    private static readonly DateTime TheRound = new(2026, 11, 2, 8, 0, 0);

    [HttpGet(RefusalPages.DoseGivenAtAConvertedTime)]
    public IActionResult AtAConvertedTime() => View("/Refusals/Conditions/DoseGivenAtAConvertedTime.cshtml", new GivenCheck("Aspirin", TheRound));

    [HttpGet(RefusalPages.DoseGivenAtATime)]
    public IActionResult AtATime() => View("/Refusals/Conditions/DoseGivenAtATime.cshtml", new GivenCheck("Aspirin", TheRound));

    [HttpGet(ConditionEndpoints.DosesGivenAtConvertedTimes)]
    public IActionResult DosesGivenAtConvertedTimes() => Json(new List<DoseGivenAtAConvertedTime>
    {
        new("Aspirin", TheRound),
        new("Digoxin", TheRound.AddMinutes(30)),
    });

    [HttpGet(ConditionEndpoints.DosesGivenAtTimes)]
    public IActionResult DosesGivenAtTimes() => Json(new List<DoseGivenAtATime>
    {
        new("Aspirin", TheRound),
        new("Digoxin", TheRound.AddMinutes(30)),
    });
}

public sealed record GivenCheck(string Medicine, DateTime Round);

public sealed record DoseGivenAtAConvertedTime(string Medicine, [property: JsonConverter(typeof(GivenAsDayAndTime))] DateTime Given);

public sealed record DoseGivenAtATime(string Medicine, DateTime Given);

public sealed class GivenAsDayAndTime : JsonConverter<DateTime>
{
    private const string Written = "dd.MM.yyyy HH:mm";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTime.ParseExact(reader.GetString() ?? "", Written, CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Written, CultureInfo.InvariantCulture));
}

public static class GivenChecks
{
    public static readonly AppEvent Check = AppEvent.Declare();
}
