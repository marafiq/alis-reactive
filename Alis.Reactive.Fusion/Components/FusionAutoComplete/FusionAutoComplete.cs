using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
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
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The selected value; <see langword="null"/> when empty.</summary>
        public ComponentProperty<string?> Value => Property<string?>(ValueMember);

        /// <summary>The text shown in the input, without changing the selected value.</summary>
        public ComponentProperty<string?> Text => Property<string?>("text");

        /// <summary>The items the popup suggests from.</summary>
        /// <remarks>Call <see cref="DataBind"/> after writing it outside a filtering event.</remarks>
        public ComponentProperty<IEnumerable<object>?> DataSource => Property<IEnumerable<object>?>("dataSource");

        /// <summary>Whether the input accepts interaction.</summary>
        public ComponentProperty<bool> Enabled => Property<bool>("enabled");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Moves focus into the input.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Removes focus from the input.</summary>
        public ComponentMethod FocusOut => Method("focusOut");

        /// <summary>Opens the suggestion popup.</summary>
        public ComponentMethod ShowPopup => Method("showPopup");

        /// <summary>Closes the suggestion popup.</summary>
        public ComponentMethod HidePopup => Method("hidePopup");

        /// <summary>Raised when the selected value changes.</summary>
        public TypedEvent<FusionAutoCompleteChangeArgs> Change =>
            Event<FusionAutoCompleteChangeArgs>("change");

        /// <summary>Raised as the user types, before the popup filters its items.</summary>
        public TypedEvent<FusionAutoCompleteFilteringArgs> Filtering =>
            Event<FusionAutoCompleteFilteringArgs>("filtering");
    }

    /// <summary>Payload of <see cref="FusionAutoComplete.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).Eq("smith")</c>.</remarks>
    public class FusionAutoCompleteChangeArgs
    {
        /// <summary>Selected value after the change.</summary>
        public string? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionAutoComplete.Filtering"/>.</summary>
    /// <remarks>
    /// Server-side filtering: suppress the built-in filter, request matches, then hand them to the
    /// popup.
    /// <code>
    /// p.Payload(args).Set(a =&gt; a.PreventDefaultAction, true);
    /// p.Get(url).Gather(g =&gt; g.FromEvent(args, x =&gt; x.Text, "q"))
    ///  .Response(r =&gt; r.OnSuccess&lt;Matches&gt;((res, s) =&gt;
    ///      s.Payload(args).Call(a =&gt; a.UpdateData, res.Read(m =&gt; m.Items))));
    /// </code>
    /// </remarks>
    public class FusionAutoCompleteFilteringArgs : JsObject
    {
        /// <summary>Text the user typed.</summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Set to <see langword="true"/> to suppress the built-in client-side filter, so the popup
        /// does not show "No records found" while a server request is in flight.
        /// </summary>
        public bool PreventDefaultAction { get; set; }

        /// <summary>Hands filtered items to the popup; valid after an HTTP response returns.</summary>
        public ComponentMethod<IEnumerable<object>> UpdateData => Method<IEnumerable<object>>("updateData");
    }

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
                Text = JsonMemberName.Of(MemberNameOf(text)),
                Value = JsonMemberName.Of(MemberNameOf(value))
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
                Text = JsonMemberName.Of(MemberNameOf(text)),
                Value = JsonMemberName.Of(MemberNameOf(value)),
                GroupBy = JsonMemberName.Of(MemberNameOf(groupBy))
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

        private static string MemberNameOf<T>(Expression<Func<T, object?>> expression)
        {
            var body = expression.Body;
            if (body is UnaryExpression unary) body = unary.Operand;
            if (body is MemberExpression member) return member.Member.Name;
            throw new ArgumentException("Expression must be a member access, for example x => x.Text.", nameof(expression));
        }
    }
}
