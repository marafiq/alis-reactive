#if NET10_0_OR_GREATER
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.NativeComponents;

public static partial class NativeCheckBoxHtml
{
    private static partial Rendered<NativeCheckBox, TModel> MvcCheckBox<TModel>(InputField<TModel, bool> field, Dictionary<string, object> attributes)
        => field.Render<NativeCheckBox>(field.Html.CheckBoxFor(field.Member, attributes));
}
#endif
