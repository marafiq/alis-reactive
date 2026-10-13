using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.RichTextEditor;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Rich Text Editor, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Reach it with <c>var carePlan = p.Component&lt;FusionRichTextEditor&gt;(m =&gt; m.CarePlan);</c> and use its
    /// members directly: <c>carePlan.Value.Set("&lt;p&gt;Daily review&lt;/p&gt;");</c> or
    /// <c>p.When(carePlan.Value).NotEmpty()</c>.
    /// </remarks>
    public sealed class FusionRichTextEditor : FusionComponent, IInputComponent
    {
        /// <summary>The content as HTML; <see langword="null"/> when empty.</summary>
        public Prop<string> Value => Prop<string>();

        /// <summary>Moves focus into the editor.</summary>
        public void FocusIn() => Call();

        /// <summary>Raised when the content changes and focus leaves the editor.</summary>
        public TypedEvent<FusionRichTextEditorChangeArgs> Change =>
            Event<FusionRichTextEditorChangeArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionRichTextEditor.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args.Value).NotNull()</c>.</remarks>
    public class FusionRichTextEditorChangeArgs : JsObject
    {
        /// <summary>Content after the change, as HTML.</summary>
        public ReadOnly<string> Value => ReadOnly<string>();

        /// <summary>Whether user interaction triggered the change.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>Renders a <see cref="FusionRichTextEditor"/> for a model-bound field.</summary>
    public static class FusionRichTextEditorHtmlExtensions
    {
        /// <summary>Renders a FusionRichTextEditor with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionRichTextEditor, TModel> FusionRichTextEditor<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionRichTextEditor(_ => { });

        /// <summary>
        /// Renders a FusionRichTextEditor bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        /// <param name="field">The model-bound field from <c>Html.InputField(...)</c>.</param>
        /// <param name="build">Configures the Syncfusion builder before rendering.</param>
        /// <returns>The rendered component; wire its events with <c>.Reactive(evt =&gt; evt.Change, ...)</c>.</returns>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionRichTextEditor, TModel> FusionRichTextEditor<TModel, TProp>(
            this InputBoundField<TModel, TProp> field,
            Action<RichTextEditorBuilder> build)
            where TModel : class
        {
            var builder = field.Helper.EJS().RichTextEditorFor(field.Expression)
                .HtmlAttributes(new Dictionary<string, object> { ["name"] = field.BindingPath });

            // RichTextEditorFor derives model.Id from the member name, and Render() uses model.Id for
            // the textarea id and the script's appendTo target; set it to the framework's element ID.
            builder.model.Id = field.ElementId;
            build(builder);

            // The editor writes its markup to Output instead of returning it.
            return field.Render<global::Alis.Reactive.Fusion.Components.FusionRichTextEditor>(writer =>
            {
                builder.Output = writer;
                builder.Render();
            });
        }
    }
}
