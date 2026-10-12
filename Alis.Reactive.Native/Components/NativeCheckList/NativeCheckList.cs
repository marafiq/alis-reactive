using System;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A native list of checkboxes whose value is the checked values, declared by the names its element uses.
    /// </summary>
    /// <remarks>
    /// The container element carries the element ID and holds the checked values as a <c>string[]</c>.
    /// Render it with <c>Html.InputField(plan, m =&gt; m.Diets).NativeCheckList(b =&gt; ...)</c>.
    /// </remarks>
    public sealed class NativeCheckList : NativeComponent, IInputComponent
    {
        /// <summary>The checked values.</summary>
        public ComponentProperty<string[]> Value => Property<string[]>("value");

        /// <summary>Moves focus to the element.</summary>
        public ComponentMethod Focus => Method("focus");

        /// <summary>Raised when the user checks or clears a box.</summary>
        public TypedEvent<NativeCheckListChangeArgs> Change =>
            Event<NativeCheckListChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="NativeCheckList.Change"/>, read from the container.</summary>
    public class NativeCheckListChangeArgs
    {
        /// <summary>The checked values after the change.</summary>
        public string[]? Value { get; set; }
    }

    /// <summary>Renders a <see cref="NativeCheckList"/> for a model-bound field.</summary>
    public static class NativeCheckListHtmlExtensions
    {
        /// <summary>Renders a NativeCheckList with its defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeCheckList, TModel> NativeCheckList<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.NativeCheckList(_ => { });

        /// <summary>
        /// Renders a NativeCheckList bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Native.Components.NativeCheckList, TModel> NativeCheckList<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<NativeCheckListBuilder<TModel, TProp>> build)
            where TModel : class
        {
            var builder = new NativeCheckListBuilder<TModel, TProp>(field.Helper, field.Expression, field.ElementId, field.BindingPath);
            build(builder);
            return field.Render<global::Alis.Reactive.Native.Components.NativeCheckList>(builder);
        }
    }
}
