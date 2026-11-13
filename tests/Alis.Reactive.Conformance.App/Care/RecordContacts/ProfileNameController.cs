using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ProfileNameController : Controller
{
    private const string Profile = "/Care/RecordContacts/ProfileName.cshtml";

    [HttpGet(CarePages.ProfileNameGiven)]
    public IActionResult Given() => View(Profile, new ProfileName { PreferredName = "Nell" });

    [HttpGet(CarePages.ProfileNameBlank)]
    public IActionResult Blank() => View(Profile, new ProfileName());
}

public sealed class ProfileName
{
    public string? PreferredName { get; set; }
}
