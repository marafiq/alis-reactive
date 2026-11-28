#if NET10_0_OR_GREATER
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionTextBoxHtml
{
    private static partial TextBoxBuilder TextBoxOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().TextBox(field.Id);

    private static partial Rendered<FusionTextBox, TModel> RenderedBox<TModel, TValue>(InputField<TModel, TValue> field, TextBoxBuilder box)
        => field.Render<FusionTextBox>(box.Render());
}
#endif
