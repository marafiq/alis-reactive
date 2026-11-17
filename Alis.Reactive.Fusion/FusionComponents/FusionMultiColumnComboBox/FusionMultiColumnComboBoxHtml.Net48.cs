#if NET48
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.MultiColumnComboBox;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionMultiColumnComboBoxHtml
{
    private static partial MultiColumnComboBoxBuilder BoxOf<TModel, TMember>(InputField<TModel, TMember> field) => field.Html.EJS().MultiColumnComboBox(field.Id);

    private static partial Rendered<FusionMultiColumnComboBox<TValue>, TModel> RenderedBox<TModel, TMember, TValue>(InputField<TModel, TMember> field, MultiColumnComboBoxBuilder box)
        => field.Render<FusionMultiColumnComboBox<TValue>>(box.Render());
}
#endif
