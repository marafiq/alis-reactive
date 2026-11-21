#if NET10_0_OR_GREATER
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionColorPickerHtml
{
    private static partial ColorPickerBuilder ColorPickerOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().ColorPicker(field.Id);

    private static partial Rendered<FusionColorPicker, TModel> RenderedPicker<TModel, TValue>(InputField<TModel, TValue> field, ColorPickerBuilder picker)
        => field.Render<FusionColorPicker>(picker.Render());
}
#endif
