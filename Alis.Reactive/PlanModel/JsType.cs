using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class JsType
    {
        public Dictionary<string, JsProperty> Properties { get; internal set; }
        public Dictionary<string, JsMethod> Methods { get; internal set; }
        public Dictionary<string, JsEvent> Events { get; internal set; }
        public DefaultValue DefaultValue { get; internal set; }

        internal JsType() { }

        internal JsType WithProperty(string name, Path path, Shape shape, string access)
        {
            Properties ??= new Dictionary<string, JsProperty>();
            Properties[name] = new JsProperty(path, shape, access);
            return this;
        }

        internal JsType WithMethod(string name, Path path, List<Shape> args = null, Shape returns = null)
        {
            Methods ??= new Dictionary<string, JsMethod>();
            Methods[name] = new JsMethod(path, args, returns);
            return this;
        }

        internal JsType WithEvent(string name, string channel, string payloadType = null)
        {
            Events ??= new Dictionary<string, JsEvent>();
            Events[name] = new JsEvent(channel, payloadType);
            return this;
        }

        internal JsType WithDefaultValue(string member, Shape shape)
        {
            DefaultValue = new DefaultValue("property", member, shape);
            return this;
        }

        internal JsType WithDefaultMethod(string member, Shape shape)
        {
            DefaultValue = new DefaultValue("method", member, shape);
            return this;
        }
    }

    internal sealed class JsProperty
    {
        public Path Path { get; }
        public Shape Shape { get; }
        public string Access { get; }

        internal JsProperty(Path path, Shape shape, string access)
        {
            Path = path;
            Shape = shape;
            Access = access;
        }
    }

    internal sealed class JsMethod
    {
        public Path Path { get; }
        public List<Shape> Args { get; }
        public Shape Returns { get; }

        internal JsMethod(Path path, List<Shape> args, Shape returns)
        {
            Path = path;
            Args = args != null && args.Count > 0 ? args : null;
            Returns = returns == null || returns.IsNone ? null : returns;
        }
    }

    internal sealed class JsEvent
    {
        public string Channel { get; }
        public string PayloadType { get; }

        internal JsEvent(string channel, string payloadType)
        {
            Channel = channel;
            PayloadType = payloadType;
        }
    }

    internal sealed class DefaultValue
    {
        public string Kind { get; }
        public string Member { get; }
        public Shape Shape { get; }

        internal DefaultValue(string kind, string member, Shape shape)
        {
            Kind = kind;
            Member = member;
            Shape = shape;
        }
    }
}
