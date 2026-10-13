using System;
using System.Collections.Generic;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Uploader, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.InputField(plan, m =&gt; m.CarePlanDocument).FusionFileUpload(b =&gt; ...)</c>. A
    /// form-data request sends the selected files with the form's other inputs:
    /// <c>p.Post(url).Gather(g =&gt; g.Form()).AsFormData()</c>.
    /// </remarks>
    public sealed class FusionFileUpload : FusionComponent, IInputComponent
    {
        /// <summary>The selected files, as the browser's <c>File</c> objects a form-data request sends.</summary>
        /// <remarks>The uploader lists its files as items that each carry the browser's file as <c>rawFile</c>.</remarks>
        public Prop<IReadOnlyList<object>> Files => Prop<IReadOnlyList<object>>("filesData[].rawFile");

        /// <summary>Raised when the user selects files.</summary>
        public TypedEvent<FusionFileUploadSelectedArgs> Selected =>
            Event<FusionFileUploadSelectedArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Files, committedBy: Selected);
    }

    /// <summary>Payload of <see cref="FusionFileUpload.Selected"/>.</summary>
    public class FusionFileUploadSelectedArgs : JsObject
    {
        /// <summary>Number of files selected.</summary>
        public ReadOnly<int> FilesCount => ReadOnly<int>();

        /// <summary>Whether user interaction made the selection.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>
    /// Renders the file upload component inside a bound input field.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.Document)</c>, then call
    /// <c>.FusionFileUpload(b =&gt; { b.AllowedExtensions(".pdf,.docx"); })</c>.
    /// </remarks>
    public static class FusionFileUploadHtmlExtensions
    {
        /// <summary>Renders a FusionFileUpload with its defaults; see the overload that takes the configuration.</summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">The bound model property's type.</typeparam>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionFileUpload, TModel> FusionFileUpload<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup)
            where TModel : class
            => setup.FusionFileUpload(_ => { });

        /// <summary>
        /// Renders the file upload component bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <typeparam name="TProp">Model value type represented by the file upload input.</typeparam>
        /// <param name="setup">Field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Configures initial uploader options before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionFileUpload, TModel> FusionFileUpload<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<UploaderBuilder> build)
            where TModel : class
        {
            var builder = setup.Helper.EJS().Uploader(setup.ElementId)
                .AutoUpload(false)
                .HtmlAttributes(new Dictionary<string, object> { ["name"] = setup.BindingPath });
            build(builder);
            return setup.Render<global::Alis.Reactive.Fusion.Components.FusionFileUpload>(builder.Render());
        }
    }
}
