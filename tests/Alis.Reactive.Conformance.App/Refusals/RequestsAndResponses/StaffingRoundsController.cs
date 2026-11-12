using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.App.Components;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class StaffingRoundsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(RefusalPages.RoundsByItsOwnConverter)]
    public IActionResult ByItsOwnConverter() => View("/Refusals/RequestsAndResponses/RoundsByItsOwnConverter.cshtml", new StaffingRounds("North"));

    [HttpGet(RefusalPages.RoundsByItsName)]
    public IActionResult ByItsName() => View("/Refusals/RequestsAndResponses/RoundsByItsName.cshtml", new StaffingRounds("North"));

    [HttpGet(RefusalEndpoints.StaffingRounds)]
    public IActionResult Rounds(RoundsOfStaffing query) => Content($"Rounds at {query.Choice} staffing", PlainText);
}

public sealed record StaffingRounds(string Ward);

public sealed class RoundsOfStaffing
{
    public Choice Choice { get; set; }
}

public sealed class RoundsOfCodedStaffing
{
    [JsonConverter(typeof(ChoiceAsItsCode))]
    public Choice Choice { get; set; }
}

public sealed class ChoiceAsItsCode : JsonConverter<Choice>
{
    private const string Code = "code";

    public override Choice Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var written = JsonDocument.ParseValue(ref reader);
        return (Choice)written.RootElement.GetProperty(Code).GetInt32();
    }

    public override void Write(Utf8JsonWriter writer, Choice value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(Code, (int)value);
        writer.WriteEndObject();
    }
}

public static class StaffingRoundEvents
{
    public static readonly AppEvent Find = AppEvent.Declare();
}
