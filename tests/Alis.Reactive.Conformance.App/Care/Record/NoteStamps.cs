using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

public static class NoteStamps
{
    public static IHtmlContent NoteStamp(this IHtmlHelper html)
        => new HtmlString("<p id=\"note-stamp\"></p><script>document.getElementById(\"note-stamp\").textContent = \"stamped by its own script\";</script>");
}
