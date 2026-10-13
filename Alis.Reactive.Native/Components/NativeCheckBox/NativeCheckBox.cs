using System;

namespace Alis.Reactive.Native.Components
{
#nullable disable annotations
    /// <summary>
    /// The native HTML checkbox, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.Ambulatory).NativeCheckBox(b =&gt; ...)</c>; target it with
    /// <c>p.Component&lt;NativeCheckBox&gt;(m =&gt; m.Ambulatory)</c>, for example <c>.Set(c =&gt; c.Checked, true)</c>.
    /// </remarks>
    public sealed class NativeCheckBox : NativeComponent, IInputComponent
    {
        /// <summary>Whether the box is checked.</summary>
        public Prop<bool> Checked => Prop<bool>();

        /// <summary>Moves focus to the element.</summary>
        public void Focus() => Call();

        /// <summary>Raised when the user checks or clears the box.</summary>
        public TypedEvent<NativeCheckBoxChangeArgs> Change =>
            Event<NativeCheckBoxChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Checked, committedBy: Change);
    }

    /// <summary>Payload of <see cref="NativeCheckBox.Change"/>, read from the element.</summary>
    public class NativeCheckBoxChangeArgs : JsObject
    {
        /// <summary>Whether the box is checked after the change.</summary>
        public ReadOnly<bool?> Checked => ReadOnly<bool?>();
    }
#nullable restore annotations

    /// <summary>Renders a <see cref="NativeCheckBox"/> for a model-bound field.</summary>
    public static class NativeCheckBoxHtmlExtensions
    {
        /// <summary>Renders a NativeCheckBox with its defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeCheckBox, TModel> NativeCheckBox<TModel>(
            this InputBoundField<TModel, bool> field)
            where TModel : class
            => field.NativeCheckBox(_ => { });

        /// <summary>
        /// Renders a NativeCheckBox bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeCheckBox, TModel> NativeCheckBox<TModel>(
            this InputBoundField<TModel, bool> field,
            Action<NativeCheckBoxBuilder<TModel, bool>> build)
            where TModel : class
        {
            var builder = new NativeCheckBoxBuilder<TModel, bool>(field.Helper, field.Expression, field.ElementId, field.BindingPath);
            build(builder);
            return field.Render<global::Alis.Reactive.Native.Components.NativeCheckBox>(builder);
        }
    }
}
