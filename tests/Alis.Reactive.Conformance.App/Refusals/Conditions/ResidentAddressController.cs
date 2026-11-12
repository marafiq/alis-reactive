using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class ResidentAddressController : Controller
{
    [HttpGet(RefusalPages.ResidentAtAnAddressItsConverterWrites)]
    public IActionResult ItsConverterWrites() => View("/Refusals/Conditions/ResidentAddressItsConverterWrites.cshtml", new AddressCheck("Ada Hart", MillLane()));

    [HttpGet(RefusalPages.ResidentAtAPostcode)]
    public IActionResult Postcode() => View("/Refusals/Conditions/ResidentAddressPostcode.cshtml", new AddressCheck("Ada Hart", MillLane()));

    [HttpGet(ConditionEndpoints.ResidentsAtConvertedAddresses)]
    public IActionResult ResidentsAtConvertedAddresses() => Json(new List<ResidentAtAConvertedAddress>
    {
        new("Ada Hart", MillLane()),
        new("Ben Ode", new HomeAddress { Street = "9 Quay Road", Postcode = "EF3 4GH" }),
    });

    [HttpGet(ConditionEndpoints.ResidentsAtAddresses)]
    public IActionResult ResidentsAtAddresses() => Json(new List<ResidentAtAnAddress>
    {
        new("Ada Hart", MillLane()),
        new("Ben Ode", new HomeAddress { Street = "9 Quay Road", Postcode = "EF3 4GH" }),
    });

    private static HomeAddress MillLane() => new() { Street = "4 Mill Lane", Postcode = "AB1 2CD" };
}

public sealed record AddressCheck(string Resident, HomeAddress Address);

public sealed record ResidentAtAConvertedAddress(string Name, [property: JsonConverter(typeof(AddressAsOneText))] HomeAddress Address);

public sealed record ResidentAtAnAddress(string Name, HomeAddress Address);

public sealed class HomeAddress
{
    public string Street { get; init; } = "";

    public string Postcode { get; init; } = "";
}

public sealed class AddressAsOneText : JsonConverter<HomeAddress>
{
    private const string Between = ", ";

    public override HomeAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var written = reader.GetString() ?? "";
        var at = written.LastIndexOf(Between, StringComparison.Ordinal);
        return at < 0 ? new HomeAddress { Street = written } : new HomeAddress { Street = written[..at], Postcode = written[(at + Between.Length)..] };
    }

    public override void Write(Utf8JsonWriter writer, HomeAddress value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.Street + Between + value.Postcode);
}

public static class AddressChecks
{
    public static readonly AppEvent Check = AppEvent.Declare();
}
