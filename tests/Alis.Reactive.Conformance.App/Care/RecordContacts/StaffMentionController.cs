using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class StaffMentionController : Controller
{
    [HttpGet(CarePages.StaffMention)]
    public IActionResult OnShift()
        => View("/Care/RecordContacts/StaffMention.cshtml", new StaffMention { Staff = [new MentionedStaff("Nurse Okafor", "okafor"), new MentionedStaff("Dr. Lindqvist", "lindqvist")] });
}

public sealed class StaffMention
{
    public IReadOnlyList<MentionedStaff> Staff { get; init; } = [];
}

public sealed record MentionedStaff(string Name, string StaffCode);
