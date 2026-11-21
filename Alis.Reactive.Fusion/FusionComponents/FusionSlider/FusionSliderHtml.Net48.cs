#if NET48
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionSliderHtml
{
    private static partial SliderBuilder SliderOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().Slider(field.Id);

    private static partial Rendered<FusionSlider<TValue>, TModel> RenderedSlider<TModel, TValue>(InputField<TModel, TValue> field, SliderBuilder slider)
        => field.Render<FusionSlider<TValue>>(slider.Render());
}
#endif
