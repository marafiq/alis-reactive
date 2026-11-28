#if NET10_0_OR_GREATER
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionComboBoxHtml
{
    private static partial ComboBoxBuilder ComboBoxOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().ComboBox(field.Id);

    private static partial Rendered<FusionComboBox<TValue>, TModel> RenderedBox<TModel, TValue>(InputField<TModel, TValue> field, ComboBoxBuilder box)
        => field.Render<FusionComboBox<TValue>>(box.Render());
}
#endif
