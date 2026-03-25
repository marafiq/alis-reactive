using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Factory extension for creating UploaderBuilder bound to a model property.
    ///
    /// SF Uploader has no UploaderFor — we use Uploader(id) with manual name binding,
    /// similar to RichTextEditor. AutoUpload(false) ensures form-mode only (no saveUrl).
    /// </summary>
    public static class FusionFileUploadHtmlExtensions
    {
        private static readonly FusionFileUpload Component = new FusionFileUpload();

        public static void FileUpload<TModel, TProp>(
            this InputFieldSetup<TModel, TProp> setup,
            Action<UploaderBuilder> configure)
            where TModel : class
        {
            setup.Plan.AddToComponentsMap(setup.BindingPath, new ComponentRegistration(
                setup.ElementId, Component.Vendor, setup.BindingPath, Component.ReadExpr, "fileupload",
                CoercionTypes.InferFromType(typeof(TProp))));

            var builder = setup.Helper.EJS().Uploader(setup.ElementId)
                .AutoUpload(false)
                .HtmlAttributes(new Dictionary<string, object> { ["name"] = setup.BindingPath });
            configure(builder);
            setup.Render(builder.Render());
        }
    }
}
