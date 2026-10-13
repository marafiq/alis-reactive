using System;

namespace Alis.Reactive.Native.Components
{
#nullable disable annotations
    /// <summary>
    /// The native HTML <c>&lt;select&gt;</c>, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.CareLevel).NativeDropDown(b =&gt; ...)</c>; reach it with
    /// <c>p.Component&lt;NativeDropDown&gt;(m =&gt; m.CareLevel)</c> and use its members directly:
    /// <code>
    /// var careLevel = p.Component&lt;NativeDropDown&gt;(m =&gt; m.CareLevel);
    /// careLevel.Value.Set("Memory Care");
    /// p.When(careLevel.Value).NotEmpty().Then(..);
    /// </code>
    /// </remarks>
    public sealed class NativeDropDown : NativeComponent, IInputComponent
    {
        /// <summary>The select value; empty text when nothing is entered.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>Moves focus to the element.</summary>
        public void Focus() => Call();

        /// <summary>Raised when the value changes and the element loses focus.</summary>
        public TypedEvent<NativeDropDownChangeArgs> Change =>
            Event<NativeDropDownChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="NativeDropDown.Change"/>, read from the element.</summary>
    public class NativeDropDownChangeArgs : JsObject
    {
        /// <summary>The selected option value after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="NativeDropDown"/> for a model-bound field.</summary>
    public static class NativeDropDownHtmlExtensions
    {
        /// <summary>Renders a NativeDropDown with its defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <typeparam name="TProp">The type of the bound model property.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeDropDown, TModel> NativeDropDown<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.NativeDropDown(_ => { });

        /// <summary>
        /// Renders a NativeDropDown bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <typeparam name="TProp">The type of the bound model property.</typeparam>
        /// <param name="field">The model-bound field the select renders into.</param>
        /// <param name="build">Configures the select, for example <c>b =&gt; b.Items(levels).Placeholder("-- Select --")</c>.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeDropDown, TModel> NativeDropDown<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<NativeDropDownBuilder<TModel, TProp>> build)
            where TModel : class
        {
            var builder = new NativeDropDownBuilder<TModel, TProp>(field.Helper, field.Expression, field.ElementId, field.BindingPath);
            build(builder);
            return field.Render<global::Alis.Reactive.Native.Components.NativeDropDown>(builder);
        }
    }
}
