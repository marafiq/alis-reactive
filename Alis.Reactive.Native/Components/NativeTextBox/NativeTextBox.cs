using System;

namespace Alis.Reactive.Native.Components
{
#nullable disable annotations
    /// <summary>
    /// The native HTML <c>&lt;input&gt;</c>, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.PreferredName).NativeTextBox(b =&gt; ...)</c>; reach it with
    /// <c>p.Component&lt;NativeTextBox&gt;(m =&gt; m.PreferredName)</c> and use its members directly:
    /// <code>
    /// var preferredName = p.Component&lt;NativeTextBox&gt;(m =&gt; m.PreferredName);
    /// preferredName.Value.Set(json.Get(x =&gt; x.PreferredName));
    /// </code>
    /// </remarks>
    public sealed class NativeTextBox : NativeComponent, IInputComponent
    {
        /// <summary>The input value; empty text when nothing is entered.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>Moves focus to the element.</summary>
        public void Focus() => Call();

        /// <summary>Raised when the value changes and the element loses focus.</summary>
        public TypedEvent<NativeTextBoxChangeArgs> Change =>
            Event<NativeTextBoxChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="NativeTextBox.Change"/>, read from the element.</summary>
    public class NativeTextBoxChangeArgs : JsObject
    {
        /// <summary>The input value after the change.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="NativeTextBox"/> for a model-bound field.</summary>
    public static class NativeTextBoxHtmlExtensions
    {
        /// <summary>Renders a NativeTextBox with its defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <typeparam name="TProp">The type of the bound model property.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeTextBox, TModel> NativeTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.NativeTextBox(_ => { });

        /// <summary>
        /// Renders a NativeTextBox bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <typeparam name="TProp">The type of the bound model property.</typeparam>
        /// <param name="field">The model-bound field the input renders into.</param>
        /// <param name="build">Configures the input, for example <c>b =&gt; b.Placeholder("Preferred name")</c>.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeTextBox, TModel> NativeTextBox<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<NativeTextBoxBuilder<TModel, TProp>> build)
            where TModel : class
        {
            var builder = new NativeTextBoxBuilder<TModel, TProp>(field.Helper, field.Expression, field.ElementId, field.BindingPath);
            build(builder);
            return field.Render<global::Alis.Reactive.Native.Components.NativeTextBox>(builder);
        }
    }
}
