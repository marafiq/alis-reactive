using System.Collections.Generic;
using Alis.Reactive.Declarations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A bullet chart: EJ2's BulletChart, which draws each item's value against its target.
/// </summary>
/// <remarks>EJ2 raises <see cref="TooltipRender"/> before it shows a bar's tooltip, and <see cref="BulletChartMouseClick"/> when a person clicks the chart.</remarks>
[Mirrors("@syncfusion/ej2-charts", "BulletChart")]
public sealed class FusionBulletChart : FusionComponent
{
    /// <summary>Gets the items the chart draws.</summary>
    /// <typeparam name="TItem">The class of the items.</typeparam>
    /// <returns>The items.</returns>
    public Property<IReadOnlyList<TItem>> DataSource<TItem>() => Property<IReadOnlyList<TItem>>();

    /// <summary>Gets the event raised before the chart shows a bar's tooltip.</summary>
    public Event<IBulletchartTooltipEventArgs> TooltipRender => Event<IBulletchartTooltipEventArgs>();

    /// <summary>Gets the event raised when a person clicks the chart.</summary>
    public Event<IBulletMouseEventArgs> BulletChartMouseClick => Event<IBulletMouseEventArgs>();

    /// <summary>
    /// What a listener of <see cref="TooltipRender"/> receives; the tooltip shows what the listener writes.
    /// </summary>
    [Mirrors("@syncfusion/ej2-charts", "IBulletchartTooltipEventArgs")]
    public sealed class IBulletchartTooltipEventArgs : JsObject
    {
        /// <summary>Gets the bar's value, as the text the tooltip shows.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Gets the bar's target, as the text the tooltip shows.</summary>
        public ReadOnly<string> Target => ReadOnly<string>();

        /// <summary>Gets the event's name.</summary>
        public Property<string> Name => Property<string>();

        /// <summary>Gets the tooltip's text.</summary>
        public Property<string> Text => Property<string>(optional: true);

        /// <summary>Gets the tooltip's template, the HTML the tooltip shows in place of its text.</summary>
        public Property<string> Template => Property<string>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="BulletChartMouseClick"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-charts", "IBulletMouseEventArgs")]
    public sealed class IBulletMouseEventArgs : JsObject
    {
        /// <summary>Gets the id of the element the person clicked.</summary>
        public ReadOnly<string> Target => ReadOnly<string>();

        /// <summary>Gets the click's horizontal position, in pixels.</summary>
        public ReadOnly<double> X => ReadOnly<double>();

        /// <summary>Gets the click's vertical position, in pixels.</summary>
        public ReadOnly<double> Y => ReadOnly<double>();
    }
}
