#if NET10_0_OR_GREATER
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionCheckBoxHtml
{
    private static partial CheckBoxBuilder CheckBoxOf<TModel>(InputField<TModel, bool> field) => field.Html.EJS().CheckBox(field.Id);

    private static partial Rendered<FusionCheckBox, TModel> RenderedBox<TModel>(InputField<TModel, bool> field, CheckBoxBuilder box)
        => field.Render<FusionCheckBox>(box.Render());
}
#endif
