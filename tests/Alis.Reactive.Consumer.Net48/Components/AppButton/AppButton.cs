using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Consumer.Net48.Components
{
    public sealed class AppButton : Component
    {
        public override ComponentVendor Vendor => ComponentVendor.Native;

        public Property<string> TextContent => Property<string>();

        public Event<Clicked> Click => Event<Clicked>();

        public sealed class Clicked : JsObject
        {
        }
    }
}
