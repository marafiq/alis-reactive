using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a Syncfusion Uploader inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.Document)</c>, then call
    /// <c>.FileUpload(b =&gt; { b.AllowedExtensions(".pdf,.docx"); })</c>.
    /// </remarks>
    public static class FusionFileUploadHtmlExtensions
    {
        private static readonly FusionFileUpload Component = new FusionFileUpload();

        /// <summary>
        /// Renders a Syncfusion Uploader bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model type.</typeparam>
        /// <typeparam name="TProp">The bound property type.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="configure">Callback to configure the Uploader (allowed extensions, max size, etc.).</param>
        public static void FileUpload<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
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
