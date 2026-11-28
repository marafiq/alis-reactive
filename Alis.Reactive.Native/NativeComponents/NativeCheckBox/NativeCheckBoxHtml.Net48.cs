#if NET48
using System.Collections.Generic;
using System.Web.Mvc.Html;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

public static partial class NativeCheckBoxHtml
{
    private static partial Rendered<NativeCheckBox, TModel> MvcCheckBox<TModel>(InputField<TModel, bool> field, Dictionary<string, object> attributes)
        => field.Render<NativeCheckBox>(field.Html.CheckBoxFor(field.Member, attributes));
}
#endif
