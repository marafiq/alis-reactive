using System.Text;
using Alis.Reactive.Conformance.World;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class ResidentExportsController : Controller
{
    private const string Pdf = "application/pdf";

    private const string Csv = "text/csv";

    [HttpGet(CarePages.ResidentExports)]
    public IActionResult Exports() => View("/Care/Residents/ResidentExports.cshtml", new ResidentExportsPage("North wing"));

    [HttpGet(ResidentExportEndpoints.Resume)]
    public IActionResult Resume()
    {
        Response.Headers.ContentDisposition = "attachment; filename=\"Resume.pdf\"; filename*=UTF-8''R%C3%A9sum%C3%A9.pdf";
        return File(Encoding.UTF8.GetBytes("Ada Hart"), Pdf);
    }

    [HttpGet(ResidentExportEndpoints.Census)]
    public IActionResult Census() => File(Encoding.UTF8.GetBytes("Name,Ward\nAda Hart,East\n"), Csv);

    [HttpGet(ResidentExportEndpoints.Plan)]
    public IActionResult Plan()
    {
        Response.Headers.ContentDisposition = "attachment; filename=\"Plan.pdf\"";
        return File(Encoding.UTF8.GetBytes("Care plan"), Pdf);
    }
}

public sealed record ResidentExportsPage(string Wing);

public static class ResidentExports
{
    public static readonly AppEvent Export = AppEvent.Declare();

    public static readonly AppEvent ExportCensus = AppEvent.Declare();

    public static readonly AppEvent ExportPlan = AppEvent.Declare();
}

public static class ResidentExportEndpoints
{
    private const string Root = CarePages.Area + "residents/exports/";

    public const string Resume = Root + "resume";

    public const string Census = Root + "census.csv";

    public const string Plan = Root + "plan";
}
