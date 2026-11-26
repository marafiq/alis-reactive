using System.IO;
using System.Web.Compilation;
using System.Web.Mvc;

namespace Alis.Reactive.Conformance.App.World;

public sealed class ViewsController : Controller
{
    private const string Compiled = "/world/views-compiled";
    private const int CompileSeconds = 600;

    [HttpGet]
    [Address(Compiled)]
    public ActionResult Compile()
    {
        Server.ScriptTimeout = CompileSeconds;
        var root = Server.MapPath("~/");
        var views = Directory.GetFiles(root, "*.cshtml", SearchOption.AllDirectories);
        foreach (var view in views) BuildManager.GetCompiledType("~/" + view.Substring(root.Length).Replace('\\', '/'));
        return Content("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>Views compiled</title></head><body><h1>Views compiled</h1><p>"
            + views.Length + " views</p></body></html>", "text/html");
    }
}
