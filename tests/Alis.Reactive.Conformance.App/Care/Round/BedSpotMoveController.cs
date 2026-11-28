using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedSpotMoveController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.BedSpotMoveOfAPlacedBed)]
    public IActionResult OfAPlacedBed() => View("/Care/Round/BedSpotMove.cshtml", new BedSpotMoveDesk { Resident = "Ada Hart", Bed = BedSpotMoves.PlacedBed });

    [HttpGet(CarePages.BedSpotMoveWithNoBed)]
    public IActionResult WithNoBed() => View("/Care/Round/BedSpotMove.cshtml", new BedSpotMoveDesk { Resident = "Ben Okafor", Bed = BedSpotMoves.NoBed });

    [HttpGet(BedSpotMoves.PlacedBed)]
    public IActionResult PlacedBed() => Json(new BedSpotMoveReply { Bed = new SpottedBed { Spot = new BedSpot(3, 4) } });

    [HttpGet(BedSpotMoves.NoBed)]
    public IActionResult NoBed() => Json(new BedSpotMoveReply());

    [HttpPost(BedSpotMoves.Moved)]
    public IActionResult Moved([FromBody] BedSpotMoveSent sent) => Content("Moved to row " + sent.Spot.Row + " column " + sent.Spot.Column, PlainText);
}

public sealed class BedSpotMoveDesk
{
    public string Resident { get; init; } = "";

    public string Bed { get; init; } = "";
}

public sealed class BedSpotMoveReply
{
    public SpottedBed? Bed { get; init; }
}

public sealed class SpottedBed
{
    public BedSpot Spot { get; init; }
}

public sealed class BedSpotMoveSent
{
    public BedSpot Spot { get; init; } = new(0, 0);
}

[JsonConverter(typeof(BedSpotAsList))]
public readonly record struct BedSpot(int Row, int Column);

public sealed class BedSpotAsList : JsonConverter<BedSpot>
{
    public override BedSpot Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray) throw new JsonException("A bed spot is the list of its row and its column.");
        var spot = JsonSerializer.Deserialize<int[]>(ref reader, options)!;
        return new BedSpot(spot[0], spot[1]);
    }

    public override void Write(Utf8JsonWriter writer, BedSpot value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Row);
        writer.WriteNumberValue(value.Column);
        writer.WriteEndArray();
    }
}

public static class BedSpotMoves
{
    private const string Root = CarePages.Area + "round/bed-spot-move/";

    public const string PlacedBed = Root + "bed/placed";

    public const string NoBed = Root + "bed/none";

    public const string Moved = Root + "moved";

    public static readonly AppEvent Move = AppEvent.Declare();
}
