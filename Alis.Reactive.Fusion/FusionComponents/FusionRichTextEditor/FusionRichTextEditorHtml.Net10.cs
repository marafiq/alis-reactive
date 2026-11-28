#if NET10_0_OR_GREATER
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.RichTextEditor;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionRichTextEditorHtml
{
    private static partial RichTextEditorBuilder RichTextEditorOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().RichTextEditor(field.Id);

    private static partial Rendered<FusionRichTextEditor, TModel> RenderedEditor<TModel, TValue>(InputField<TModel, TValue> field, RichTextEditorBuilder editor)
        => field.Render<FusionRichTextEditor>(editor.Render());
}
#endif
