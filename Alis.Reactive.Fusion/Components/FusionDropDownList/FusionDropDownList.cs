using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Native;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion DropDownList, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Use as the type in <c>p.Component&lt;FusionDropDownList&gt;(m =&gt; m.CareLevel)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Value, "memory")</c>, <c>.Call(c =&gt; c.ShowPopup)</c>, <c>.Get(c =&gt; c.Value)</c>.
    /// </remarks>
    public sealed class FusionDropDownList : FusionComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMember => "value";

        /// <summary>The selected value; <see langword="null"/> when empty.</summary>
        public ComponentProperty<string?> Value => Property<string?>(ValueMember);

        /// <summary>The text shown for the selection, without changing the selected value.</summary>
        public ComponentProperty<string?> Text => Property<string?>("text");

        /// <summary>The items the list offers.</summary>
        /// <remarks>Call <see cref="DataBind"/> after writing it so the list renders the new items.</remarks>
        public ComponentProperty<IEnumerable<object>> DataSource => Property<IEnumerable<object>>("dataSource");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>Moves focus into the list.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Removes focus from the list.</summary>
        public ComponentMethod FocusOut => Method("focusOut");

        /// <summary>Opens the popup.</summary>
        public ComponentMethod ShowPopup => Method("showPopup");

        /// <summary>Closes the popup.</summary>
        public ComponentMethod HidePopup => Method("hidePopup");

        /// <summary>Raised when the selected value changes.</summary>
        public TypedEvent<FusionDropDownListChangeArgs> Change =>
            Event<FusionDropDownListChangeArgs>("change");

        /// <summary>Raised when the list receives focus.</summary>
        public TypedEvent<FusionDropDownListFocusArgs> Focus =>
            Event<FusionDropDownListFocusArgs>("focus");

        /// <summary>Raised when the list loses focus.</summary>
        public TypedEvent<FusionDropDownListBlurArgs> Blur =>
            Event<FusionDropDownListBlurArgs>("blur");
    }

    /// <summary>Payload of <see cref="FusionDropDownList.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).Eq("US")</c>.</remarks>
    public class FusionDropDownListChangeArgs
    {
        /// <summary>Selected value after the change.</summary>
        public string? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Payload of <see cref="FusionDropDownList.Focus"/>; carries no data.</summary>
    public class FusionDropDownListFocusArgs
    {
    }

    /// <summary>Payload of <see cref="FusionDropDownList.Blur"/>; carries no data.</summary>
    public class FusionDropDownListBlurArgs
    {
    }

    /// <summary>Renders a <see cref="FusionDropDownList"/> for a model-bound field.</summary>
    public static class FusionDropDownListHtmlExtensions
    {
        /// <summary>Maps display text and selected value fields using typed item expressions.</summary>
        public static DropDownListBuilder Fields<TItem>(
            this DropDownListBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value)
        {
            return builder.Fields(new DropDownListFieldSettings
            {
                Text = JsonMemberName.Of(MemberNameOf(text)),
                Value = JsonMemberName.Of(MemberNameOf(value))
            });
        }

        /// <summary>Maps display text, selected value and grouping fields using typed item expressions.</summary>
        public static DropDownListBuilder Fields<TItem>(
            this DropDownListBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value,
            Expression<Func<TItem, object?>> groupBy)
        {
            return builder.Fields(new DropDownListFieldSettings
            {
                Text = JsonMemberName.Of(MemberNameOf(text)),
                Value = JsonMemberName.Of(MemberNameOf(value)),
                GroupBy = JsonMemberName.Of(MemberNameOf(groupBy))
            });
        }

        /// <summary>Renders a FusionDropDownList with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDropDownList, TModel> FusionDropDownList<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionDropDownList(_ => { });

        /// <summary>
        /// Renders a FusionDropDownList bound to the field's model property, with the framework's
        /// element ID and binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDropDownList, TModel> FusionDropDownList<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<DropDownListBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().DropDownListFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["id"] = field.ElementId, ["name"] = field.BindingPath });
            build(builder);
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionDropDownList>(builder.Render());
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
