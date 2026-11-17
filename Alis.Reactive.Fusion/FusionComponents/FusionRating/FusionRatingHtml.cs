using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a rating bound to a member of its model.
/// </summary>
public static partial class FusionRatingHtml
{
    private static readonly JsonSerializerOptions WrittenNumbers = new() { NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals };

    /// <summary>Renders a rating for the field's member, starting at the member's score.</summary>
    /// <param name="field">The input field the rating is rendered for.</param>
    /// <returns>The rendered rating, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionRating, TModel> FusionRating<TModel>(this InputField<TModel, double> field)
        => field.FusionRating(_ => { });

    /// <summary>Renders a rating for the field's member, starting at the member's score, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the rating is rendered for.</param>
    /// <param name="build">What the view sets on the rating.</param>
    /// <returns>The rendered rating, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionRating, TModel> FusionRating<TModel>(this InputField<TModel, double> field, Action<RatingBuilder> build)
    {
        var rating = RatingOf(field).Value(NumberOf(field.ValueJson)).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name });
        (build ?? throw new ArgumentNullException(nameof(build)))(rating);
        return RenderedRating(field, rating);
    }

    private static double NumberOf(string valueJson)
        => JsonSerializer.Deserialize<double>(valueJson, WrittenNumbers);

    private static partial RatingBuilder RatingOf<TModel>(InputField<TModel, double> field);

    private static partial Rendered<FusionRating, TModel> RenderedRating<TModel>(InputField<TModel, double> field, RatingBuilder rating);
}
