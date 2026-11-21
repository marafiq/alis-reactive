#if NET10_0_OR_GREATER
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionMultiSelectHtml
{
    private static partial MultiSelectBuilder MultiSelectOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().MultiSelect(field.Id);

    private static partial Rendered<FusionMultiSelect<TValue>, TModel> RenderedBox<TModel, TValue>(InputField<TModel, TValue> field, MultiSelectBuilder box)
        => field.Render<FusionMultiSelect<TValue>>(box.Render());
}
#endif
