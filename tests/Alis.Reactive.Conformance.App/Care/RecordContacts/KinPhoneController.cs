using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class KinPhoneController : Controller
{
    private const string Phone = "/Care/RecordContacts/KinPhone.cshtml";

    [HttpGet(CarePages.KinHomePhone)]
    public IActionResult AtHome() => View(Phone, new KinPhone { Phone = "5551234567" });

    [HttpGet(CarePages.KinMobilePhone)]
    public IActionResult OnTheMove() => View(Phone, new KinPhone { Phone = "5559876543" });
}

public sealed class KinPhone
{
    public string? Phone { get; set; }
}
