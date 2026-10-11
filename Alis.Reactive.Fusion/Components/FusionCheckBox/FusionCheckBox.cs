using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion CheckBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionCheckBox&gt;(m =&gt; m.Consent)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>. Syncfusion applies
    /// property writes on its next tick; follow a write with <c>.Call(c =&gt; c.DataBind)</c> to apply
    /// it before the next step: <c>.Set(c =&gt; c.Checked, true).Call(c =&gt; c.DataBind)</c>.
    /// </remarks>
    public sealed class FusionCheckBox : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "checked";

        /// <summary>Whether the box is checked.</summary>
        public ComponentProperty<bool> Checked => Property<bool>(ValueMember);

        /// <summary>Whether the box shows the indeterminate state.</summary>
        public ComponentProperty<bool> Indeterminate => Property<bool>("indeterminate");

        /// <summary>Whether the box is disabled.</summary>
        public ComponentProperty<bool> Disabled => Property<bool>("disabled");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Toggles the box as a click would, raising <see cref="Change"/>.</summary>
        public ComponentMethod Click => Method("click");

        /// <summary>Moves focus into the box.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Raised when the box is checked or cleared.</summary>
        public TypedEvent<FusionCheckBoxChangeArgs> Change => Event<FusionCheckBoxChangeArgs>("change");
    }

    /// <summary>Payload of <see cref="FusionCheckBox.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Checked).Truthy()</c>.</remarks>
    public class FusionCheckBoxChangeArgs
    {
        /// <summary>Whether the box is checked after the change.</summary>
        public bool Checked { get; set; }
    }

    /// <summary>Renders a <see cref="FusionCheckBox"/> for a model-bound field.</summary>
    public static class FusionCheckBoxHtmlExtensions
    {
        /// <summary>Renders a FusionCheckBox with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionCheckBox, TModel> FusionCheckBox<TModel>(
            this InputBoundField<TModel, bool> field)
            where TModel : class
            => field.FusionCheckBox(_ => { });

        /// <summary>
        /// Renders a FusionCheckBox bound to the field's boolean model property, with the framework's
        /// element ID and binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionCheckBox, TModel> FusionCheckBox<TModel>(
            this InputBoundField<TModel, bool> field,
            Action<CheckBoxBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().CheckBoxFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionCheckBox>(builder.Render());
        }
    }
}
