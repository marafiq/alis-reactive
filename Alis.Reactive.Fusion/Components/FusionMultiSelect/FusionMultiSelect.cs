using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion MultiSelect, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionMultiSelect&gt;(m =&gt; m.Skills)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Value, new[] { "cpr" })</c> or <c>p.When(ms.Get(c =&gt; c.Value)).NotNull()</c>.
    /// </remarks>
    public sealed class FusionMultiSelect : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The selected values.</summary>
        public ComponentProperty<string[]> Value => Property<string[]>(ValueMember);

        /// <summary>The items the list offers. Call <see cref="DataBind"/> after writing it outside a filtering event.</summary>
        public ComponentProperty<IEnumerable<object>?> DataSource => Property<IEnumerable<object>?>("dataSource");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Opens the selection popup.</summary>
        public ComponentMethod ShowPopup => Method("showPopup");

        /// <summary>Closes the selection popup.</summary>
        public ComponentMethod HidePopup => Method("hidePopup");

        /// <summary>Raised when the selection changes.</summary>
        public TypedEvent<FusionMultiSelectChangeArgs> Change =>
            Event<FusionMultiSelectChangeArgs>("change");

        /// <summary>Raised as the user types, before the popup filters its items.</summary>
        public TypedEvent<FusionMultiSelectFilteringArgs> Filtering =>
            Event<FusionMultiSelectFilteringArgs>("filtering");
    }

    /// <summary>Payload of <see cref="FusionMultiSelect.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.</remarks>
    public class FusionMultiSelectChangeArgs
    {
        /// <summary>Selected values after the change.</summary>
        public string[]? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionMultiSelect.Filtering"/>.</summary>
    /// <remarks>Server-side filtering: <c>p.Payload(args).Set(a =&gt; a.PreventDefaultAction, true)</c>, request matches, then <c>s.Payload(args).Call(a =&gt; a.UpdateData, res.Read(r =&gt; r.Items))</c>.</remarks>
    public class FusionMultiSelectFilteringArgs : JsObject
    {
        /// <summary>Text the user typed.</summary>
        public string Text { get; set; } = "";

        /// <summary>Set to <see langword="true"/> to suppress the built-in client-side filter while a server request is in flight.</summary>
        public bool PreventDefaultAction { get; set; }

        /// <summary>Hands filtered items to the popup; valid after an HTTP response returns.</summary>
        public ComponentMethod<IEnumerable<object>> UpdateData => Method<IEnumerable<object>>("updateData");
    }

    /// <summary>Renders a <see cref="FusionMultiSelect"/> for a model-bound field.</summary>
    public static class FusionMultiSelectHtmlExtensions
    {
        /// <summary>Maps display text and selected value fields using typed item expressions.</summary>
        public static MultiSelectBuilder Fields<TItem>(
            this MultiSelectBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value)
        {
            return builder.Fields(new MultiSelectFieldSettings
            {
                Text = JsonMemberName.Of(MemberNameOf(text)),
                Value = JsonMemberName.Of(MemberNameOf(value))
            });
        }

        /// <summary>Maps display text, selected value and grouping fields using typed item expressions.</summary>
        public static MultiSelectBuilder Fields<TItem>(
            this MultiSelectBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value,
            Expression<Func<TItem, object?>> groupBy)
        {
            return builder.Fields(new MultiSelectFieldSettings
            {
                Text = JsonMemberName.Of(MemberNameOf(text)),
                Value = JsonMemberName.Of(MemberNameOf(value)),
                GroupBy = JsonMemberName.Of(MemberNameOf(groupBy))
            });
        }

        /// <summary>Renders a FusionMultiSelect with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionMultiSelect, TModel> FusionMultiSelect<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionMultiSelect(_ => { });

        /// <summary>
        /// Renders a FusionMultiSelect bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
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

        private static string MemberNameOf<T>(Expression<Func<T, object?>> expression)
        {
            var body = expression.Body;
            if (body is UnaryExpression unary) body = unary.Operand;
            if (body is MemberExpression member) return member.Member.Name;
            throw new ArgumentException("Expression must be a member access, for example x => x.Text.", nameof(expression));
        }
    }
}
