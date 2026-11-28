using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class VideoCallsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.VideoCalls)]
    public IActionResult Calls() => View("/Care/Admission/VideoCalls.cshtml", new VideoCalls { Calls = [new VideoCall { Note = "Bloods" }, new VideoCall()] });

    [HttpPost(VideoCallsEndpoints.Calls)]
    public IActionResult Save([FromForm] VideoCalls videoCalls)
        => Content($"{videoCalls.Resident}: {string.Join(", ", videoCalls.Calls.Select(call => call.Link?.ToString() ?? "no link"))}", PlainText);
}

public sealed class VideoCalls
{
    public string? Resident { get; set; }

    public List<VideoCall> Calls { get; set; } = [];
}

public sealed class VideoCall
{
    public Uri? Link { get; set; }

    public string? Note { get; set; }
}

public static class VideoCallsEndpoints
{
    public const string Calls = CarePages.VideoCalls + "/calls";
}
