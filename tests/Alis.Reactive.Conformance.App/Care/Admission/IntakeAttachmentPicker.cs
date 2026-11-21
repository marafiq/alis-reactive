using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Vendors;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Conformance.App.Care;

[Mirrors("lib.dom", "HTMLInputElement")]
public sealed class IntakeAttachmentPicker : Component, IInputComponent
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public SelectedFiles Files => LiveObject<SelectedFiles>(nullable: true);

    public Event<Changed> Change => Event<Changed>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Files.Items, committedBy: Change);

    [Mirrors("lib.dom", "FileList")]
    public sealed class SelectedFiles : JsObject
    {
        public LiveObjects<BrowserFile> Items => LiveIterator<BrowserFile>();
    }

    [Mirrors("lib.dom", "Event")]
    public sealed class Changed : JsObject
    {
    }
}

public static class IntakeAttachmentFields
{
    public static Rendered<IntakeAttachmentPicker, TModel> AttachmentPicker<TModel>(this InputField<TModel, List<IFormFile>> field)
    {
        var input = new TagBuilder("input") { TagRenderMode = TagRenderMode.SelfClosing };
        input.Attributes["id"] = field.Id;
        input.Attributes["name"] = field.Name;
        input.Attributes["type"] = "file";
        input.Attributes["multiple"] = "multiple";
        return field.Render<IntakeAttachmentPicker>(input);
    }
}
