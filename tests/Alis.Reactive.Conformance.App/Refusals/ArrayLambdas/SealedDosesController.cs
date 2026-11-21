using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class SealedDosesController : Controller
{
    [HttpGet(RefusalPages.SealedDosesComparedByTheirSeal)]
    public IActionResult ComparedByTheirSeal() => View("/Refusals/ArrayLambdas/SealedDosesComparedByTheirSeal.cshtml", new SealCheck(new DoseSeal { Tag = "S4" }));

    [HttpGet(RefusalPages.SealedDosesComparedByTheirTag)]
    public IActionResult ComparedByTheirTag() => View("/Refusals/ArrayLambdas/SealedDosesComparedByTheirTag.cshtml", new SealCheck(new DoseSeal { Tag = "S4" }));

    [HttpGet(ArrayLambdaEndpoints.SealedDoses)]
    public IActionResult Doses() => Json(new List<SealedDose>
    {
        new("Aspirin", new DoseSeal { Tag = "S4" }),
        new("Digoxin", new DoseSeal { Tag = "S7" }),
        new("Insulin", new DoseSeal { Tag = "S9" }),
    });

    [HttpGet(ArrayLambdaEndpoints.TaggedDoses)]
    public IActionResult TaggedDoses() => Json(new List<TaggedDose>
    {
        new("Aspirin", new DoseSeal { Tag = "S4" }),
        new("Digoxin", new DoseSeal { Tag = "S7" }),
        new("Insulin", new DoseSeal { Tag = "S9" }),
    });
}

public sealed record SealCheck(DoseSeal Seal);

public sealed record SealedDose(string Medicine, [property: JsonConverter(typeof(DoseSealAsItsTag))] DoseSeal Seal);

public sealed record TaggedDose(string Medicine, DoseSeal Seal);

public sealed class DoseSeal
{
    public string Tag { get; init; } = "";
}

public sealed class DoseSealAsItsTag : JsonConverter<DoseSeal>
{
    public override DoseSeal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => new() { Tag = reader.GetString() ?? "" };

    public override void Write(Utf8JsonWriter writer, DoseSeal value, JsonSerializerOptions options) => writer.WriteStringValue(value.Tag);
}
