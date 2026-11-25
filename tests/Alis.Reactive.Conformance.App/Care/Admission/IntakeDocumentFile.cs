using Alis.Reactive.Declarations;
using Alis.Reactive.RequestsAndResponses;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Conformance.App.Care;

[Mirrors("lib.dom", "HTMLInputElement")]
public sealed class IntakeDocumentFile : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public ChosenFiles Files => LiveObject<ChosenFiles>(nullable: true);

    [Mirrors("lib.dom", "FileList")]
    public sealed class ChosenFiles : JsObject
    {
        public LiveObjects<BrowserFile> Items => LiveIterator<BrowserFile>();
    }
}
