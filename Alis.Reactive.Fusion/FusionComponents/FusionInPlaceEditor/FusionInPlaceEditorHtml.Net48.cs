#if NET48
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.InPlaceEditor;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionInPlaceEditorHtml
{
    private static partial InPlaceEditorBuilder InPlaceEditorOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().InPlaceEditor(field.Id);

    private static partial Rendered<FusionInPlaceEditor<TValue>, TModel> RenderedEditor<TModel, TValue>(InputField<TModel, TValue> field, InPlaceEditorBuilder editor)
        => field.Render<FusionInPlaceEditor<TValue>>(editor.Render());
}
#endif
