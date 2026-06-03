namespace Alis.Reactive.PlanModel
{
    /// <summary>Identifies one browser object: the <c>(vendor, kind, id)</c> token a
    /// component's member contract is keyed by. Was <c>TypeKey</c>; renamed because
    /// "TypeKey" never said what it keyed.</summary>
    internal sealed class BrowserObjectId : PlanString
    {
        private BrowserObjectId(string value) : base(value, nameof(value)) { }

        internal static BrowserObjectId Of(string value) => new BrowserObjectId(value);
        internal static BrowserObjectId NativeElement(ComponentId componentId) => Of("native.element." + componentId.Value);
        internal static BrowserObjectId ComponentObject(ComponentVendor vendor, ComponentId componentId) => Of(vendor.Value + ".component." + componentId.Value);
        internal static BrowserObjectId Plugin(PluginName pluginName) => Of("plugin." + pluginName.Value);
    }
}
