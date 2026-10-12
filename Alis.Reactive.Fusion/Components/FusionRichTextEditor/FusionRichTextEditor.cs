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
    /// Use as the type in <c>p.Component&lt;FusionRichTextEditor&gt;(m =&gt; m.CarePlan)</c>; every
    /// declared member works with <c>Set</c>, <c>Call</c> and <c>Get</c>, for example
    /// <c>.Set(c =&gt; c.Value, "&lt;p&gt;Daily review&lt;/p&gt;")</c> or <c>p.When(carePlan.Get(c =&gt; c.Value)).NotEmpty()</c>.
    /// </remarks>
    public sealed class FusionRichTextEditor : FusionComponent, IInputComponent
    {
        /// <summary>The content as HTML; <see langword="null"/> when empty.</summary>
        public ComponentProperty<string?> Value => Property<string?>("value");

        /// <summary>Moves focus into the editor.</summary>
        public ComponentMethod FocusIn => Method("focusIn");

        /// <summary>Raised when the content changes and focus leaves the editor.</summary>
        public TypedEvent<FusionRichTextEditorChangeArgs> Change =>
            Event<FusionRichTextEditorChangeArgs>("change");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);
    }

    /// <summary>Payload of <see cref="FusionRichTextEditor.Change"/>.</summary>
    /// <remarks>Read in conditions: <c>p.When(args, x =&gt; x.Value).NotNull()</c>.</remarks>
    public class FusionRichTextEditorChangeArgs
    {
        /// <summary>Content after the change, as HTML.</summary>
        public string? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }

    /// <summary>Renders a <see cref="FusionRichTextEditor"/> for a model-bound field.</summary>
    public static class FusionRichTextEditorHtmlExtensions
    {
        /// <summary>Renders a FusionRichTextEditor with Syncfusion's defaults; see the overload that takes <c>build</c>.</summary>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionRichTextEditor, TModel> FusionRichTextEditor<TModel, TProp>(
            this InputBoundField<TModel, TProp> field)
            where TModel : class
            => field.FusionRichTextEditor(_ => { });

        /// <summary>
        /// Renders a FusionRichTextEditor bound to the field's model property, with the framework's element ID and
        /// binding name.
        /// </summary>
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
