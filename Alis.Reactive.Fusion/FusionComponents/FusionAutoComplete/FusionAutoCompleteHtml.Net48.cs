#if NET48
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionAutoCompleteHtml
{
    private static partial AutoCompleteBuilder AutoCompleteOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().AutoComplete(field.Id);

    private static partial Rendered<FusionAutoComplete<TValue>, TModel> RenderedBox<TModel, TValue>(InputField<TModel, TValue> field, AutoCompleteBuilder box)
        => field.Render<FusionAutoComplete<TValue>>(box.Render());
}
#endif
