using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.MultiColumnComboBox;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion MultiColumnComboBox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.Facility).FusionMultiColumnComboBox(b =&gt; ...)</c>; target it with
    /// <c>p.Component&lt;FusionMultiColumnComboBox&gt;(m =&gt; m.Facility)</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Set(c =&gt; c.Value, "1")</c> or <c>.Call(c =&gt; c.ShowPopup)</c>.
    /// </remarks>
    public sealed class FusionMultiColumnComboBox : FusionComponent, IInputComponent
    {
        /// <summary>The selected value; <see langword="null"/> when empty.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>The text shown for the selection.</summary>
        public Prop<string> Text => Prop<string>();

        /// <summary>The rows the popup offers; call <see cref="DataBind"/> after writing it.</summary>
        public Prop<object> DataSource => Prop<object>();

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

        /// <summary>Raised when the selection changes.</summary>
        public TypedEvent<FusionMultiColumnComboBoxChangeArgs> Change =>
            Event<FusionMultiColumnComboBoxChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionMultiColumnComboBox.Change"/>.</summary>
    public class FusionMultiColumnComboBoxChangeArgs : JsObject
    {
        /// <summary>Selected value after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Whether user interaction made the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
#nullable restore annotations

    /// <summary>
    /// Adds typed field mapping and rendering helpers for <see cref="FusionMultiColumnComboBox"/>.
    /// </summary>
    public static class FusionMultiColumnComboBoxHtmlExtensions
    {
        /// <summary>
        /// Maps display text and selected value fields using typed item expressions.
        /// </summary>
        public static MultiColumnComboBoxBuilder Fields<TItem>(
            this MultiColumnComboBoxBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value)
        {
            return builder.Fields(new MultiColumnComboBoxFieldSettings
            {
                Text = Field.Of(text).Name,
                Value = Field.Of(value).Name
            });
        }

        /// <summary>
        /// Maps display text, selected value, and grouping fields using typed item expressions.
        /// </summary>
        public static MultiColumnComboBoxBuilder Fields<TItem>(
            this MultiColumnComboBoxBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value,
            Expression<Func<TItem, object?>> groupBy)
        {
            return builder.Fields(new MultiColumnComboBoxFieldSettings
            {
                Text = Field.Of(text).Name,
                Value = Field.Of(value).Name,
                GroupBy = Field.Of(groupBy).Name
            });
        }

        /// <summary>Renders a FusionMultiColumnComboBox with its defaults; see the overload that takes the configuration.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionMultiColumnComboBox, TModel> FusionMultiColumnComboBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup)
            where TModel : class
            => setup.FusionMultiColumnComboBox(_ => { });

        /// <summary>
        /// Renders a FusionMultiColumnComboBox bound to the field wrapper's model property.
        /// </summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionMultiColumnComboBox, TModel> FusionMultiColumnComboBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<MultiColumnComboBoxBuilder> build)
            where TModel : class
        {
            var builder = setup.Helper.EJS().MultiColumnComboBoxFor(setup.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = setup.ElementId, ["name"] = setup.BindingPath });
            build(builder);
            return setup.Render<global::Alis.Reactive.Fusion.Components.FusionMultiColumnComboBox>(builder.Render());
        }
    }
}
