#if NET48
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionTextAreaHtml
{
    private static partial TextAreaBuilder TextAreaOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().TextArea(field.Id);

    private static partial Rendered<FusionTextArea, TModel> RenderedArea<TModel, TValue>(InputField<TModel, TValue> field, TextAreaBuilder area)
        => field.Render<FusionTextArea>(area.Render());
}
#endif
