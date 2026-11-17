#if NET10_0_OR_GREATER
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionSmartTextAreaHtml
{
    private static partial Rendered<FusionSmartTextArea, TModel> Area<TModel, TValue>(InputField<TModel, TValue> field, Dictionary<string, string> attributes)
    {
        var area = new TagBuilder("textarea");
        foreach (var attribute in attributes) area.MergeAttribute(attribute.Key, attribute.Value);
        return field.Render<FusionSmartTextArea>(area);
    }
}
#endif
