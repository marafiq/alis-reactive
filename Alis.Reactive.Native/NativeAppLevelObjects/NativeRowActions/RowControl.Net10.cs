#if NET10_0_OR_GREATER
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

public sealed partial class RowControl : IHtmlContent
{
    /// <summary>Writes the control's markup.</summary>
    /// <param name="writer">The writer of the view.</param>
    /// <param name="encoder">The encoder of the view.</param>
    public void WriteTo(TextWriter writer, HtmlEncoder encoder) => Element().WriteTo(writer, encoder);

    private TagBuilder Element()
    {
        var control = new TagBuilder(_tag);
        control.MergeAttribute("id", _id);
        foreach (var attribute in _attributes) control.MergeAttribute(attribute.Key, attribute.Value);
        if (_hasCssClass) control.AddCssClass(_cssClass);
        control.InnerHtml.Append(_text);
        return control;
    }
}
#endif
