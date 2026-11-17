using System.Runtime.CompilerServices;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.Realtime;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class ActivityBoardController : Controller
{
    [HttpGet(RefusalPages.ActivityBoard)]
    public IActionResult Board() => View("/Refusals/Realtime/ActivityBoard.cshtml", new ActivityBoard("Garden lounge"));

    [HttpGet(RefusalPages.ActivityNews)]
    public IResult News(CancellationToken stopped) => ServerEvents.Write(HttpContext, FirstNews(stopped));

    private static async IAsyncEnumerable<ServerMessage> FirstNews([EnumeratorCancellation] CancellationToken stopped)
    {
        yield return new ActivityNews().News.Of(new Activity { Name = "Bingo at three" });
        await Task.Delay(Timeout.Infinite, stopped);
    }
}

public sealed record ActivityBoard(string Lounge);

public sealed class ActivityNews : ServerPushStream
{
    public static readonly ServerPushSource<ActivityNews> Live = At<ActivityNews>(RefusalPages.ActivityNews);

    public ServerEvent<Activity> News => Message<Activity>();
}

public sealed class Activity
{
    public string Name { get; set; } = "";
}
