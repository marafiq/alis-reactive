using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a file uploader bound to a file member of its model.
/// </summary>
public static partial class FusionUploaderHtml
{
    private const string Marker = "data-fusion-uploader";

    private static Rendered<FusionUploader, TModel> Rendered<TModel, TValue>(InputField<TModel, TValue> field, bool multiple, Action<UploaderBuilder> build)
    {
        var uploader = UploaderOf(field).AutoUpload(false).Multiple(multiple).HtmlAttributes(new Dictionary<string, object> { ["name"] = field.Name, [Marker] = string.Empty });
        (build ?? throw new ArgumentNullException(nameof(build)))(uploader);
        return RenderedUploader(field, uploader);
    }

    private static partial UploaderBuilder UploaderOf<TModel, TValue>(InputField<TModel, TValue> field);

    private static partial Rendered<FusionUploader, TModel> RenderedUploader<TModel, TValue>(InputField<TModel, TValue> field, UploaderBuilder uploader);
}
