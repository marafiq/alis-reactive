using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ReceptionController : Controller
{
    [HttpGet(CarePages.ReceptionWithANameSection)]
    public IActionResult WithANameSection() => View("/Care/Admission/Reception.cshtml", new Reception { Section = ReceptionEndpoints.NameSection });

    [HttpGet(CarePages.ReceptionWithAPhoneSection)]
    public IActionResult WithAPhoneSection() => View("/Care/Admission/Reception.cshtml", new Reception { Section = ReceptionEndpoints.PhoneSection });

    [HttpGet(ReceptionEndpoints.NameSection)]
    public IActionResult NameSection() => PartialView("/Care/Admission/ReceptionNameSection.cshtml", new Reception { Section = ReceptionEndpoints.NameSection });

    [HttpGet(ReceptionEndpoints.PhoneSection)]
    public IActionResult PhoneSection() => PartialView("/Care/Admission/ReceptionPhoneSection.cshtml", new Reception { Section = ReceptionEndpoints.PhoneSection });
}

public sealed class Reception
{
    public string? Name { get; set; }

    public string? Phone { get; set; }

    public required string Section { get; init; }
}

public static class ReceptionEndpoints
{
    public const string NameSection = CarePages.Area + "admission/reception/sections/name";

    public const string PhoneSection = CarePages.Area + "admission/reception/sections/phone";
}

public static class Receptions
{
    public static readonly AppEvent Load = AppEvent.Declare();
}
