using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion MultiSelect, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it with <c>var skills = p.Component&lt;FusionMultiSelect&gt;(m =&gt; m.Skills);</c> and use its
    /// members directly: <c>skills.Value.Set(new[] { "cpr" });</c> or <c>p.When(skills.Value).NotNull()</c>.
    /// </remarks>
    public sealed class FusionMultiSelect : FusionComponent, IInputComponent
    {
        /// <summary>The selected values.</summary>
        public Property<string[]> Value => Property<string[]>();

        /// <summary>The items the list offers, typed by your item class. Call <see cref="DataBind"/> after writing it outside a filtering event.</summary>
        /// <typeparam name="TItem">Your item class.</typeparam>
        public Property<IEnumerable<TItem>> Items<TItem>() where TItem : class => Property<IEnumerable<TItem>>("dataSource");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Method();

        /// <summary>Opens the selection popup.</summary>
        public void ShowPopup() => Method();

        /// <summary>Closes the selection popup.</summary>
        public void HidePopup() => Method();

        /// <summary>Raised when the selection changes.</summary>
        public Event<FusionMultiSelectChangeArgs> Change =>
            Event<FusionMultiSelectChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised as the user types, before the popup filters its items.</summary>
        public Event<FusionMultiSelectFilteringArgs> Filtering =>
            Event<FusionMultiSelectFilteringArgs>();
    }

    /// <summary>Payload of <see cref="FusionMultiSelect.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Value).NotNull()</c>.</remarks>
    public class FusionMultiSelectChangeArgs : JsObject
    {
        /// <summary>Selected values after the change.</summary>
        public ReadOnly<string[]> Value => ReadOnly<string[]>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionMultiSelect.Filtering"/>.</summary>
    /// <remarks>
    /// For server-side filtering, stop the built-in filter with <c>args.PreventDefaultAction.Set(true)</c>,
    /// request the matches, and hand them over in the success callback with
    /// <c>args.UpdateData(json.Get(x =&gt; x.Items))</c>.
    /// </remarks>
    public class FusionMultiSelectFilteringArgs : JsObject
    {
        /// <summary>Text the user typed.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Set to <see langword="true"/> to suppress the built-in client-side filter while a server request is in flight.</summary>
        public Property<bool> PreventDefaultAction => Property<bool>();

        /// <summary>Hands filtered items to the popup; valid after an HTTP response returns.</summary>
        /// <typeparam name="TItems">The items' type, such as the server's own list.</typeparam>
        /// <param name="items">The items to show, read when the step runs.</param>
        public void UpdateData<TItems>(TypedSource<TItems> items) where TItems : IEnumerable<object> =>
            Method(Pass<IEnumerable<object>, TItems>(items));
    }

    /// <summary>Renders a <see cref="FusionMultiSelect"/> for a model-bound field.</summary>
    public static class FusionMultiSelectHtmlExtensions
    {
        /// <summary>Maps display text and selected value fields using typed item expressions.</summary>
        /// <typeparam name="TItem">The item class the data source holds.</typeparam>
        /// <param name="builder">The Syncfusion builder being configured.</param>
        /// <param name="text">The item member shown as each option's text.</param>
        /// <param name="value">The item member submitted as each option's value.</param>
        public static MultiSelectBuilder Fields<TItem>(
            this MultiSelectBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value)
        {
            return builder.Fields(new MultiSelectFieldSettings
            {
                Text = Field.Of(text).Name,
                Value = Field.Of(value).Name
            });
        }

        /// <summary>Maps display text, selected value and grouping fields using typed item expressions.</summary>
        /// <typeparam name="TItem">The item class the data source holds.</typeparam>
        /// <param name="builder">The Syncfusion builder being configured.</param>
        /// <param name="text">The item member shown as each option's text.</param>
        /// <param name="value">The item member submitted as each option's value.</param>
        /// <param name="groupBy">The item member the options are grouped by.</param>
        public static MultiSelectBuilder Fields<TItem>(
            this MultiSelectBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value,
            Expression<Func<TItem, object?>> groupBy)
        {
            return builder.Fields(new MultiSelectFieldSettings
            {
                Text = Field.Of(text).Name,
                Value = Field.Of(value).Name,
                GroupBy = Field.Of(groupBy).Name
            });
        }

        /// <summary>Renders a FusionMultiSelect with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionMultiSelect, TModel> FusionMultiSelect<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionMultiSelect(_ => { });

        /// <summary>
        /// Renders a FusionMultiSelect bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionMultiSelect, TModel> FusionMultiSelect<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<MultiSelectBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().MultiSelectFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionMultiSelect>(builder.Render());
        }
    }
}
