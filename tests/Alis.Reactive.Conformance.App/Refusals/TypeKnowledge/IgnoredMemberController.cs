using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Refusals;

public sealed class IgnoredMemberController : Controller
{
    [HttpGet(RefusalPages.IgnoredMember)]
    public IActionResult Ignored() => View("/Refusals/TypeKnowledge/IgnoredMember.cshtml", new WardRoster("East wing"));

    [HttpGet(RefusalPages.WrittenMember)]
    public IActionResult Written() => View("/Refusals/TypeKnowledge/WrittenMember.cshtml", new WardRoster("East wing"));

    [HttpGet(TypeKnowledgeEndpoints.WardBindings)]
    public IActionResult WardBindings() => Json(new List<WardBinding>
    {
        new("Ada") { Ignored = "Ward 4" },
        new("Ben") { Ignored = "Ward 7" },
    });
}

public sealed record WardRoster(string Wing);

public abstract record WardBindingBase
{
    public abstract string Ignored { get; init; }
}

public sealed record WardBinding(string Name) : WardBindingBase
{
    public override string Ignored { get; init; } = "";
}

public static class TypeKnowledgeEndpoints
{
    public const string WardBindings = RefusalPages.Area + "type-knowledge/written-member/bindings";
}

public static class IgnoredMembers
{
    public static readonly AppEvent Show = AppEvent.Declare();
}
