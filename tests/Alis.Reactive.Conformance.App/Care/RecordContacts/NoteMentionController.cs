using System.Text.Json.Serialization;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class NoteMentionController : Controller
{
    [HttpGet(CarePages.NoteMention)]
    public IActionResult OfTheShift()
        => View("/Care/RecordContacts/NoteMention.cshtml", new NoteMention { Staff = [new OnDutyStaff("Nurse Okafor", "okafor"), new OnDutyStaff("Dr. Lindqvist", "lindqvist")] });
}

public sealed class NoteMention
{
    public IReadOnlyList<OnDutyStaff> Staff { get; init; } = [];
}

public sealed record OnDutyStaff([property: JsonPropertyName("label")] string DisplayLabel, string StaffCode);
