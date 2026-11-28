#if NET48
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionMaskedTextBoxHtml
{
    private static partial MaskedTextBoxBuilder MaskedTextBoxOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().MaskedTextBox(field.Id);

    private static partial Rendered<FusionMaskedTextBox, TModel> RenderedBox<TModel, TValue>(InputField<TModel, TValue> field, MaskedTextBoxBuilder box)
        => field.Render<FusionMaskedTextBox>(box.Render());
}
#endif
