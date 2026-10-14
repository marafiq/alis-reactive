using System;
using System.Linq.Expressions;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// The native HTML <c>&lt;input type="hidden"&gt;</c>, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.HiddenFieldFor(plan, m =&gt; m.ScreeningId)</c>; reach it with
    /// <c>p.Component&lt;NativeHiddenField&gt;(m =&gt; m.ScreeningId)</c> and use its members directly, for
    /// example to keep the id a response returns:
    /// <code>
    /// var screening = s.Component&lt;NativeHiddenField&gt;(m =&gt; m.ScreeningId);
    /// screening.Value.Set(json.Get(x =&gt; x.ScreeningId));
    /// </code>
    /// </remarks>
    public sealed class NativeHiddenField : NativeComponent, IInputComponent
    {
        /// <summary>The hidden value. Writing a new value raises <see cref="Change"/>.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>
        /// Raised when the value is written with a new value, by the plan or a script, so a value the page
        /// writes commits as one the user changes does: the field's rules check it again at once. Writing
        /// the value the field already holds raises nothing.
        /// </summary>
        public TypedEvent<NativeHiddenFieldChangeArgs> Change =>
            Event<NativeHiddenFieldChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="NativeHiddenField.Change"/>, read from the element.</summary>
    public class NativeHiddenFieldChangeArgs : JsObject
    {
        /// <summary>The hidden value after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();
    }

    /// <summary>
    /// Factory extension for rendering model-bound hidden fields directly in a Razor view.
    /// </summary>
    /// <remarks>
    /// A hidden field is a hidden <c>InputField</c>: it renders without the label and validation
    /// slot, and still registers with the Reactive Plan for gather.
    /// </remarks>
    public static class NativeHiddenFieldHtmlExtensions
    {
        /// <summary>
        /// Registers and renders a hidden field bound to a model property.
        /// </summary>
        /// <typeparam name="TModel">View model that owns the bound property.</typeparam>
        /// <typeparam name="TProp">Model value type registered as the hidden input value.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
        /// <param name="plan">Reactive Plan that receives the hidden-field registration.</param>
        /// <param name="expression">Model property expression used for MVC binding and component registration.</param>
        /// <returns>The rendered hidden field, ready for <c>.Reactive(...)</c>.</returns>
        public static RenderedComponent<NativeHiddenField, TModel> HiddenFieldFor<TModel, TProp>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            Expression<Func<TModel, TProp>> expression)
            where TModel : class
        {
            var field = html.InputField(plan, expression, o => o.Hidden());
            return field.Render<NativeHiddenField>(
                new NativeHiddenFieldBuilder<TModel, TProp>(html, expression, field.ElementId, field.BindingPath));
        }
    }
}
