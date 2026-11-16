using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ContactMoveInController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet]
    [Address(CarePages.ContactMoveIn)]
    public ActionResult OfAResident() => View("/Care/Admission/ContactMoveIn.Net48.cshtml", new ContactMoveIn());

    [HttpPost]
    [Address(ContactMoveInEndpoints.MoveIns)]
    public ActionResult Save(ContactMoveIn moveIn) => Content(moveIn.Bound(), PlainText);
}
