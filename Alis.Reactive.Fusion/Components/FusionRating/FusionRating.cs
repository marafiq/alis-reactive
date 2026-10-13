using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion Rating, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionRating&gt;(m =&gt; m.SatisfactionScore)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>. Syncfusion applies property writes on its next tick; follow a write with
    /// <c>.Call(c =&gt; c.DataBind)</c> to apply it before the next step:
    /// <c>.Set(c =&gt; c.Value, 4d).Call(c =&gt; c.DataBind)</c>.
    /// </remarks>
    public sealed class FusionRating : FusionComponent, IInputComponent
    {
        /// <summary>The rating.</summary>
        public Prop<double> Value => Prop<double>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Resets the rating to its minimum.</summary>
        public void Reset() => Call();

        /// <summary>Raised when the rating changes.</summary>
        public TypedEvent<FusionRatingValueChangedArgs> ValueChanged =>
            Event<FusionRatingValueChangedArgs>("valueChanged");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: ValueChanged);
    }

    /// <summary>Payload of <see cref="FusionRating.ValueChanged"/>.</summary>
    public class FusionRatingValueChangedArgs : JsObject
    {
        /// <summary>Rating after the change.</summary>
        public ReadOnly<double> Value => ReadOnly<double>();

        /// <summary>Rating before the change.</summary>
        public ReadOnly<double> PreviousValue => ReadOnly<double>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="FusionRating"/> for a model-bound field.</summary>
    public static class FusionRatingHtmlExtensions
    {
        /// <summary>Renders a FusionRating with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionRating, TModel> FusionRating<TModel>(
            this InputBoundField<TModel, double> field)
            where TModel : class
            => field.FusionRating(_ => { });

        /// <summary>
        /// Renders a FusionRating bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.ValueChanged, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionRating, TModel> FusionRating<TModel>(
            this InputBoundField<TModel, double> field,
            Action<RatingBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().Rating(field.ElementId)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionRating>(builder.Render());
        }
    }
}
