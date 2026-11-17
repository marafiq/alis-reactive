#if NET48
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionSmartTextAreaHtml
{
    private static partial Rendered<FusionSmartTextArea, TModel> Area<TModel, TValue>(InputField<TModel, TValue> field, Dictionary<string, string> attributes)
    {
        var area = new TagBuilder("textarea");
        foreach (var attribute in attributes) area.MergeAttribute(attribute.Key, attribute.Value);
        return field.Render<FusionSmartTextArea>(new HtmlString(area.ToString(TagRenderMode.Normal)));
    }
}
#endif
