using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Builders.Requests
{
    internal abstract class RequestValuePart { }

    internal sealed class IncludeAllBindingsRequestValue : RequestValuePart { }

    internal sealed class LiteralRequestValue : RequestValuePart
    {
        internal LiteralRequestValue(string key, object value)
        {
            Key = key;
            Value = value;
        }

        internal string Key { get; }
        internal object Value { get; }
    }

    internal sealed class ContextRequestValue : RequestValuePart
    {
        internal ContextRequestValue(string key, ValueExpr value)
        {
            Key = key;
            Value = value;
        }

        internal string Key { get; }
        internal ValueExpr Value { get; }
    }

    internal sealed class ComponentRequestValue : RequestValuePart
    {
        internal ComponentRequestValue(
            string key,
            string componentId,
            ComponentMetadata component,
            CapabilityProperty binding,
            ValueShape? shape = null)
        {
            Key = key;
            ComponentId = componentId;
            Component = component;
            Binding = binding;
            Shape = shape;
        }

        internal string Key { get; }
        internal string ComponentId { get; }
        internal ComponentMetadata Component { get; }
        internal CapabilityProperty Binding { get; }
        internal ValueShape? Shape { get; }
    }
}
