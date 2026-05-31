using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    internal sealed class RegisteredComponentIdentity
    {
        private RegisteredComponentIdentity(ComponentId componentId, ComponentVendor vendor)
        {
            ComponentId = componentId;
            Vendor = vendor;
        }

        internal ComponentId ComponentId { get; }
        internal ComponentVendor Vendor { get; }

        internal bool Matches(RegisteredComponentIdentity other) =>
            ComponentId.Equals(other.ComponentId) && Vendor.Equals(other.Vendor);

        internal static RegisteredComponentIdentity For(string componentId, string vendor) =>
            new RegisteredComponentIdentity(
                ComponentId.Of(componentId),
                ComponentVendor.From(vendor));

        internal static RegisteredComponentIdentity For(ComponentId componentId, ComponentVendor vendor) =>
            new RegisteredComponentIdentity(componentId, vendor);
    }
}
