#if NET48
using System.Collections.Generic;
using System.Web.Mvc.Html;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

public static partial class NativeTextBoxHtml
{
    private static partial Rendered<NativeTextBox, TModel> MvcTextBox<TModel, TValue>(InputField<TModel, TValue> field, string? format, Dictionary<string, object> attributes)
        => field.Render<NativeTextBox>(field.Html.TextBoxFor(field.Member, format, attributes));
}
#endif
