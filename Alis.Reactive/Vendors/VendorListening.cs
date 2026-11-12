using Alis.Reactive.Schema;

namespace Alis.Reactive.Vendors;

internal static class VendorListening
{
    internal static readonly MethodListening OfAnEventTarget = new("addEventListener", "removeEventListener");
}
