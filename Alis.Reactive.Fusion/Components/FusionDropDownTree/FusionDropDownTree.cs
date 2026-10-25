using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion DropDownTree, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.ResidentIds).FusionDropDownTree(b =&gt; ...)</c>, then reach it
    /// in a callback by its model member and use its members directly:
    /// <code>
    /// var residents = p.Component&lt;FusionDropDownTree&gt;(m =&gt; m.ResidentIds);
    /// residents.Value.Set(new[] { "alice" });
    /// residents.DataBind();
    /// </code>
    /// </remarks>
    public sealed class FusionDropDownTree : FusionComponent, IInputComponent
    {
        /// <summary>The selected node values; <see langword="null"/> clears them.</summary>
        public Property<string[]> Value => Property<string[]>();

        /// <summary>The text shown for the selection.</summary>
        public Property<string> Text => Property<string>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Method();

        /// <summary>Opens the popup.</summary>
        public void ShowPopup() => Method();

        /// <summary>Closes the popup.</summary>
        public void HidePopup() => Method();

        /// <summary>Clears the selection.</summary>
        public void Clear() => Method();

        /// <summary>Raised when the selection changes.</summary>
        public Event<FusionDropDownTreeChangeArgs> Change =>
            Event<FusionDropDownTreeChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionDropDownTree.Change"/>.</summary>
    public class FusionDropDownTreeChangeArgs : JsObject
    {
        /// <summary>Selected values after the change.</summary>
        public ReadOnly<string[]> Value => ReadOnly<string[]>();

        /// <summary>Selected values before the change.</summary>
        public ReadOnly<string[]> OldValue => ReadOnly<string[]>();

        /// <summary>Whether user interaction made the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
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
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDropDownTree, TModel> FusionDropDownTree<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup)
            where TModel : class
            => setup.FusionDropDownTree(_ => { });

        /// <summary>
        /// Renders a FusionDropDownTree bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
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
