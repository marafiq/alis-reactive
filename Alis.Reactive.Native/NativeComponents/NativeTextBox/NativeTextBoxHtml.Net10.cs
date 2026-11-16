#if NET10_0_OR_GREATER
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Native.NativeComponents;

public static partial class NativeTextBoxHtml
{
    private static partial Rendered<NativeTextBox, TModel> MvcTextBox<TModel, TValue>(InputField<TModel, TValue> field, Dictionary<string, object> attributes)
        => field.Render<NativeTextBox>(field.Html.TextBoxFor(field.Member, attributes));
}
#endif
