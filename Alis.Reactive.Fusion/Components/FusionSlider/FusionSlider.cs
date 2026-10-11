using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Slider, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.RoomTemperature).FusionSlider(b =&gt; ...)</c>; target it with
    /// <c>p.Component&lt;FusionSlider&gt;(m =&gt; m.RoomTemperature)</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>. A range slider's value is <see cref="RangeValue"/>, the same JavaScript property typed as its
    /// start and end: <c>.Set(c =&gt; c.RangeValue, new double[] { 14, 16 }).Call(c =&gt; c.DataBind)</c>.
    /// </remarks>
    public sealed class FusionSlider : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The value of a single-handle slider.</summary>
        public ComponentProperty<double> Value => Property<double>(ValueMember);

        /// <summary>The start and end of a range slider.</summary>
        public ComponentProperty<double[]> RangeValue => Property<double[]>("value");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Raised as the handle moves.</summary>
        public TypedEvent<FusionSliderChangeArgs> Change =>
            Event<FusionSliderChangeArgs>("change");

        /// <summary>Raised when the handle comes to rest.</summary>
        public TypedEvent<FusionSliderChangeArgs> Changed =>
            Event<FusionSliderChangeArgs>("changed");
    }

    /// <summary>Payload of <see cref="FusionSlider.Change"/> and <see cref="FusionSlider.Changed"/>.</summary>
    public class FusionSliderChangeArgs
    {
        /// <summary>The value after the move.</summary>
        public double Value { get; set; }

        /// <summary>The value before the move.</summary>
        public double PreviousValue { get; set; }

        /// <summary>The value as shown.</summary>
        public string? Text { get; set; }

        /// <summary>The kind of move.</summary>
        public string? Action { get; set; }

        /// <summary>Whether user interaction made the move.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>
    /// Creates a FusionSlider inside a field wrapper, bound to a model property.
    /// </summary>
    public static class FusionSliderHtmlExtensions
    {
        /// <summary>Renders a FusionSlider with its defaults; see the overload that takes the configuration.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSlider, TModel> FusionSlider<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup)
            where TModel : class
            => setup.FusionSlider(_ => { });

        /// <summary>
        /// Renders a FusionSlider bound to the field's model property.
        /// </summary>
        /// <typeparam name="TProp">Model value type rendered by the slider.</typeparam>
        /// <param name="setup">Field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures initial Slider options before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSlider, TModel> FusionSlider<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<SliderBuilder> build)
            where TModel : class
        {
            var builder = setup.Helper.EJS().Slider(setup.ElementId)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            return setup.Render<global::Alis.Reactive.Fusion.Components.FusionSlider>(builder.Render());
        }
    }
}
