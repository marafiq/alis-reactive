#if NET48
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionSwitchHtml
{
    private static partial SwitchBuilder SwitchOf<TModel>(InputField<TModel, bool> field) => field.Html.EJS().Switch(field.Id);

    private static partial Rendered<FusionSwitch, TModel> RenderedSwitch<TModel>(InputField<TModel, bool> field, SwitchBuilder toggle)
        => field.Render<FusionSwitch>(toggle.Render());
}
#endif
