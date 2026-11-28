using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class StartedVisitsController : Controller
{
    private static readonly DateTime TheStart = new(2026, 11, 2, 9, 0, 0);

    [HttpGet(RefusalPages.VisitsAtAConvertedStart)]
    public IActionResult AtAConvertedStart() => View("/Refusals/ArrayLambdas/VisitsAtAConvertedStart.cshtml", new VisitingHour(TheStart));

    [HttpGet(RefusalPages.VisitsAtAStart)]
    public IActionResult AtAStart() => View("/Refusals/ArrayLambdas/VisitsAtAStart.cshtml", new VisitingHour(TheStart));

    [HttpGet(ArrayLambdaEndpoints.ConvertedVisits)]
    public IActionResult ConvertedVisits() => Json(new List<ConvertedVisit>
    {
        new("Ada Hart", TheStart),
        new("Ben Ode", TheStart),
        new("Cy Roe", TheStart.AddTicks(1)),
        new("Di Lam", TheStart.AddMinutes(90)),
    });

    [HttpGet(ArrayLambdaEndpoints.Visits)]
    public IActionResult Visits() => Json(new List<StartedVisit>
    {
        new("Ada Hart", TheStart),
        new("Ben Ode", TheStart),
        new("Cy Roe", TheStart.AddTicks(1)),
        new("Di Lam", TheStart.AddMinutes(90)),
    });
}

public sealed record VisitingHour(DateTime Start);

public sealed record ConvertedVisit(string Visitor, [property: JsonConverter(typeof(DayAndTime))] DateTime At);

public sealed record StartedVisit(string Visitor, DateTime At);

public sealed class DayAndTime : JsonConverter<DateTime>
{
    private const string Written = "dd.MM.yyyy HH:mm";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateTime.ParseExact(reader.GetString() ?? "", Written, CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString(Written, CultureInfo.InvariantCulture));
}
