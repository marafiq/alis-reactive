using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ContactMoveInController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.ContactMoveIn)]
    public IActionResult OfAResident() => View("/Care/Admission/ContactMoveIn.cshtml", new ContactMoveIn());

    [HttpPost(ContactMoveInEndpoints.MoveIns)]
    public IActionResult Save([FromForm] ContactMoveIn moveIn) => Content(moveIn.Bound(), PlainText);
}
