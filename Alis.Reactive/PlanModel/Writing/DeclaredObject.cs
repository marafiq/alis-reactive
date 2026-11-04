namespace Alis.Reactive.PlanModel.Writing
{
    internal sealed class DeclaredObject
    {
        internal DeclaredObject(string id, ComponentVendor vendor)
        {
            Id = id;
            Vendor = vendor;
        }

        internal string Id { get; }

        internal ComponentVendor Vendor { get; }
    }
}
