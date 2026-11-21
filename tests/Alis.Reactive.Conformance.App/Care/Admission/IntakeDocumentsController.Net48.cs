using System.Web;
using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class IntakeDocumentsController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet]
    [Address(CarePages.IntakeDocuments)]
    public ActionResult OfAResident() => View("/Care/Admission/IntakeDocuments.cshtml", new IntakeDocuments());

    [HttpPost]
    [Address(IntakeDocumentsEndpoints.Saves)]
    public ActionResult Save(IntakeDocuments intake)
        => Content(string.Join("; ", new[] { intake.Name }.Concat(intake.Documents.Select(document => $"{document.File?.FileName} {document.File?.ContentLength} {document.Caption}"))), PlainText);
}

public sealed partial class IntakeDocument
{
    public HttpPostedFileBase? File { get; set; }
}
