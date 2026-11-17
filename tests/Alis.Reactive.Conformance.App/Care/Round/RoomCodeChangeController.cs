using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class RoomCodeChangeController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.RoomCodeChangeOfAnUncodedRoom)]
    public IActionResult OfAnUncodedRoom() => View("/Care/Round/RoomCodeChange.cshtml", new RoomCodeChangeDesk { Resident = "Ada Hart", Room = RoomCodeChanges.UncodedRoom });

    [HttpGet(CarePages.RoomCodeChangeWithNoRoom)]
    public IActionResult WithNoRoom() => View("/Care/Round/RoomCodeChange.cshtml", new RoomCodeChangeDesk { Resident = "Ben Okafor", Room = RoomCodeChanges.NoRoom });

    [HttpGet(RoomCodeChanges.UncodedRoom)]
    public IActionResult UncodedRoom() => Json(new RoomCodeReply { Room = new CodedRoom() });

    [HttpGet(RoomCodeChanges.NoRoom)]
    public IActionResult NoRoom() => Json(new RoomCodeReply());

    [HttpPost(RoomCodeChanges.Changed)]
    public IActionResult Changed([FromBody] RoomCodeSent sent) => Content("Room code " + (sent.Code.Text ?? "none"), PlainText);
}

public sealed class RoomCodeChangeDesk
{
    public string Resident { get; init; } = "";

    public string Room { get; init; } = "";
}

public sealed class RoomCodeReply
{
    public CodedRoom? Room { get; init; }
}

public sealed class CodedRoom
{
    public RoomCode Code { get; init; }
}

public sealed class RoomCodeSent
{
    public RoomCode Code { get; init; } = new("A1");
}

[JsonConverter(typeof(RoomCodeAsText))]
public readonly struct RoomCode(string text)
{
    public string? Text { get; } = text;
}

public sealed class RoomCodeAsText : JsonConverter<RoomCode>
{
    public override RoomCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType == JsonTokenType.Null ? default : new RoomCode(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, RoomCode value, JsonSerializerOptions options)
    {
        if (value.Text is null) writer.WriteNullValue();
        else writer.WriteStringValue(value.Text);
    }
}

public static class RoomCodeChanges
{
    private const string Root = CarePages.Area + "round/room-code-change/";

    public const string UncodedRoom = Root + "room/uncoded";

    public const string NoRoom = Root + "room/none";

    public const string Changed = Root + "changed";

    public static readonly AppEvent Change = AppEvent.Declare();
}
