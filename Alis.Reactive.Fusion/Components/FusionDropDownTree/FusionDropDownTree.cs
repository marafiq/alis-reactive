using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion DropDownTree, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.ResidentIds).FusionDropDownTree(b =&gt; ...)</c>; target it with
    /// <c>p.Component&lt;FusionDropDownTree&gt;(m =&gt; m.ResidentIds)</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Set(c =&gt; c.Value, new[] { "alice" }).Call(c =&gt; c.DataBind)</c>.
    /// </remarks>
    public sealed class FusionDropDownTree : FusionComponent, IInputComponent
    {
        /// <summary>The selected node values; <see langword="null"/> clears them.</summary>
        public ComponentProperty<string[]?> Value => Property<string[]?>("value");

        /// <summary>The text shown for the selection.</summary>
        public ComponentProperty<string?> Text => Property<string?>("text");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Opens the popup.</summary>
        public ComponentMethod ShowPopup => Method("showPopup");

        /// <summary>Closes the popup.</summary>
        public ComponentMethod HidePopup => Method("hidePopup");

        /// <summary>Clears the selection.</summary>
        public ComponentMethod Clear => Method("clear");

        /// <summary>Raised when the selection changes.</summary>
        public TypedEvent<FusionDropDownTreeChangeArgs> Change =>
            Event<FusionDropDownTreeChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionDropDownTree.Change"/>.</summary>
    public class FusionDropDownTreeChangeArgs
    {
        /// <summary>Selected values after the change.</summary>
        public string[]? Value { get; set; }

        /// <summary>Selected values before the change.</summary>
        public string[]? OldValue { get; set; }

        /// <summary>Whether user interaction made the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>
    /// Creates a FusionDropDownTree inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Syncfusion's MVC builder owns initial render configuration such as fields,
    /// tree settings, popup dimensions, templates, and selection mode.
    /// </remarks>
    public static class FusionDropDownTreeHtmlExtensions
    {
        /// <summary>Renders a FusionDropDownTree with its defaults; see the overload that takes the configuration.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDropDownTree, TModel> FusionDropDownTree<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup)
            where TModel : class
            => setup.FusionDropDownTree(_ => { });

        /// <summary>
        /// Renders a FusionDropDownTree bound to the field's model property.
        /// </summary>
        /// <typeparam name="TProp">Model value type rendered by the drop-down tree.</typeparam>
        /// <param name="setup">Field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures initial DropDownTree options before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDropDownTree, TModel> FusionDropDownTree<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<DropDownTreeBuilder> build)
            where TModel : class
        {
            var builder = setup.Helper.EJS().DropDownTreeFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            return setup.Render<global::Alis.Reactive.Fusion.Components.FusionDropDownTree>(builder.Render());
        }
    }
}
