using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.Components
{
#nullable disable annotations
    /// <summary>
    /// The Syncfusion DropDownList, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it in a callback by its model member and use its members directly:
    /// <code>
    /// var careLevel = p.Component&lt;FusionDropDownList&gt;(m =&gt; m.CareLevel);
    /// careLevel.Value.Set("memory");
    /// careLevel.ShowPopup();
    /// </code>
    /// Read it in a condition: <c>p.When(careLevel.Value).Eq("memory")</c>.
    /// </remarks>
    public sealed class FusionDropDownList : FusionComponent, IInputComponent
    {
        /// <summary>The selected value; <see langword="null"/> when empty.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>The text shown for the selection, without changing the selected value.</summary>
        public Prop<string> Text => Prop<string>();

        /// <summary>The items the list offers.</summary>
        /// <remarks>Call <see cref="DataBind"/> after writing it so the list renders the new items.</remarks>
        public Prop<IEnumerable<object>> DataSource => Prop<IEnumerable<object>>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>Moves focus into the list.</summary>
        public void FocusIn() => Call();

        /// <summary>Removes focus from the list.</summary>
        public void FocusOut() => Call();

        /// <summary>Opens the popup.</summary>
        public void ShowPopup() => Call();

        /// <summary>Closes the popup.</summary>
        public void HidePopup() => Call();

        /// <summary>Raised when the selected value changes.</summary>
        public TypedEvent<FusionDropDownListChangeArgs> Change =>
            Event<FusionDropDownListChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

        /// <summary>Raised when the list receives focus.</summary>
        public TypedEvent<FusionDropDownListFocusArgs> Focus =>
            Event<FusionDropDownListFocusArgs>();

        /// <summary>Raised when the list loses focus.</summary>
        public TypedEvent<FusionDropDownListBlurArgs> Blur =>
            Event<FusionDropDownListBlurArgs>();
    }

    /// <summary>Payload of <see cref="FusionDropDownList.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Value).Eq("US")</c>.</remarks>
    public class FusionDropDownListChangeArgs : JsObject
    {
        /// <summary>Selected value after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionDropDownList.Focus"/>; carries no data.</summary>
    public class FusionDropDownListFocusArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionDropDownList.Blur"/>; carries no data.</summary>
    public class FusionDropDownListBlurArgs : JsObject
    {
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="FusionDropDownList"/> for a model-bound field.</summary>
    public static class FusionDropDownListHtmlExtensions
    {
        /// <summary>Maps display text and selected value fields using typed item expressions.</summary>
        /// <typeparam name="TItem">The item class the data source holds.</typeparam>
        /// <param name="builder">The Syncfusion builder being configured.</param>
        /// <param name="text">The item member shown as each option's text.</param>
        /// <param name="value">The item member submitted as each option's value.</param>
        public static DropDownListBuilder Fields<TItem>(
            this DropDownListBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value)
        {
            return builder.Fields(new DropDownListFieldSettings
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
        public static DropDownListBuilder Fields<TItem>(
            this DropDownListBuilder builder,
            Expression<Func<TItem, object?>> text,
            Expression<Func<TItem, object?>> value,
            Expression<Func<TItem, object?>> groupBy)
        {
            return builder.Fields(new DropDownListFieldSettings
            {
                Text = Field.Of(text).Name,
                Value = Field.Of(value).Name,
                GroupBy = Field.Of(groupBy).Name
            });
        }

        /// <summary>Renders a FusionDropDownList with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionDropDownList, TModel> FusionDropDownList<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionDropDownList(_ => { });

        /// <summary>
        /// Renders a FusionDropDownList bound to the field's model property, with the framework's
        /// element ID and binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the component before rendering.</param>
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
    }
}
