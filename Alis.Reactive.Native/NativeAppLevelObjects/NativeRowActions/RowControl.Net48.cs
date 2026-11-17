#if NET48
using System.Web;
using System.Web.Mvc;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

public sealed partial class RowControl : IHtmlString
{
    /// <summary>Returns the control's markup.</summary>
    /// <returns>The markup, encoded as HTML.</returns>
    public string ToHtmlString() => Element().ToString(TagRenderMode.Normal);

    private TagBuilder Element()
    {
        var control = new TagBuilder(_tag);
        control.MergeAttribute("id", _id);
        foreach (var attribute in _attributes) control.MergeAttribute(attribute.Key, attribute.Value);
        if (_hasCssClass) control.AddCssClass(_cssClass);
        control.SetInnerText(_text);
        return control;
    }
}
#endif
