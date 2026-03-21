namespace Alis.Reactive
{
    public sealed class ComponentRegistration
    {
        public string ComponentId { get; }
        public string Vendor { get; }
        public string BindingPath { get; }
        public string ReadExpr { get; }
        public string ComponentType { get; }

        public ComponentRegistration(string componentId, string vendor, string bindingPath, string readExpr, string componentType)
        {
            ComponentId = componentId;
            Vendor = vendor;
            BindingPath = bindingPath;
            ReadExpr = readExpr;
            ComponentType = componentType;
        }
    }
}
