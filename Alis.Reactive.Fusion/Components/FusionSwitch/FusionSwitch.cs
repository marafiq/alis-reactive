using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Switch, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it with <c>var notifications = p.Component&lt;FusionSwitch&gt;(m =&gt; m.ReceiveNotifications);</c> and use
    /// its members directly: <c>notifications.Checked.Set(true);</c> or <c>p.When(notifications.Checked).Truthy()</c>.
    /// </remarks>
    public sealed class FusionSwitch : FusionComponent, IInputComponent
    {
        /// <summary>Whether the switch is on.</summary>
        public Property<bool> Checked => Property<bool>();

        /// <summary>Raised when the switch is turned on or off.</summary>
        public Event<FusionSwitchChangeArgs> Change => Event<FusionSwitchChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Checked, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionSwitch.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Checked).Truthy()</c>.</remarks>
    public class FusionSwitchChangeArgs : JsObject
    {
        /// <summary>Whether the switch is on after the change.</summary>
        public ReadOnly<bool> Checked => ReadOnly<bool>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Renders a <see cref="FusionSwitch"/> for a model-bound field.</summary>
    public static class FusionSwitchHtmlExtensions
    {
        /// <summary>Renders a FusionSwitch with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSwitch, TModel> FusionSwitch<TModel>(
            this InputBoundField<TModel, bool> field)
            where TModel : class
            => field.FusionSwitch(_ => { });

        /// <summary>
        /// Renders a FusionSwitch bound to the field's boolean model property, with the framework's
        /// element ID and binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSwitch, TModel> FusionSwitch<TModel>(
            this InputBoundField<TModel, bool> field,
            Action<SwitchBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().SwitchFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionSwitch>(builder.Render());
        }
    }
}
