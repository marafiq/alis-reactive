using System.Text;
using System.Web;

namespace Alis.Reactive.Conformance.App;

internal static class ErrorPage
{
    private const int Unhandled = 500;

    internal static void Answer(HttpContext context)
    {
        if (context.Server.GetLastError() is not { } thrown) return;

        var shown = thrown is HttpUnhandledException { InnerException: { } inner } ? inner : thrown;
        context.Server.ClearError();
        var response = context.Response;
        response.Clear();
        response.StatusCode = shown is HttpException http ? http.GetHttpCode() : Unhandled;
        response.TrySkipIisCustomErrors = true;
        response.ContentType = "text/html";
        response.ContentEncoding = Encoding.UTF8;
        response.Write("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\"><title>An unhandled exception</title></head><body><h1>"
            + HttpUtility.HtmlEncode(shown.GetType().Name + ": " + shown.Message) + "</h1></body></html>");
    }
}
