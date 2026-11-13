using System;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a rating bound to a member of its model.
/// </summary>
public static class FusionRatingHtml
{
    /// <summary>Renders a rating for the field's member, starting at the member's score.</summary>
    /// <param name="field">The input field the rating is rendered for.</param>
    /// <returns>The rendered rating, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionRating, TModel> FusionRating<TModel>(this InputField<TModel, double> field)
        => throw new NotImplementedException();

    /// <summary>Renders a rating for the field's member, starting at the member's score, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the rating is rendered for.</param>
    /// <param name="build">What the view sets on the rating.</param>
    /// <returns>The rendered rating, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionRating, TModel> FusionRating<TModel>(this InputField<TModel, double> field, Action<RatingBuilder> build)
        => throw new NotImplementedException();
}
