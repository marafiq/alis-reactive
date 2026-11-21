#if NET10_0_OR_GREATER
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionOtpInputHtml
{
    private static partial OtpInputBuilder OtpInputOf<TModel>(InputField<TModel, string?> field) => field.Html.EJS().OtpInput(field.Id);

    private static partial Rendered<FusionOtpInput, TModel> RenderedBox<TModel>(InputField<TModel, string?> field, OtpInputBuilder box)
        => field.Render<FusionOtpInput>(box.Render());
}
#endif
