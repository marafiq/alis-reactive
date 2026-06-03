namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Identifies the plan contract key for a DOM element, component, layout object,
    /// or plugin.
    /// </summary>
    internal sealed class BrowserObjectId : PlanString
    {
        private BrowserObjectId(string value) : base(value, nameof(value)) { }

        internal static BrowserObjectId Of(string value) => new BrowserObjectId(value);
        internal static BrowserObjectId NativeElement(ComponentId componentId) => Of("native.element." + componentId.Value);
        internal static BrowserObjectId ComponentObject(ComponentVendor vendor, ComponentId componentId) => Of(vendor.Value + ".component." + componentId.Value);
        internal static BrowserObjectId Plugin(PluginName pluginName) => Of("plugin." + pluginName.Value);
    }
}
