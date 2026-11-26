using System.Web.Mvc;
using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HandoverNotesController : Controller
{
    private const string PlainText = "text/plain";

    private HandoverNotesBook Book => TwinWorlds.Of(HttpContext).HandoverNotes;

    [HttpGet]
    [Address(CarePages.HandoverNotes)]
    public ActionResult ToHandOver() => View("/Care/Record/HandoverNotes.cshtml", new HandoverNotes { Notes = [null, null, null], OnRecord = Book.OnRecord });

    [HttpPut]
    [Address(CareEndpoints.NotesHandedOver)]
    public ActionResult HandOver(HandoverNotes handover)
    {
        var validation = this.Validate<HandoverNotes, HandoverNotesRules>(handover);
        if (!validation.IsValid) return validation.ToErrorResponse();

        return Content(Book.Record(handover), PlainText);
    }
}
