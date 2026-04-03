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
        internal ContextRequestValue(string key, string path)
        {
            Key = key;
            Path = path;
        }

        internal string Key { get; }
        internal string Path { get; }
    }

    internal sealed class ComponentRequestValue : RequestValuePart
    {
        internal ComponentRequestValue(string key, string componentId, string vendor, string valueMemberPath)
        {
            Key = key;
            ComponentId = componentId;
            Vendor = vendor;
            ValueMemberPath = valueMemberPath;
        }

        internal string Key { get; }
        internal string ComponentId { get; }
        internal string Vendor { get; }
        internal string ValueMemberPath { get; }
    }
}
