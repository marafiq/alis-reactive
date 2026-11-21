#if NET10_0_OR_GREATER
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionDropDownListHtml
{
    private static partial DropDownListBuilder DropDownListOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().DropDownList(field.Id);

    private static partial Rendered<FusionDropDownList<TValue>, TModel> RenderedList<TModel, TValue>(InputField<TModel, TValue> field, DropDownListBuilder list)
        => field.Render<FusionDropDownList<TValue>>(list.Render());
}
#endif
