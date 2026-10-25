using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion AutoComplete, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it in a callback by its model member and use its members directly:
    /// <code>
    /// var physician = p.Component&lt;FusionAutoComplete&gt;(m =&gt; m.Physician);
    /// physician.Value.Set("smith");
    /// physician.ShowPopup();
    /// </code>
    /// Read it in a condition: <c>p.When(physician.Value).NotNull()</c>.
    /// </remarks>
    public sealed class FusionAutoComplete : FusionComponent, IInputComponent
    {
        /// <summary>The selected value; <see langword="null"/> when empty.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>The text shown for the selection; writing it selects the item that shows this text.</summary>
        public Prop<string> Text => Prop<string>();

        /// <summary>The items the popup suggests from, typed by your item class.</summary>
        /// <remarks>Call <see cref="DataBind"/> after writing it outside a filtering event.</remarks>
        /// <typeparam name="TItem">Your item class.</typeparam>
        public Prop<IEnumerable<TItem>> Items<TItem>() where TItem : class => Prop<IEnumerable<TItem>>("dataSource");

        /// <summary>Whether the input accepts interaction.</summary>
        public Prop<bool> Enabled => Prop<bool>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Moves focus into the input.</summary>
        public void FocusIn() => Call();

        /// <summary>Removes focus from the input.</summary>
        public void FocusOut() => Call();

        /// <summary>Opens the suggestion popup.</summary>
        public void ShowPopup() => Call();

        /// <summary>Closes the suggestion popup.</summary>
        public void HidePopup() => Call();

        /// <summary>Clears the selection and the text; call <see cref="DataBind"/> to apply it at once.</summary>
        public void Clear() => Call();

        /// <summary>Raised when the selected value changes.</summary>
        public TypedEvent<FusionAutoCompleteChangeArgs> Change =>
            Event<FusionAutoCompleteChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised as the user types, before the popup filters its items.</summary>
        public TypedEvent<FusionAutoCompleteFilteringArgs> Filtering =>
            Event<FusionAutoCompleteFilteringArgs>();
    }

    /// <summary>Payload of <see cref="FusionAutoComplete.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Value).Eq("smith")</c>.</remarks>
    public class FusionAutoCompleteChangeArgs : JsObject
    {
        /// <summary>Selected value after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionAutoComplete.Filtering"/>.</summary>
    /// <remarks>
    /// Server-side filtering: suppress the built-in filter, request matches, then hand them to the
    /// popup. Here the directory search binds <c>DirectorySearch { Name }</c> and answers
    /// <c>DirectoryMatches { Physicians, Count, Summary }</c>; a search that matches nobody closes the
    /// popup instead.
    /// <code>
    /// args.PreventDefaultAction.Set(true);
    /// p.Get(url)
    ///     .Gather&lt;DirectorySearch&gt;(g =&gt; g.Set(x =&gt; x.Name, args.Text))
    ///     .WhileLoading(l =&gt; l.Element("directory-status").Text.Set("Searching the physician directory..."))
    ///     .Response(r =&gt; r.OnSuccess&lt;DirectoryMatches&gt;((found, s) =&gt;
    ///     {
    ///         s.Element("directory-status").Text.Set(found.Get(x =&gt; x.Summary));
    ///         // A name nobody in the directory has closes the list the last search left open.
    ///         s.When(found.Get(x =&gt; x.Count)).Eq(0)
    ///             .Then(none =&gt; none.Component&lt;FusionAutoComplete&gt;(m =&gt; m.Physician).HidePopup())
    ///             .Else(some =&gt; args.UpdateData(found.Get(x =&gt; x.Physicians)));
    ///     }));
    /// </code>
    /// </remarks>
    public class FusionAutoCompleteFilteringArgs : JsObject
    {
        /// <summary>Text the user typed.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>
        /// Set to <see langword="true"/> to suppress the built-in client-side filter, so the popup
        /// does not show "No records found" while a server request is in flight.
        /// </summary>
        public Prop<bool> PreventDefaultAction => Prop<bool>();

        /// <summary>Hands filtered items to the popup; valid after an HTTP response returns.</summary>
        /// <typeparam name="TItems">The items' type, such as the server's own list.</typeparam>
        /// <param name="items">The items to show, read when the step runs.</param>
        public void UpdateData<TItems>(TypedSource<TItems> items) where TItems : IEnumerable<object> =>
            Call(Pass<IEnumerable<object>, TItems>(items));
    }

    /// <summary>Renders a <see cref="FusionAutoComplete"/> for a model-bound field.</summary>
    public static class FusionAutoCompleteHtmlExtensions
    {
        /// <summary>Maps display text and selected value fields using typed item expressions.</summary>
        /// <typeparam name="TItem">The item class the data source holds.</typeparam>
        /// <param name="builder">The Syncfusion builder being configured.</param>
        /// <param name="text">The item member shown as each option's text.</param>
        /// <param name="value">The item member submitted as each option's value.</param>
        public static AutoCompleteBuilder Fields<TItem>(
            this AutoCompleteBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value)
        {
            return builder.Fields(new AutoCompleteFieldSettings
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
        public static AutoCompleteBuilder Fields<TItem>(
            this AutoCompleteBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value,
            Expression<Func<TItem, object?>> groupBy)
        {
            return builder.Fields(new AutoCompleteFieldSettings
            {
                Text = Field.Of(text).Name,
                Value = Field.Of(value).Name,
                GroupBy = Field.Of(groupBy).Name
            });
        }

        /// <summary>
        /// Renders a FusionAutoComplete bound to the field's model property, with the framework's
        /// element ID and binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the component before rendering.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionAutoComplete, TModel> FusionAutoComplete<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<AutoCompleteBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().AutoCompleteFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionAutoComplete>(builder.Render());
        }
    }
}
