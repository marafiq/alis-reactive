using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class CircleController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.Circle)]
    public IActionResult OfAResident() => View("/Care/Admission/Circle.cshtml", new Circle { Members = [new CircleMember(), new CircleMember()] });

    [HttpPost(CircleEndpoints.Circles)]
    public IActionResult Save([FromBody] Circle circle)
    {
        var bound = circle.Members.Select((member, position) => (member.Name, Position: position)).ToList();
        var named = bound.Where(member => member.Name is not null).Select(member => $"member {member.Position} {member.Name}");
        var unnamed = bound.Where(member => member.Name is null).Select(member => $"member {member.Position} none");
        return Content(string.Join(" and ", named.Concat(unnamed)), PlainText);
    }
}

public sealed class Circle
{
    public List<CircleMember> Members { get; set; } = [];
}

public sealed class CircleMember
{
    public string? Name { get; set; }
}

public static class CircleEndpoints
{
    public const string Circles = CarePages.Area + "admission/circle/circles";
}

public static class Circles
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
