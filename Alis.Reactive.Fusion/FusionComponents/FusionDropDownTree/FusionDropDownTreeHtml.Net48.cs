#if NET48
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionDropDownTreeHtml
{
    private static partial DropDownTreeBuilder TreeOf<TModel, TValue>(InputField<TModel, TValue> field) => field.Html.EJS().DropDownTree(field.Id);

    private static partial Rendered<FusionDropDownTree, TModel> RenderedTree<TModel, TValue>(InputField<TModel, TValue> field, DropDownTreeBuilder tree)
        => field.Render<FusionDropDownTree>(tree.Render());
}
#endif
