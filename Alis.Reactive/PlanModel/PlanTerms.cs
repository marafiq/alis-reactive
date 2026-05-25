using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    internal abstract class PlanString : IEquatable<PlanString>
    {
        protected PlanString(string value, string parameterName)
            : this(value, parameterName, EmptyPlanStringPolicy.Reject)
        {
        }

        protected PlanString(string value, string parameterName, EmptyPlanStringPolicy emptyPolicy)
        {
            if (value == null)
                throw new ArgumentNullException(parameterName);
            if (emptyPolicy == EmptyPlanStringPolicy.Reject && string.IsNullOrWhiteSpace(value))
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
        Reject,
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
        private readonly PlanMergePart _mergePart;

        private PlanIdentity(PlanId planId, PlanMergePart mergePart)
        {
            _planId = planId ?? throw new ArgumentNullException(nameof(planId));
            _mergePart = mergePart ?? throw new ArgumentNullException(nameof(mergePart));
        }

        internal string PlanIdForJson => _planId.Value;

        internal PlanScope ScopeForJson => _mergePart.ScopeForJson;

        internal static PlanIdentity Root(PlanId planId) =>
            new PlanIdentity(planId, PlanMergePart.Root);

        internal static PlanIdentity Partial(PlanId planId) =>
            new PlanIdentity(planId, PlanMergePart.Partial);
    }

    internal abstract class PlanMergePart
    {
        private protected PlanMergePart() { }

        internal static PlanMergePart Root { get; } = new RootPlanMergePart();

        internal static PlanMergePart Partial { get; } = new PartialPlanMergePart();

        internal abstract PlanScope ScopeForJson { get; }

        private sealed class RootPlanMergePart : PlanMergePart
        {
            internal override PlanScope ScopeForJson => PlanScope.Root;
        }

        private sealed class PartialPlanMergePart : PlanMergePart
        {
            internal override PlanScope ScopeForJson => PlanScope.Partial;
        }
    }

    /// <summary>Base class for plan merge scope. Not constructed in application code.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<PlanScope>))]
    public abstract class PlanScope
    {
        private protected PlanScope() { }

        internal static PlanScope Root { get; } = new RootPlanScope();

        internal static PlanScope Partial { get; } = new PartialPlanScope();

        /// <summary>Gets the scope kind.</summary>
        public abstract string Kind { get; }
    }

    /// <summary>Represents a root view plan.</summary>
    public sealed class RootPlanScope : PlanScope
    {
        internal RootPlanScope() { }

        /// <summary>Gets the kind. Always <c>"root"</c>.</summary>
        public override string Kind => "root";
    }

    /// <summary>Represents a partial plan contribution that can be merged into a root plan.</summary>
    public sealed class PartialPlanScope : PlanScope
    {
        internal PartialPlanScope() { }

        /// <summary>Gets the kind. Always <c>"partial"</c>.</summary>
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

    internal sealed class TypeKey : PlanString
    {
        private TypeKey(string value) : base(value, nameof(value)) { }

        internal static TypeKey Of(string value) => new TypeKey(value);
        internal static TypeKey NativeElement(ComponentId componentId) => Of("native.element." + componentId.Value);
        internal static TypeKey Component(ComponentVendor vendor, ComponentId componentId) => Of(vendor.Value + ".component." + componentId.Value);
        internal static TypeKey Plugin(PluginName pluginName) => Of("plugin." + pluginName.Value);
    }

    internal sealed class BindingPath : PlanString
    {
        private BindingPath(string value) : base(value, nameof(value)) { }

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

    internal sealed class MinimumTextLength
    {
        private MinimumTextLength(int value)
        {
            Value = value;
        }

        internal int Value { get; }

        internal static MinimumTextLength From(int length, string parameterName)
        {
            if (parameterName == null) throw new ArgumentNullException(nameof(parameterName));
            if (length < 0)
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    length,
                    "Condition minimum text length must be zero or greater.");

            return new MinimumTextLength(length);
        }
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
            for (var i = 0; i < value.Length; i++)
            {
                if (char.IsWhiteSpace(value[i])) return true;
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

        internal static MemberAccess From(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (Known.TryGetValue(value, out var access)) return access;
            throw new ArgumentException(
                "Unknown member access '" + value + "'. Expected read, write, or readwrite.",
                nameof(value));
        }

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
                { "PATCH", new HttpMethodName("PATCH") },
            };

        private HttpMethodName(string value) : base(value, nameof(value)) { }

        internal static HttpMethodName Get => Known["GET"];
        internal static HttpMethodName Post => Known["POST"];
        internal static HttpMethodName Put => Known["PUT"];
        internal static HttpMethodName Delete => Known["DELETE"];
        internal static HttpMethodName Patch => Known["PATCH"];
        internal static IReadOnlyCollection<string> Values => Known.Keys;

        internal static HttpMethodName From(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (Known.TryGetValue(value, out var method)) return method;
            throw new ArgumentException(
                "Unknown HTTP method '" + value + "'. Expected GET, POST, PUT, DELETE, or PATCH.",
                nameof(value));
        }
    }

    internal sealed class RequestTransport : PlanString
    {
        private static readonly Dictionary<string, RequestTransport> Known =
            new Dictionary<string, RequestTransport>(StringComparer.Ordinal)
            {
                { "json", new RequestTransport("json") },
                { "form-data", new RequestTransport("form-data") },
            };

        private RequestTransport(string value) : base(value, nameof(value)) { }

        internal static RequestTransport Json => Known["json"];
        internal static RequestTransport FormData => Known["form-data"];
        internal static IReadOnlyCollection<string> Values => Known.Keys;

        internal static RequestTransport From(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (Known.TryGetValue(value, out var transport)) return transport;
            throw new ArgumentException(
                "Unknown request transport '" + value + "'. Expected json or form-data.",
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
            };

        private PayloadScope(string value) : base(value, nameof(value)) { }

        internal static PayloadScope Event => Known["event"];
        internal static PayloadScope Success => Known["success"];
        internal static PayloadScope Error => Known["error"];
        internal static PayloadScope Request => Known["request"];
        internal static PayloadScope Dispatch => Known["dispatch"];
        internal static PayloadScope Local => Known["local"];
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

    /// <summary>Base class for payload typing contracts. Not constructed in application code.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<PayloadContract>))]
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

        /// <summary>Gets the payload contract kind.</summary>
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

    internal sealed class CompareOperator : PlanString
    {
        internal static CompareOperator Eq { get; } = new CompareOperator(CompareOp.Eq);
        internal static CompareOperator Neq { get; } = new CompareOperator(CompareOp.Neq);
        internal static CompareOperator Gt { get; } = new CompareOperator(CompareOp.Gt);
        internal static CompareOperator Gte { get; } = new CompareOperator(CompareOp.Gte);
        internal static CompareOperator Lt { get; } = new CompareOperator(CompareOp.Lt);
        internal static CompareOperator Lte { get; } = new CompareOperator(CompareOp.Lte);
        internal static CompareOperator Truthy { get; } = new CompareOperator(CompareOp.Truthy);
        internal static CompareOperator Falsy { get; } = new CompareOperator(CompareOp.Falsy);
        internal static CompareOperator IsNull { get; } = new CompareOperator(CompareOp.IsNull);
        internal static CompareOperator NotNull { get; } = new CompareOperator(CompareOp.NotNull);
        internal static CompareOperator IsEmpty { get; } = new CompareOperator(CompareOp.IsEmpty);
        internal static CompareOperator NotEmpty { get; } = new CompareOperator(CompareOp.NotEmpty);
        internal static CompareOperator In { get; } = new CompareOperator(CompareOp.In);
        internal static CompareOperator NotIn { get; } = new CompareOperator(CompareOp.NotIn);
        internal static CompareOperator Between { get; } = new CompareOperator(CompareOp.Between);
        internal static CompareOperator Contains { get; } = new CompareOperator(CompareOp.Contains);
        internal static CompareOperator StartsWith { get; } = new CompareOperator(CompareOp.StartsWith);
        internal static CompareOperator EndsWith { get; } = new CompareOperator(CompareOp.EndsWith);
        internal static CompareOperator Matches { get; } = new CompareOperator(CompareOp.Matches);
        internal static CompareOperator MinLength { get; } = new CompareOperator(CompareOp.MinLength);
        internal static CompareOperator ArrayContains { get; } = new CompareOperator(CompareOp.ArrayContains);

        private static readonly Dictionary<string, CompareOperator> Known =
            new Dictionary<string, CompareOperator>(StringComparer.Ordinal)
            {
                { CompareOp.Eq, Eq },
                { CompareOp.Neq, Neq },
                { CompareOp.Gt, Gt },
                { CompareOp.Gte, Gte },
                { CompareOp.Lt, Lt },
                { CompareOp.Lte, Lte },
                { CompareOp.Truthy, Truthy },
                { CompareOp.Falsy, Falsy },
                { CompareOp.IsNull, IsNull },
                { CompareOp.NotNull, NotNull },
                { CompareOp.IsEmpty, IsEmpty },
                { CompareOp.NotEmpty, NotEmpty },
                { CompareOp.In, In },
                { CompareOp.NotIn, NotIn },
                { CompareOp.Between, Between },
                { CompareOp.Contains, Contains },
                { CompareOp.StartsWith, StartsWith },
                { CompareOp.EndsWith, EndsWith },
                { CompareOp.Matches, Matches },
                { CompareOp.MinLength, MinLength },
                { CompareOp.ArrayContains, ArrayContains },
            };

        private CompareOperator(string value) : base(value, nameof(value)) { }

        internal static IReadOnlyCollection<string> Values => Known.Keys;

        internal static CompareOperator From(string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (Known.TryGetValue(value, out var op)) return op;
            throw new ArgumentException("Unknown comparison operator '" + value + "'.", nameof(value));
        }
    }
}
