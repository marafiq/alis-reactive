using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Switch, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionSwitch&gt;(m =&gt; m.ReceiveNotifications)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Checked, true)</c> or <c>p.When(sw.Get(c =&gt; c.Checked)).Truthy()</c>.
    /// </remarks>
    public sealed class FusionSwitch : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "checked";

        /// <summary>Whether the switch is on.</summary>
        public ComponentProperty<bool> Checked => Property<bool>(ValueMember);

        /// <summary>Raised when the switch is turned on or off.</summary>
        public TypedEvent<FusionSwitchChangeArgs> Change => Event<FusionSwitchChangeArgs>("change");
    }

    /// <summary>Payload of <see cref="FusionSwitch.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Checked).Truthy()</c>.</remarks>
    public class FusionSwitchChangeArgs
    {
        /// <summary>Whether the switch is on after the change.</summary>
        public bool Checked { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Renders a <see cref="FusionSwitch"/> for a model-bound field.</summary>
    public static class FusionSwitchHtmlExtensions
    {
        /// <summary>Renders a FusionSwitch with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSwitch, TModel> FusionSwitch<TModel>(
            this InputBoundField<TModel, bool> field)
            where TModel : class
            => field.FusionSwitch(_ => { });

        /// <summary>
        /// Renders a FusionSwitch bound to the field's boolean model property, with the framework's
        /// element ID and binding name.
        /// </summary>
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
