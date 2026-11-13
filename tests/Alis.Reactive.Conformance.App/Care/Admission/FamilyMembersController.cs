using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class FamilyMembersController : Controller
{
    private const string NameTheSecondMember = "Name the second member.";

    [HttpGet(CarePages.FamilyMembers)]
    public IActionResult OfAResident() => View("/Care/Admission/FamilyMembers.cshtml", Family(FamilyMembersEndpoints.SecondKeyed));

    [HttpGet(CarePages.FamilyMembersWithTheFirstKeyed)]
    public IActionResult WithTheFirstKeyed() => View("/Care/Admission/FamilyMembers.cshtml", Family(FamilyMembersEndpoints.FirstKeyed));

    [HttpPost(FamilyMembersEndpoints.SecondKeyed)]
    public IActionResult RefuseTheSecond()
    {
        ModelState.AddModelError("Members[1].Name", NameTheSecondMember);
        return ValidationProblem(ModelState);
    }

    [HttpPost(FamilyMembersEndpoints.FirstKeyed)]
    public IActionResult RefuseTheFirst()
    {
        ModelState.AddModelError("Members[0].Name", NameTheSecondMember);
        return ValidationProblem(ModelState);
    }

    private static FamilyMembers Family(string saves) => new() { Members = [new() { Name = "Ada" }, new() { Name = "Ben" }], Saves = saves };
}

public sealed class FamilyMembers
{
    public List<FamilyMember> Members { get; set; } = [];

    public required string Saves { get; init; }
}

public sealed class FamilyMember
{
    public string? Name { get; set; }
}

public static class FamilyMembersEndpoints
{
    public const string SecondKeyed = CarePages.Area + "admission/family-members/saves";

    public const string FirstKeyed = CarePages.Area + "admission/family-members/first-keyed/saves";
}

public static class FamilyMembersButtons
{
    public static readonly AppEvent Save = AppEvent.Declare();
}
