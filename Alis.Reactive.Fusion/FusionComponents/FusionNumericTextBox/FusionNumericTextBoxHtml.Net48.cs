#if NET48
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionNumericTextBoxHtml
{
    private static partial NumericTextBoxBuilder NumericTextBoxOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().NumericTextBox(field.Id);

    private static partial Rendered<FusionNumericTextBox<TValue>, TModel> RenderedBox<TModel, TValue>(InputField<TModel, TValue> field, NumericTextBoxBuilder box)
        => field.Render<FusionNumericTextBox<TValue>>(box.Render());
}
#endif
