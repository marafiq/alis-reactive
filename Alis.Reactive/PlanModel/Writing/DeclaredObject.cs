namespace Alis.Reactive.PlanModel
{
    /// <summary>An element the plan names, with the vendor whose object it holds.</summary>
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
