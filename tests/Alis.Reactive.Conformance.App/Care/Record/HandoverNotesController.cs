using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HandoverNotesController : Controller
{
    private const string PlainText = "text/plain";

    private HandoverNotesBook Book => HttpContext.RequestServices.GetRequiredService<CurrentWorld>().Data.Owns(HandoverNotesBook.Open);

    [HttpGet(CarePages.HandoverNotes)]
    public IActionResult ToHandOver() => View("/Care/Record/HandoverNotes.cshtml", new HandoverNotes { Notes = [null, null, null], OnRecord = Book.OnRecord });

    [HttpPut(CareEndpoints.NotesHandedOver)]
    public IActionResult HandOver([FromForm] HandoverNotes handover)
    {
        var validation = this.Validate<HandoverNotes, HandoverNotesRules>(handover);
        if (!validation.IsValid) return validation.ToErrorResponse();

        return Content(Book.Record(handover), PlainText);
    }
}

internal sealed partial class HandoverNotesBook : IOwnedByAWorld
{
    void IOwnedByAWorld.Take(Seed seed)
    {
    }
}
