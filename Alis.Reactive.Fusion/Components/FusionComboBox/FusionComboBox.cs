using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion ComboBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it in a callback by its model member and use its members directly:
    /// <code>
    /// var resident = p.Component&lt;FusionComboBox&gt;(m =&gt; m.Resident);
    /// resident.Value.Set("alice");
    /// resident.ShowPopup();
    /// p.Element("resident-name").Text.Set(resident.Text);
    /// </code>
    /// </remarks>
    public sealed class FusionComboBox : FusionComponent, IInputComponent
    {
        /// <summary>The selected value.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>The text shown in the input.</summary>
        public Prop<string> Text => Prop<string>();

        /// <summary>The zero-based index of the selected item.</summary>
        public Prop<int?> Index => Prop<int?>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Moves focus into the input.</summary>
        public void FocusIn() => Call();

        /// <summary>Removes focus from the input.</summary>
        public void FocusOut() => Call();

        /// <summary>Opens the popup.</summary>
        public void ShowPopup() => Call();

        /// <summary>Closes the popup.</summary>
        public void HidePopup() => Call();

        /// <summary>Clears the selection and the text.</summary>
        public void Clear() => Call();

        /// <summary>Raised when the selected value changes.</summary>
        public TypedEvent<FusionComboBoxChangeArgs> Change =>
            Event<FusionComboBoxChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised when the input receives focus.</summary>
        public TypedEvent<FusionComboBoxFocusArgs> Focus =>
            Event<FusionComboBoxFocusArgs>();

        /// <summary>Raised when the input loses focus.</summary>
        public TypedEvent<FusionComboBoxBlurArgs> Blur =>
            Event<FusionComboBoxBlurArgs>();
    }

    /// <summary>Payload of <see cref="FusionComboBox.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Value).Eq("alice")</c>.</remarks>
    public class FusionComboBoxChangeArgs : JsObject
    {
        /// <summary>Selected value after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionComboBox.Focus"/>; carries no data.</summary>
    public class FusionComboBoxFocusArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionComboBox.Blur"/>; carries no data.</summary>
    public class FusionComboBoxBlurArgs : JsObject
    {
    }

    /// <summary>Renders a <see cref="FusionComboBox"/> for a model-bound field.</summary>
    public static class FusionComboBoxHtmlExtensions
    {
        /// <summary>Maps display text and selected value fields using typed item expressions.</summary>
        /// <typeparam name="TItem">The item class the data source holds.</typeparam>
        /// <param name="builder">The Syncfusion builder being configured.</param>
        /// <param name="text">The item member shown as each option's text.</param>
        /// <param name="value">The item member submitted as each option's value.</param>
        public static ComboBoxBuilder Fields<TItem>(
            this ComboBoxBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value)
        {
            return builder.Fields(new ComboBoxFieldSettings
            {
                Text = Field.Of(text).Name,
                Value = Field.Of(value).Name
            });
        }

        /// <summary>Renders a FusionComboBox with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionComboBox, TModel> FusionComboBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionComboBox(_ => { });

        /// <summary>
        /// Renders a FusionComboBox bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the component before rendering.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionComboBox, TModel> FusionComboBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<ComboBoxBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().ComboBoxFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionComboBox>(builder.Render());
        }
    }
}
