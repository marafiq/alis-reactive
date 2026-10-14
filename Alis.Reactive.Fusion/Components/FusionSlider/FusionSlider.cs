using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Slider, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.RoomTemperature).FusionSlider(b =&gt; ...)</c>, reach it with
    /// <c>var temperature = p.Component&lt;FusionSlider&gt;(m =&gt; m.RoomTemperature);</c> and use its members directly.
    /// A range slider's value is <see cref="RangeValue"/>, the same JavaScript property typed as its start and end:
    /// <c>temperature.RangeValue.Set(new double[] { 14, 16 }); temperature.DataBind();</c>.
    /// </remarks>
    public sealed class FusionSlider : FusionComponent, IInputComponent
    {
        /// <summary>The value of a single-handle slider.</summary>
        public Prop<double> Value => Prop<double>();

        /// <summary>The start and end of a range slider.</summary>
        public Prop<double[]> RangeValue => Prop<double[]>("value");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Raised as the handle moves.</summary>
        public TypedEvent<FusionSliderChangeArgs> Change =>
            Event<FusionSliderChangeArgs>();

        /// <summary>Raised when the handle comes to rest.</summary>
        public TypedEvent<FusionSliderChangeArgs> Changed =>
            Event<FusionSliderChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Changed);
    }

    /// <summary>Payload of <see cref="FusionSlider.Change"/> and <see cref="FusionSlider.Changed"/>.</summary>
    public class FusionSliderChangeArgs : JsObject
    {
        /// <summary>The value after the move.</summary>
        public ReadOnly<double> Value => ReadOnly<double>();

        /// <summary>The value before the move.</summary>
        public ReadOnly<double> PreviousValue => ReadOnly<double>();

        /// <summary>The value as shown.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>The kind of move.</summary>
        public ReadOnly<string> Action => ReadOnly<string>();

        /// <summary>Whether user interaction made the move.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>
    /// Creates a FusionSlider inside a field wrapper, bound to a model property.
    /// </summary>
    public static class FusionSliderHtmlExtensions
    {
        /// <summary>Renders a FusionSlider with its defaults; see the overload that takes the configuration.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSlider, TModel> FusionSlider<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup)
            where TModel : class
            => setup.FusionSlider(_ => { });

        /// <summary>
        /// Renders a FusionSlider bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
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
