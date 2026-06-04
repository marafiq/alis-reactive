using System;
using System.Collections.Generic;
using Alis.Reactive.Native;
using Alis.Reactive.Native.Extensions;
using Syncfusion.EJ2;
using Syncfusion.EJ2.Inputs;

using ComponentRegistrationSource = Alis.Reactive.Fusion.Components.FusionFileUpload;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Creates a FusionFileUpload inside a field wrapper, bound to a model property.
    /// </summary>
    /// <remarks>
    /// Start the chain with <c>Html.InputField(plan, m =&gt; m.Document)</c>, then call
    /// <c>.FusionFileUpload(b =&gt; { b.AllowedExtensions(".pdf,.docx"); })</c>.
    /// </remarks>
    public static class FusionFileUploadHtmlExtensions
    {
        /// <summary>
        /// Renders a FusionFileUpload bound to the field's model property.
        /// </summary>
        /// <typeparam name="TModel">The view model used to author the Reactive Plan.</typeparam>
        /// <typeparam name="TProp">The model value type represented by the file upload input.</typeparam>
        /// <param name="setup">The field wrapper created by <c>Html.InputField()</c>.</param>
        /// <param name="build">Callback to build the Uploader (allowed extensions, max size, etc.).</param>
        public static void FusionFileUpload<TModel, TProp>(
            this InputBoundField<TModel, TProp> setup,
            Action<UploaderBuilder> build)
            where TModel : class
        {
            var registration = ComponentRegistrationSource.Registration;
            setup.RegisterInputComponent(registration);

            var builder = setup.Helper.EJS().Uploader(setup.ElementId)
                .AutoUpload(false)
                .HtmlAttributes(new Dictionary<string, object> { ["name"] = setup.BindingPath });
            build(builder);
            setup.Render(builder.Render());
        }
    }
}
