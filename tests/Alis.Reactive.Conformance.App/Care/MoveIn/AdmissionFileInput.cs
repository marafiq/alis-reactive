using Alis.Reactive.Declarations;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

[Mirrors("lib.dom", "HTMLInputElement")]
public sealed class AdmissionFileInput : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public AdmissionFileList Files => LiveObject<AdmissionFileList>(nullable: true);

    public Event<AdmissionFileChange> Change => Event<AdmissionFileChange>();
}

[Mirrors("lib.dom", "Event")]
public sealed class AdmissionFileChange : JsObject
{
}

[Mirrors("lib.dom", "FileList")]
public sealed class AdmissionFileList : JsObject
{
    public LiveObjects<BrowserFile> Items => LiveIterator<BrowserFile>();
}

public static class AdmissionFilesHtml
{
    public static Rendered<AdmissionFileInput, TModel> AdmissionFiles<TModel>(this ReactivePlan<TModel> plan, string id, bool multiple)
    {
        var input = new TagBuilder("input");
        input.TagRenderMode = TagRenderMode.SelfClosing;
        input.Attributes["id"] = id;
        input.Attributes["type"] = "file";
        if (multiple) input.Attributes["multiple"] = "multiple";
        return plan.Render<AdmissionFileInput>(id, input);
    }
}
