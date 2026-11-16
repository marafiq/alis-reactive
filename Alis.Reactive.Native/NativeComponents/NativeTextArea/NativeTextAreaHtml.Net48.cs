#if NET48
using System.Collections.Generic;
using System.Web.Mvc.Html;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

public static partial class NativeTextAreaHtml
{
    private static partial Rendered<NativeTextArea, TModel> MvcTextArea<TModel, TValue>(InputField<TModel, TValue> field, Dictionary<string, object> attributes)
        => field.Render<NativeTextArea>(field.Html.TextAreaFor(field.Member, attributes));
}
#endif
