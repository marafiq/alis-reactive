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
        public ReadOnly<IReadOnlyList<BrowserFile>> Files => FilesData.Each(file => file.RawFile!);

        // The uploader lists its files as items that each carry the browser's file.
        private ReadOnly<IReadOnlyList<FusionUploaderFile>> FilesData => ReadOnly<IReadOnlyList<FusionUploaderFile>>();

        /// <summary>Raised when the user selects files, before the uploader adds them; set <c>Cancel</c> to refuse them.</summary>
        public Event<FusionFileUploadSelectedArgs> Selected =>
            Event<FusionFileUploadSelectedArgs>();

        /// <summary>
        /// Raised once the uploader holds the files the user chose: after files are added or a selection is
        /// refused, and after the user removes a file.
        /// </summary>
        /// <remarks>
        /// The framework raises it, not Syncfusion: the Uploader raises <c>selected</c> and <c>removing</c>
        /// before it adds or removes the files and nothing after, so the framework's uploader code raises
        /// <c>filesChanged</c> once it has. The input commits on it, so a form re-checks the files the user
        /// now sees. Clearing the files (a form reset) raises nothing.
        /// </remarks>
        public Event<FusionFileUploadFilesChangedArgs> FilesChanged =>
            Event<FusionFileUploadFilesChangedArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Files, committedBy: FilesChanged);
    }

    /// <summary>Payload of <see cref="FusionFileUpload.Selected"/>.</summary>
    public class FusionFileUploadSelectedArgs : JsObject
    {
        /// <summary>The files just selected: <c>p.From(args.FilesData).Count()</c> counts them.</summary>
        public ReadOnly<IReadOnlyList<FusionUploaderFile>> FilesData => ReadOnly<IReadOnlyList<FusionUploaderFile>>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to refuse the selection.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>Payload of <see cref="FusionFileUpload.FilesChanged"/>.</summary>
    public class FusionFileUploadFilesChangedArgs : JsObject
    {
        /// <summary>The files the uploader holds now: <c>p.From(args.FilesData).Count()</c> counts them.</summary>
        public ReadOnly<IReadOnlyList<FusionUploaderFile>> FilesData => ReadOnly<IReadOnlyList<FusionUploaderFile>>();
    }

    /// <summary>A file the uploader lists, by the names its JavaScript API uses.</summary>
    public class FusionUploaderFile
    {
        /// <summary>The file's name.</summary>
        public string Name { get; set; } = "";

        /// <summary>The file's size in bytes.</summary>
        public long Size { get; set; }

        /// <summary>The file's extension, without the dot.</summary>
        public string Type { get; set; } = "";

        /// <summary>The browser's own <c>File</c>, which a form-data request sends.</summary>
        /// <remarks>Only the browser holds it; a server-side copy of the item has none, so it may be null.</remarks>
        public BrowserFile? RawFile { get; set; }
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
                .HtmlAttributes(new Dictionary<string, object>
                {
                    ["name"] = setup.BindingPath,
                    // The framework's uploader code finds the uploader by it and raises FilesChanged.
                    ["data-alis-file-upload"] = ""
                });
            build(builder);
            return setup.Render<global::Alis.Reactive.Fusion.Components.FusionFileUpload>(builder.Render());
        }
    }
}
