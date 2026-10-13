using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion AutoComplete, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionAutoComplete&gt;(m =&gt; m.Physician)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Value, "smith")</c>, <c>.Call(c =&gt; c.ShowPopup)</c>, <c>.Get(c =&gt; c.Value)</c>.
    /// </remarks>
    public sealed class FusionAutoComplete : FusionComponent, IInputComponent
    {
        /// <summary>The selected value; <see langword="null"/> when empty.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>The text shown in the input, without changing the selected value.</summary>
        public Prop<string> Text => Prop<string>();

        /// <summary>The items the popup suggests from.</summary>
        /// <remarks>Call <see cref="DataBind"/> after writing it outside a filtering event.</remarks>
        public Prop<IEnumerable<object>> DataSource => Prop<IEnumerable<object>>();

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

        /// <summary>Raised when the selected value changes.</summary>
        public TypedEvent<FusionAutoCompleteChangeArgs> Change =>
            Event<FusionAutoCompleteChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised as the user types, before the popup filters its items.</summary>
        public TypedEvent<FusionAutoCompleteFilteringArgs> Filtering =>
            Event<FusionAutoCompleteFilteringArgs>("filtering");
    }

    /// <summary>Payload of <see cref="FusionAutoComplete.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).Eq("smith")</c>.</remarks>
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
    /// popup.
    /// <code>
    /// p.Payload(args).Set(a =&gt; a.PreventDefaultAction, true);
    /// p.Get(url)
    ///  .Gather&lt;MedicationSearchQuery&gt;(g =&gt; g.Set(x =&gt; x.MedicationType, p.Payload(args).Get(a =&gt; a.Text)))
    ///  .Response(r =&gt; r.OnSuccess&lt;MedicationSearchResponse&gt;((json, s) =&gt;
    ///      s.Payload(args).Call(a =&gt; a.UpdateData, json.Read(j =&gt; j.Medications))));
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
#nullable restore annotations

    /// <summary>Renders a <see cref="FusionAutoComplete"/> for a model-bound field.</summary>
    public static class FusionAutoCompleteHtmlExtensions
    {
        /// <summary>Maps display text and selected value fields using typed item expressions.</summary>
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
