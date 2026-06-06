using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    internal abstract class PlanString : IEquatable<PlanString>
    {
        protected PlanString(string value, string parameterName)
            : this(value, parameterName, EmptyPlanStringPolicy.Disallow)
        {
        }

        protected PlanString(string value, string parameterName, EmptyPlanStringPolicy emptyPolicy)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);
            if (emptyPolicy == EmptyPlanStringPolicy.Disallow && string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(parameterName + " must not be empty.", parameterName);

            Value = value;
        }

        internal string Value { get; }

        public bool Equals(PlanString? other) =>
            other != null && GetType() == other.GetType() && Value == other.Value;

        public override bool Equals(object? obj) => Equals(obj as PlanString);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((GetType().GetHashCode() * 397) ^ Value.GetHashCode());
            }
        }

        public override string ToString() => Value;
    }

    internal enum EmptyPlanStringPolicy
    {
        Disallow,
        Allow
    }

    internal sealed class PlanId : PlanString
    {
        private PlanId(string value) : base(value, nameof(value)) { }

        internal static PlanId ForModel(Type modelType)
        {
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            return new PlanId(modelType.FullName
                ?? throw new ArgumentException("Model type must have a full name.", nameof(modelType)));
        }

        internal static PlanId Of(string value) => new PlanId(value);
    }

    internal sealed class PlanIdentity
    {
        private readonly PlanId _planId;
        private readonly PlanScope _scope;

        private PlanIdentity(PlanId planId, PlanScope scope)
        {
            _planId = planId ?? throw new ArgumentNullException(nameof(planId));
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
        }

        internal string PlanIdForJson => _planId.Value;

        internal PlanScope ScopeForJson => _scope;

        internal static PlanIdentity Root(PlanId planId) =>
            new PlanIdentity(planId, PlanScope.Root);

        internal static PlanIdentity Partial(PlanId planId) =>
            new PlanIdentity(planId, PlanScope.Partial);
    }

    /// <summary>Wire base for plan merge scope emitted by root and partial plans.</summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<PlanScope>))]
    public abstract class PlanScope
    {
        private protected PlanScope() { }

        internal static PlanScope Root { get; } = new RootPlanScope();

        internal static PlanScope Partial { get; } = new PartialPlanScope();

        /// <summary>JSON discriminator for plan merge scope.</summary>
        public abstract string Kind { get; }
    }

    /// <summary>Plan scope for the root view plan.</summary>
    public sealed class RootPlanScope : PlanScope
    {
        internal RootPlanScope() { }

        /// <summary>JSON discriminator for root view plans. Always <c>"root"</c>.</summary>
        public override string Kind => "root";
    }

    /// <summary>Plan scope for a partial plan that can be loaded into a DOM slot.</summary>
    public sealed class PartialPlanScope : PlanScope
    {
        internal PartialPlanScope() { }

        /// <summary>JSON discriminator for partial plans. Always <c>"partial"</c>.</summary>
        public override string Kind => "partial";
    }

    internal sealed class ComponentId : PlanString
    {
        private ComponentId(string value) : base(value, nameof(value)) { }

        internal static ComponentId Of(string value) => new ComponentId(value);
    }

    internal sealed class ComponentKey : PlanString
    {
        private ComponentKey(string value) : base(value, nameof(value)) { }

        internal static ComponentKey Of(string value) => new ComponentKey(value);
    }

    internal sealed class BindingPath : PlanString
    {
        private readonly Path _path;

        private BindingPath(string value) : base(value, nameof(value))
        {
            _path = Path.Parse(value);
        }

        public Path Path => _path;

        internal static BindingPath Of(string value) => new BindingPath(value);
    }

    internal sealed class MemberName : PlanString
    {
        private MemberName(string value) : base(value, nameof(value)) { }

        internal static MemberName Of(string value) => new MemberName(value);
    }

    internal sealed class ComponentKind : PlanString
    {
        private ComponentKind(string value) : base(value, nameof(value)) { }

        internal static ComponentKind Of(string value) => new ComponentKind(value);
    }

    internal sealed class EventName : PlanString
    {
        private EventName(string value) : base(value, nameof(value)) { }

        internal static EventName Of(string value) => new EventName(value);
    }

    internal sealed class PluginName : PlanString
    {
        private PluginName(string value) : base(value, nameof(value))
        {
            if (HasWhitespace(value))
                throw new ArgumentException("Plugin name must not contain whitespace.", nameof(value));
        }

        internal static PluginName Of(string value) => new PluginName(value);

        private static bool HasWhitespace(string value)
        {
            for (var characterIndex = 0; characterIndex < value.Length; characterIndex++)
            {
                if (char.IsWhiteSpace(value[characterIndex])) return true;
            }

            return false;
        }
    }

    internal sealed class RequestUrl : PlanString
    {
        private RequestUrl(string value) : base(value, nameof(value), EmptyPlanStringPolicy.Allow) { }

        internal static RequestUrl Of(string value) => new RequestUrl(value);
    }

    internal sealed class HeaderName : PlanString
    {
        private HeaderName(string value) : base(value, nameof(value)) { }

        internal static HeaderName Of(string value) => new HeaderName(value);
    }

    internal sealed class RouteParameterName : PlanString
    {
        private static readonly System.Text.RegularExpressions.Regex Pattern =
            new System.Text.RegularExpressions.Regex(
                @"^[a-zA-Z0-9_]+$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        private RouteParameterName(string value) : base(value, nameof(value))
        {
            var hasInvalidCharacters = !Pattern.IsMatch(value);
            if (hasInvalidCharacters)
                throw new ArgumentException(
                    "Route param name '" + value + "' contains invalid characters. " +
                    "Names must match [a-zA-Z0-9_] (ASCII only) to align with URL template placeholders.",
                    nameof(value));
        }

        internal static RouteParameterName Of(string value) => new RouteParameterName(value);
    }

    internal sealed class ComponentVendor : PlanString
    {
        private static readonly System.Text.RegularExpressions.Regex TokenPattern =
            new System.Text.RegularExpressions.Regex(
                @"^[a-zA-Z][a-zA-Z0-9_-]*$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        private ComponentVendor(string value) : base(value, nameof(value))
        {
            var tokenHasInvalidCharacters = !TokenPattern.IsMatch(value);
            if (tokenHasInvalidCharacters)
                throw new ArgumentException(
                    "Component vendor token '" + value + "' contains invalid characters. " +
                    "Vendor tokens must start with a letter and contain only ASCII letters, digits, underscore, or hyphen.",
                    nameof(value));
        }

        internal static ComponentVendor Native { get; } = new ComponentVendor("native");
        internal static ComponentVendor Fusion { get; } = new ComponentVendor("fusion");

        internal static ComponentVendor From(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (Native.Value == value) return Native;
            if (Fusion.Value == value) return Fusion;
            return new ComponentVendor(value);
        }
    }

    internal sealed class MemberAccess : PlanString
    {
        private static readonly Dictionary<string, MemberAccess> Known =
            new Dictionary<string, MemberAccess>(StringComparer.Ordinal)
            {
                { "read", new MemberAccess("read") },
                { "write", new MemberAccess("write") },
                { "readwrite", new MemberAccess("readwrite") },
            };

        private MemberAccess(string value) : base(value, nameof(value)) { }

        internal static MemberAccess Read => Known["read"];
        internal static MemberAccess Write => Known["write"];
        internal static MemberAccess ReadWrite => Known["readwrite"];
        internal static IReadOnlyCollection<string> Values => Known.Keys;

        internal MemberAccess Widen(MemberAccess incoming)
        {
            if (incoming == null) throw new ArgumentNullException(nameof(incoming));
            if (this == ReadWrite || incoming == ReadWrite) return ReadWrite;
            if ((this == Read && incoming == Write) || (this == Write && incoming == Read))
                return ReadWrite;
            if (this == incoming) return this;

            throw new InvalidOperationException(
                "Cannot combine member access '" + Value + "' with '" + incoming.Value + "'.");
        }
    }

    internal sealed class HttpMethodName : PlanString
    {
        private static readonly Dictionary<string, HttpMethodName> Known =
            new Dictionary<string, HttpMethodName>(StringComparer.Ordinal)
            {
                { "GET", new HttpMethodName("GET") },
                { "POST", new HttpMethodName("POST") },
                { "PUT", new HttpMethodName("PUT") },
                { "DELETE", new HttpMethodName("DELETE") },
            };

        private HttpMethodName(string value) : base(value, nameof(value)) { }

        internal static HttpMethodName Get => Known["GET"];
        internal static HttpMethodName Post => Known["POST"];
        internal static HttpMethodName Put => Known["PUT"];
        internal static HttpMethodName Delete => Known["DELETE"];
        internal static IReadOnlyCollection<string> Values => Known.Keys;

        internal static HttpMethodName From(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (Known.TryGetValue(value, out var method)) return method;
            throw new ArgumentException(
                "Unknown HTTP method '" + value + "'. Expected GET, POST, PUT, or DELETE.",
                nameof(value));
        }
    }

    internal sealed class RequestBodyFormat : PlanString
    {
        private static readonly Dictionary<string, RequestBodyFormat> Known =
            new Dictionary<string, RequestBodyFormat>(StringComparer.Ordinal)
            {
                { "json", new RequestBodyFormat("json") },
                { "form-data", new RequestBodyFormat("form-data") },
            };

        private RequestBodyFormat(string value) : base(value, nameof(value)) { }

        internal static RequestBodyFormat Json => Known["json"];
        internal static RequestBodyFormat FormData => Known["form-data"];
        internal static IReadOnlyCollection<string> Values => Known.Keys;

        internal static RequestBodyFormat From(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (Known.TryGetValue(value, out var bodyFormat)) return bodyFormat;
            throw new ArgumentException(
                "Unknown request body format '" + value + "'. Expected json or form-data.",
                nameof(value));
        }
    }

    internal sealed class HttpResponseStatusCode
    {
        private const int MinimumStandardStatusCode = 100;
        private const int MaximumStandardStatusCode = 599;

        private HttpResponseStatusCode(int value)
        {
            Value = value;
        }

        internal int Value { get; }

        internal static HttpResponseStatusCode FromDeveloperStatus(int value)
        {
            var isStandardHttpStatusCode =
                value >= MinimumStandardStatusCode &&
                value <= MaximumStandardStatusCode;
            if (isStandardHttpStatusCode) return new HttpResponseStatusCode(value);

            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "HTTP response status code must be between 100 and 599. " +
                "Use OnError(...) without a status code for network/client failures.");
        }
    }

    internal sealed class PayloadScope : PlanString
    {
        private static readonly Dictionary<string, PayloadScope> Known =
            new Dictionary<string, PayloadScope>(StringComparer.Ordinal)
            {
                { "event", new PayloadScope("event") },
                { "success", new PayloadScope("success") },
                { "error", new PayloadScope("error") },
                { "request", new PayloadScope("request") },
                { "dispatch", new PayloadScope("dispatch") },
                { "local", new PayloadScope("local") },
                { "element", new PayloadScope("element") },
            };

        private PayloadScope(string value) : base(value, nameof(value)) { }

        internal static PayloadScope Event => Known["event"];
        internal static PayloadScope Success => Known["success"];
        internal static PayloadScope Error => Known["error"];
        internal static PayloadScope Request => Known["request"];
        internal static PayloadScope Dispatch => Known["dispatch"];
        internal static PayloadScope Local => Known["local"];
        internal static PayloadScope Element => Known["element"];
        internal static IReadOnlyCollection<string> Values => Known.Keys;

        internal static PayloadScope From(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (Known.TryGetValue(value, out var scope)) return scope;
            throw new ArgumentException(
                "Unknown payload scope '" + value + "'.",
                nameof(value));
        }
    }

    /// <summary>Wire base for payload typing contracts authored by typed triggers.</summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<PayloadContract>))]
    public abstract class PayloadContract
    {
        private protected PayloadContract() { }

        internal static PayloadContract Untyped { get; } = new UntypedPayloadContract();

        internal static PayloadContract Named(string value) => new NamedPayloadContract(value);

        internal static PayloadContract ForPayload(Type payloadType)
        {
            if (payloadType == null) throw new ArgumentNullException(nameof(payloadType));
            return Named(payloadType.FullName
                ?? throw new ArgumentException("Payload type must have a full name.", nameof(payloadType)));
        }

        /// <summary>JSON discriminator for payload typing contracts.</summary>
        public abstract string Kind { get; }

        internal abstract string DisplayName { get; }

        internal abstract bool SameAs(PayloadContract other);
    }

    internal sealed class UntypedPayloadContract : PayloadContract
    {
        public override string Kind => "untyped";

        internal override string DisplayName => "<untyped>";

        internal override bool SameAs(PayloadContract other) =>
            other is UntypedPayloadContract;
    }

    internal sealed class NamedPayloadContract : PayloadContract
    {
        private readonly PlanString _name;

        internal NamedPayloadContract(string value)
        {
            _name = PayloadTypeName.Of(value);
        }

        public override string Kind => "typed";

        public string Type => _name.Value;

        internal override string DisplayName => _name.Value;

        internal override bool SameAs(PayloadContract other) =>
            other is NamedPayloadContract named && named._name.Equals(_name);
    }

    internal sealed class PayloadTypeName : PlanString
    {
        private PayloadTypeName(string value) : base(value, nameof(value)) { }

        internal static PayloadTypeName Of(string value) => new PayloadTypeName(value);
    }
}
