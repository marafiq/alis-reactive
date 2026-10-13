using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Buttons;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion CheckBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it in a callback by its model member and use its members directly. Syncfusion applies
    /// property writes on its next tick; follow a write with <c>DataBind()</c> to apply it before the
    /// next step:
    /// <code>
    /// var checkBox = p.Component&lt;FusionCheckBox&gt;(m =&gt; m.Consent);
    /// checkBox.Checked.Set(true);
    /// checkBox.DataBind();
    /// </code>
    /// </remarks>
    public sealed class FusionCheckBox : FusionComponent, IInputComponent
    {
        /// <summary>Whether the box is checked.</summary>
        public Prop<bool> Checked => Prop<bool>();

        /// <summary>Whether the box shows the indeterminate state.</summary>
        public Prop<bool> Indeterminate => Prop<bool>();

        /// <summary>Whether the box is disabled.</summary>
        public Prop<bool> Disabled => Prop<bool>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Toggles the box as a click would, raising <see cref="Change"/>.</summary>
        public void Click() => Call();

        /// <summary>Moves focus into the box.</summary>
        public void FocusIn() => Call();

        /// <summary>Raised when the box is checked or cleared.</summary>
        public TypedEvent<FusionCheckBoxChangeArgs> Change => Event<FusionCheckBoxChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Checked, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionCheckBox.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Checked).Truthy()</c>.</remarks>
    public class FusionCheckBoxChangeArgs : JsObject
    {
        /// <summary>Whether the box is checked after the change.</summary>
        public ReadOnly<bool> Checked => ReadOnly<bool>();
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="FusionCheckBox"/> for a model-bound field.</summary>
    public static class FusionCheckBoxHtmlExtensions
    {
        /// <summary>Renders a FusionCheckBox with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionCheckBox, TModel> FusionCheckBox<TModel>(
            this InputBoundField<TModel, bool> field)
            where TModel : class
            => field.FusionCheckBox(_ => { });

        /// <summary>
        /// Renders a FusionCheckBox bound to the field's boolean model property, with the framework's
        /// element ID and binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the component before rendering.</param>
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
