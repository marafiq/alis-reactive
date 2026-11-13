#if NET48
using System;
using System.Collections.Generic;
using System.Web;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.Inputs;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a file uploader bound to a file member of its model.
/// </summary>
public static class FusionUploaderHtml
{
    /// <summary>Renders a file uploader for the field's file member, sending its file with the form.</summary>
    /// <param name="field">The input field the uploader is rendered for, over one file.</param>
    /// <returns>The rendered uploader, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionUploader, TModel> FusionUploader<TModel>(this InputField<TModel, HttpPostedFileBase?> field)
        => throw new NotImplementedException();

    /// <summary>Renders a file uploader for the field's file member, sending its file with the form, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the uploader is rendered for, over one file.</param>
    /// <param name="build">What the view sets on the uploader.</param>
    /// <returns>The rendered uploader, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionUploader, TModel> FusionUploader<TModel>(this InputField<TModel, HttpPostedFileBase?> field, Action<UploaderBuilder> build)
        => throw new NotImplementedException();

    /// <summary>Renders a file uploader for the field's list of files, sending its files with the form.</summary>
    /// <param name="field">The input field the uploader is rendered for, over a list of files.</param>
    /// <returns>The rendered uploader, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionUploader, TModel> FusionUploader<TModel>(this InputField<TModel, List<HttpPostedFileBase>> field)
        => throw new NotImplementedException();

    /// <summary>Renders a file uploader for the field's list of files, sending its files with the form, with what the view sets on EJ2's builder.</summary>
    /// <param name="field">The input field the uploader is rendered for, over a list of files.</param>
    /// <param name="build">What the view sets on the uploader.</param>
    /// <returns>The rendered uploader, which writes its markup where the view puts it.</returns>
    public static Rendered<FusionUploader, TModel> FusionUploader<TModel>(this InputField<TModel, List<HttpPostedFileBase>> field, Action<UploaderBuilder> build)
        => throw new NotImplementedException();
}
#endif
