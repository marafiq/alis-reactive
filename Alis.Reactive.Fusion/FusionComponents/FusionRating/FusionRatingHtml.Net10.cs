#if NET10_0_OR_GREATER
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

public static partial class FusionRatingHtml
{
    private static partial RatingBuilder RatingOf<TModel>(InputField<TModel, double> field) => field.Html.EJS().Rating(field.Id);

    private static partial Rendered<FusionRating, TModel> RenderedRating<TModel>(InputField<TModel, double> field, RatingBuilder rating)
        => field.Render<FusionRating>(rating.Render());
}
#endif
