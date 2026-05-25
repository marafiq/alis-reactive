using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>An HTTP request definition in the reactive plan.</summary>
    public sealed class Request
    {
        private readonly RequestEndpoint _endpoint;
        private readonly RequestPayload _payload;
        private readonly RequestLifecycle _lifecycle;
        private readonly RequestParameters _parameters;
        private readonly RequestValidationTarget _validationTarget;

        /// <summary>Gets the HTTP method (GET, POST, PUT, DELETE, PATCH).</summary>
        public string Method => _endpoint.Method.Value;
        /// <summary>Gets the request URL, which may contain template placeholders like <c>{id}</c>.</summary>
        public string Url => _endpoint.Url.Value;
        /// <summary>Gets the validation target used before sending this request.</summary>
        public RequestValidationTarget Validation => _validationTarget;
        /// <summary>Gets the request body strategy. Bodiless requests use <see cref="NoRequestInput"/>.</summary>
        public RequestInput Input => _payload.InputForJson;
        /// <summary>Gets reactions to execute before the request is sent.</summary>
        public IReadOnlyList<Reaction> Before => _lifecycle.Before;
        /// <summary>Gets the success response handlers.</summary>
        public IReadOnlyList<ResponseHandler> Success => _lifecycle.Success;
        /// <summary>Gets the error response handlers.</summary>
        public IReadOnlyList<ResponseHandler> Error => _lifecycle.Error;
        /// <summary>Gets reactions to execute after the request completes regardless of outcome.</summary>
        public IReadOnlyList<Reaction> Complete => _lifecycle.Complete;
        /// <summary>Gets the request chain: terminal or followed by another request.</summary>
        public RequestChain Chain => _lifecycle.Chain;
        /// <summary>Gets the custom HTTP headers. Each value is evaluated at request time.</summary>
        public IReadOnlyDictionary<string, ValueProducer> Headers => _parameters.Headers;
        /// <summary>Gets the URL template parameters. Each value is evaluated and URI-encoded before replacing placeholders in the URL.</summary>
        public IReadOnlyDictionary<string, ValueProducer> RouteParams => _parameters.RouteParams;

        internal RequestPayload Payload => _payload;

        private Request(
            RequestEndpoint endpoint,
            RequestPayload payload,
            RequestLifecycle lifecycle,
            RequestParameters parameters,
            RequestValidationTarget validationTarget)
        {
            _endpoint = endpoint ?? throw new System.ArgumentNullException(nameof(endpoint));
            _payload = payload ?? throw new System.ArgumentNullException(nameof(payload));
            _lifecycle = lifecycle ?? throw new System.ArgumentNullException(nameof(lifecycle));
            _parameters = parameters ?? throw new System.ArgumentNullException(nameof(parameters));
            _validationTarget = validationTarget ?? throw new System.ArgumentNullException(nameof(validationTarget));
        }

        internal static Request Create(
            RequestEndpoint endpoint,
            RequestPayload payload,
            RequestLifecycle lifecycle,
            RequestParameters parameters,
            RequestValidationTarget validationTarget) =>
            new Request(endpoint, payload, lifecycle, parameters, validationTarget);

        /// <summary>Returns a copy of this request with <see cref="Before"/> replaced.</summary>
        internal Request WithBefore(IReadOnlyList<Reaction> before) =>
            new Request(
                _endpoint,
                _payload,
                _lifecycle.WithBefore(before),
                _parameters,
                _validationTarget);
    }

    internal sealed class RequestEndpoint
    {
        private RequestEndpoint(HttpMethodName method, RequestUrl url)
        {
            Method = method ?? throw new System.ArgumentNullException(nameof(method));
            Url = url ?? throw new System.ArgumentNullException(nameof(url));
        }

        internal HttpMethodName Method { get; }
        internal RequestUrl Url { get; }

        internal static RequestEndpoint To(HttpMethodName method, RequestUrl url) =>
            new RequestEndpoint(method, url);
    }

    internal abstract class RequestPayload
    {
        private RequestPayload() { }

        internal static RequestPayload None { get; } = new BodilessRequestPayload();

        internal abstract RequestInput InputForJson { get; }

        internal static RequestPayload Send(RequestInput input)
        {
            if (input == null) throw new System.ArgumentNullException(nameof(input));
            return new BodyRequestPayload(input);
        }

        private sealed class BodilessRequestPayload : RequestPayload
        {
            internal override RequestInput InputForJson => RequestInput.None;
        }

        private sealed class BodyRequestPayload : RequestPayload
        {
            private readonly RequestInput _input;

            internal BodyRequestPayload(RequestInput input)
            {
                _input = input ?? throw new System.ArgumentNullException(nameof(input));
            }

            internal override RequestInput InputForJson => _input;
        }
    }

    internal sealed class RequestLifecycle
    {
        private readonly RequestReactionStages _stages;
        private readonly RequestChain _chain;

        private RequestLifecycle(
            RequestReactionStages stages,
            RequestChain chain)
        {
            _stages = stages ?? throw new System.ArgumentNullException(nameof(stages));
            _chain = chain ?? throw new System.ArgumentNullException(nameof(chain));
        }

        internal IReadOnlyList<Reaction> Before => _stages.Before;
        internal IReadOnlyList<ResponseHandler> Success => _stages.Success;
        internal IReadOnlyList<ResponseHandler> Error => _stages.Error;
        internal IReadOnlyList<Reaction> Complete => _stages.Complete;
        internal RequestChain Chain => _chain;

        internal static RequestLifecycle Create(
            RequestReactionStages stages,
            RequestChain chain) =>
            new RequestLifecycle(stages, chain);

        internal RequestLifecycle WithBefore(IReadOnlyList<Reaction> before) =>
            new RequestLifecycle(_stages.WithBefore(before), _chain);
    }

    internal sealed class RequestReactionStages
    {
        private readonly RequestReactionList _before;
        private readonly ResponseHandlerList _success;
        private readonly ResponseHandlerList _error;
        private readonly RequestReactionList _complete;

        private RequestReactionStages(
            RequestReactionList before,
            ResponseHandlerList success,
            ResponseHandlerList error,
            RequestReactionList complete)
        {
            _before = before ?? throw new System.ArgumentNullException(nameof(before));
            _success = success ?? throw new System.ArgumentNullException(nameof(success));
            _error = error ?? throw new System.ArgumentNullException(nameof(error));
            _complete = complete ?? throw new System.ArgumentNullException(nameof(complete));
        }

        internal IReadOnlyList<Reaction> Before => _before.ForJson;
        internal IReadOnlyList<ResponseHandler> Success => _success.ForJson;
        internal IReadOnlyList<ResponseHandler> Error => _error.ForJson;
        internal IReadOnlyList<Reaction> Complete => _complete.ForJson;

        internal static RequestReactionStages From(
            IReadOnlyList<Reaction> before,
            IReadOnlyList<ResponseHandler> success,
            IReadOnlyList<ResponseHandler> error,
            IReadOnlyList<Reaction> complete) =>
            new RequestReactionStages(
                RequestReactionList.From(before),
                ResponseHandlerList.From(success),
                ResponseHandlerList.From(error),
                RequestReactionList.From(complete));

        internal RequestReactionStages WithBefore(IReadOnlyList<Reaction> before) =>
            new RequestReactionStages(
                RequestReactionList.From(before),
                _success,
                _error,
                _complete);

        internal RequestReactionStages WithoutCompletionStage() =>
            new RequestReactionStages(
                _before,
                _success,
                _error,
                RequestReactionList.Empty);
    }

    internal sealed class RequestReactionList
    {
        private readonly IReadOnlyList<Reaction> _items;

        private RequestReactionList(IReadOnlyList<Reaction> items)
        {
            _items = items;
        }

        internal IReadOnlyList<Reaction> ForJson => _items;

        internal static RequestReactionList Empty { get; } =
            new RequestReactionList(System.Array.Empty<Reaction>());

        internal static RequestReactionList From(IEnumerable<Reaction> items)
        {
            if (items == null) throw new System.ArgumentNullException(nameof(items));

            var snapshot = new List<Reaction>();
            foreach (var item in items)
            {
                if (item == null)
                    throw new System.ArgumentException("Request reaction must not be null.", nameof(items));

                snapshot.Add(item);
            }

            var hasNoReactions = snapshot.Count == 0;
            if (hasNoReactions) return Empty;
            return new RequestReactionList(snapshot);
        }
    }

    internal sealed class ResponseHandlerList
    {
        private readonly IReadOnlyList<ResponseHandler> _items;

        private ResponseHandlerList(IReadOnlyList<ResponseHandler> items)
        {
            _items = items;
        }

        internal IReadOnlyList<ResponseHandler> ForJson => _items;

        internal static ResponseHandlerList Empty { get; } =
            new ResponseHandlerList(System.Array.Empty<ResponseHandler>());

        internal static ResponseHandlerList From(IEnumerable<ResponseHandler> items)
        {
            if (items == null) throw new System.ArgumentNullException(nameof(items));

            var snapshot = new List<ResponseHandler>();
            foreach (var item in items)
            {
                if (item == null)
                    throw new System.ArgumentException("Response handler must not be null.", nameof(items));

                snapshot.Add(item);
            }

            var hasNoHandlers = snapshot.Count == 0;
            if (hasNoHandlers) return Empty;
            return new ResponseHandlerList(snapshot);
        }
    }

    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<RequestChain>))]
    public abstract class RequestChain
    {
        private protected RequestChain() { }

        internal static RequestChain Terminal { get; } = new TerminalRequestChain();

        /// <summary>Gets the chain kind.</summary>
        public abstract string Kind { get; }

        internal abstract RequestChain AttachFollowUp(Request next);

        internal static RequestChain ContinueWith(Request next)
        {
            if (next == null) throw new System.ArgumentNullException(nameof(next));
            return new FollowUpRequestChain(next);
        }
    }

    /// <summary>Represents a request with no chained follow-up request.</summary>
    public sealed class TerminalRequestChain : RequestChain
    {
        /// <summary>Gets the kind. Always <c>"terminal"</c>.</summary>
        public override string Kind => "terminal";

        internal override RequestChain AttachFollowUp(Request next) =>
            ContinueWith(next);
    }

    /// <summary>Represents a request followed by another request after successful completion.</summary>
    public sealed class FollowUpRequestChain : RequestChain
    {
        private readonly Request _next;

        internal FollowUpRequestChain(Request next)
        {
            _next = next ?? throw new System.ArgumentNullException(nameof(next));
        }

        /// <summary>Gets the kind. Always <c>"follow-up"</c>.</summary>
        public override string Kind => "follow-up";

        /// <summary>Gets the request to run after the current request succeeds.</summary>
        public Request Next => _next;

        internal override RequestChain AttachFollowUp(Request next)
        {
            throw new System.InvalidOperationException(
                "A response can declare only one chained request. " +
                "To continue the sequence, attach the next Chained request to the existing follow-up request.");
        }
    }

    internal sealed class RequestParameters
    {
        private readonly RequestHeaders _headers;
        private readonly RequestRouteParameters _routeParams;

        private RequestParameters(
            RequestHeaders headers,
            RequestRouteParameters routeParams)
        {
            _headers = headers ?? throw new System.ArgumentNullException(nameof(headers));
            _routeParams = routeParams ?? throw new System.ArgumentNullException(nameof(routeParams));
        }

        internal IReadOnlyDictionary<string, ValueProducer> Headers => _headers.ForJson;
        internal IReadOnlyDictionary<string, ValueProducer> RouteParams => _routeParams.ForJson;

        internal static RequestParameters From(
            IReadOnlyDictionary<string, ValueProducer> headers,
            IReadOnlyDictionary<string, ValueProducer> routeParams) =>
            new RequestParameters(
                RequestHeaders.From(headers),
                RequestRouteParameters.From(routeParams));
    }

    internal sealed class RequestHeaders
    {
        private readonly IReadOnlyDictionary<string, ValueProducer> _headers;

        private RequestHeaders(IReadOnlyDictionary<string, ValueProducer> headers)
        {
            _headers = headers;
        }

        internal IReadOnlyDictionary<string, ValueProducer> ForJson => _headers;

        internal static RequestHeaders Empty { get; } =
            new RequestHeaders(new Dictionary<string, ValueProducer>());

        internal static RequestHeaders From(IReadOnlyDictionary<string, ValueProducer> headers)
        {
            if (headers == null) throw new System.ArgumentNullException(nameof(headers));

            var snapshot = new Dictionary<string, ValueProducer>(System.StringComparer.Ordinal);
            foreach (var header in headers)
                snapshot[HeaderName.Of(header.Key).Value] = RequireValue(header.Value, header.Key);

            var requestHasNoHeaders = snapshot.Count == 0;
            if (requestHasNoHeaders) return Empty;
            return new RequestHeaders(snapshot);
        }

        private static ValueProducer RequireValue(ValueProducer value, string header)
        {
            if (value == null)
                throw new System.ArgumentException(
                    "Request header '" + header + "' must have a value producer.",
                    nameof(value));

            return value;
        }
    }

    internal sealed class RequestRouteParameters
    {
        private readonly IReadOnlyDictionary<string, ValueProducer> _routeParams;

        private RequestRouteParameters(IReadOnlyDictionary<string, ValueProducer> routeParams)
        {
            _routeParams = routeParams;
        }

        internal IReadOnlyDictionary<string, ValueProducer> ForJson => _routeParams;

        internal static RequestRouteParameters Empty { get; } =
            new RequestRouteParameters(new Dictionary<string, ValueProducer>());

        internal static RequestRouteParameters From(IReadOnlyDictionary<string, ValueProducer> routeParams)
        {
            if (routeParams == null) throw new System.ArgumentNullException(nameof(routeParams));

            var snapshot = new Dictionary<string, ValueProducer>(System.StringComparer.Ordinal);
            foreach (var routeParam in routeParams)
                snapshot[RouteParameterName.Of(routeParam.Key).Value] = RequireValue(routeParam.Value, routeParam.Key);

            var requestHasNoRouteParameters = snapshot.Count == 0;
            if (requestHasNoRouteParameters) return Empty;
            return new RequestRouteParameters(snapshot);
        }

        private static ValueProducer RequireValue(ValueProducer value, string routeParam)
        {
            if (value == null)
                throw new System.ArgumentException(
                    "Route parameter '" + routeParam + "' must have a value producer.",
                    nameof(value));

            return value;
        }
    }

    /// <summary>Base class for request validation targets. Not constructed in application code.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<RequestValidationTarget>))]
    public abstract class RequestValidationTarget
    {
        private protected RequestValidationTarget() { }

        internal static RequestValidationTarget None { get; } =
            new NoRequestValidationTarget();

        /// <summary>Gets the validation target kind.</summary>
        public abstract string Kind { get; }

        internal static RequestValidationTarget DisplayIn(ComponentId container)
        {
            if (container == null) throw new System.ArgumentNullException(nameof(container));
            return new ContainerRequestValidationTarget(container);
        }
    }

    /// <summary>Represents a request that does not run client validation before sending.</summary>
    public sealed class NoRequestValidationTarget : RequestValidationTarget
    {
        /// <summary>Gets the kind. Always <c>"none"</c>.</summary>
        public override string Kind => "none";
    }

    /// <summary>Represents a request that validates a component container before sending.</summary>
    public sealed class ContainerRequestValidationTarget : RequestValidationTarget
    {
        private readonly ComponentId _container;

        internal ContainerRequestValidationTarget(ComponentId container)
        {
            _container = container ?? throw new System.ArgumentNullException(nameof(container));
        }

        /// <summary>Gets the kind. Always <c>"container"</c>.</summary>
        public override string Kind => "container";

        /// <summary>Gets the container component ID to validate.</summary>
        public string Container => _container.Value;
    }

    /// <summary>Base class for request body strategies. Not constructed in application code.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<RequestInput>))]
    public abstract class RequestInput
    {
        private protected RequestInput() { }

        internal static RequestInput None { get; } = new NoRequestInput();
    }

    /// <summary>Represents a request with no body or gathered input.</summary>
    public sealed class NoRequestInput : RequestInput
    {
        /// <summary>Gets the kind. Always <c>"none"</c>.</summary>
        public string Kind => "none";
    }

    [JsonConverter(typeof(GatherInputJsonConverter))]
    internal sealed class GatherInput : RequestInput
    {
        private readonly GatherPayloadFieldList _payloadFields;
        private readonly RequestTransport _transport;
        private readonly SupplementalGatherFields _supplementalFields;
        private readonly GatherSelection _selection;

        public string Kind => "gather";
        public IReadOnlyList<GatherPayloadField> PayloadFields => _payloadFields.ForJson;
        public string Transport => _transport.Value;
        public SupplementalGatherFields SupplementalFields => _supplementalFields;
        public GatherSelection Selection => _selection;

        private GatherInput(
            GatherPayloadFieldList payloadFields,
            RequestTransport transport,
            SupplementalGatherFields supplementalFields,
            GatherSelection selection)
        {
            _payloadFields = payloadFields ?? throw new System.ArgumentNullException(nameof(payloadFields));
            _transport = transport ?? throw new System.ArgumentNullException(nameof(transport));
            _supplementalFields = supplementalFields ?? throw new System.ArgumentNullException(nameof(supplementalFields));
            _selection = selection ?? throw new System.ArgumentNullException(nameof(selection));
        }

        internal static GatherInput From(
            IEnumerable<GatherPayloadField> payloadFields,
            RequestTransport transport,
            SupplementalGatherFields supplementalFields,
            GatherSelection selection) =>
            new GatherInput(
                GatherPayloadFieldList.From(payloadFields),
                transport,
                supplementalFields,
                selection);
    }

    internal sealed class GatherInputJsonConverter : JsonConverter<GatherInput>
    {
        public override void Write(Utf8JsonWriter writer, GatherInput value, JsonSerializerOptions options)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            WriteProperty(writer, options, "payloadFields", value.PayloadFields);
            writer.WriteString("transport", value.Transport);
            WriteProperty(writer, options, "supplementalFields", value.SupplementalFields);
            WriteProperty(writer, options, "selection", value.Selection);
            writer.WriteEndObject();
        }

        public override GatherInput Read(
            ref Utf8JsonReader reader,
            System.Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new System.NotSupportedException("Plan types are write-only.");

        private static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    internal sealed class GatherPayloadFieldList
    {
        private readonly IReadOnlyList<GatherPayloadField> _fields;

        private GatherPayloadFieldList(IReadOnlyList<GatherPayloadField> fields)
        {
            _fields = fields;
        }

        internal IReadOnlyList<GatherPayloadField> ForJson => _fields;

        internal static GatherPayloadFieldList From(IEnumerable<GatherPayloadField> fields)
        {
            if (fields == null) throw new System.ArgumentNullException(nameof(fields));

            var snapshot = new List<GatherPayloadField>();
            foreach (var field in fields)
            {
                if (field == null)
                    throw new System.ArgumentException("Gather payload field must not be null.", nameof(fields));

                snapshot.Add(field);
            }

            return new GatherPayloadFieldList(snapshot);
        }
    }

    [JsonConverter(typeof(SupplementalGatherFieldsJsonConverter))]
    internal abstract class SupplementalGatherFields
    {
        private SupplementalGatherFields() { }

        internal static SupplementalGatherFields None { get; } = new NoSupplementalGatherFields();

        public abstract string Kind { get; }
        internal abstract void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options);

        internal static SupplementalGatherFields From(ObjectProducer value)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));
            return new DeclaredSupplementalGatherFields(value);
        }

        private sealed class NoSupplementalGatherFields : SupplementalGatherFields
        {
            public override string Kind => "none";

            internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options)
            {
            }
        }

        private sealed class DeclaredSupplementalGatherFields : SupplementalGatherFields
        {
            private readonly ObjectProducer _value;

            internal DeclaredSupplementalGatherFields(ObjectProducer value)
            {
                _value = value ?? throw new System.ArgumentNullException(nameof(value));
            }

            public override string Kind => "declared";

            internal override void WritePayload(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                SupplementalGatherFieldsJsonConverter.WriteProperty(writer, options, "value", _value);
        }
    }

    internal sealed class SupplementalGatherFieldsJsonConverter : JsonConverter<SupplementalGatherFields>
    {
        public override void Write(Utf8JsonWriter writer, SupplementalGatherFields value, JsonSerializerOptions options)
        {
            if (value == null) throw new System.ArgumentNullException(nameof(value));

            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            value.WritePayload(writer, options);
            writer.WriteEndObject();
        }

        public override SupplementalGatherFields Read(
            ref Utf8JsonReader reader,
            System.Type typeToConvert,
            JsonSerializerOptions options) =>
            throw new System.NotSupportedException("Plan types are write-only.");

        internal static void WriteProperty<T>(
            Utf8JsonWriter writer,
            JsonSerializerOptions options,
            string name,
            T value)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, value, options);
        }
    }

    internal abstract class GatherSelection
    {
        private GatherSelection() { }

        internal static GatherSelection ExplicitFields { get; } = new ExplicitGatherSelection();

        internal static GatherSelection AllRegisteredInputs { get; } = new AllRegisteredInputsGatherSelection();

        public abstract string Kind { get; }

        internal abstract bool MayExpandRegisteredInputsAtRuntime { get; }

        internal void AddBuildTimeFields(
            List<GatherPayloadField> fields,
            PlanBuildContext context,
            GatherPayloadClaims claims)
        {
            if (fields == null) throw new System.ArgumentNullException(nameof(fields));
            if (context == null) throw new System.ArgumentNullException(nameof(context));
            if (claims == null) throw new System.ArgumentNullException(nameof(claims));

            AddBuildTimeFieldsCore(fields, context, claims);
        }

        private protected abstract void AddBuildTimeFieldsCore(
            List<GatherPayloadField> fields,
            PlanBuildContext context,
            GatherPayloadClaims claims);

        private sealed class ExplicitGatherSelection : GatherSelection
        {
            public override string Kind => "explicit";

            internal override bool MayExpandRegisteredInputsAtRuntime => false;

            private protected override void AddBuildTimeFieldsCore(
                List<GatherPayloadField> fields,
                PlanBuildContext context,
                GatherPayloadClaims claims)
            {
            }
        }

        private sealed class AllRegisteredInputsGatherSelection : GatherSelection
        {
            public override string Kind => "all-registered-inputs";

            internal override bool MayExpandRegisteredInputsAtRuntime => true;

            private protected override void AddBuildTimeFieldsCore(
                List<GatherPayloadField> fields,
                PlanBuildContext context,
                GatherPayloadClaims claims)
            {
                var buildTimeFields = BuildTimeGatherPayloadFields.From(fields, claims);

                foreach (var registration in context.GetRegisteredComponents())
                    buildTimeFields.AddRegisteredInput(registration);
            }
        }
    }

    internal sealed class GatherPayloadClaims
    {
        private readonly GatherPayloadSlots _payloadSlots;
        private readonly SelectedGatherComponentReads _componentReads;

        private GatherPayloadClaims(
            GatherPayloadSlots payloadSlots,
            SelectedGatherComponentReads componentReads)
        {
            _payloadSlots = payloadSlots ?? throw new System.ArgumentNullException(nameof(payloadSlots));
            _componentReads = componentReads ?? throw new System.ArgumentNullException(nameof(componentReads));
        }

        internal static GatherPayloadClaims From(
            IEnumerable<GatherPayloadField> fields,
            IEnumerable<string> supplementalPayloadPaths)
        {
            if (fields == null) throw new System.ArgumentNullException(nameof(fields));
            if (supplementalPayloadPaths == null)
                throw new System.ArgumentNullException(nameof(supplementalPayloadPaths));

            var fieldSnapshot = new List<GatherPayloadField>(fields);
            var payloadSlots = GatherPayloadSlots.From(fieldSnapshot);
            foreach (var payloadPath in supplementalPayloadPaths)
            {
                if (payloadPath == null)
                    throw new System.ArgumentException(
                        "Supplemental gather payload path must not be null.",
                        nameof(supplementalPayloadPaths));

                payloadSlots.ClaimDeclared(payloadPath);
            }

            return new GatherPayloadClaims(
                payloadSlots,
                SelectedGatherComponentReads.From(fieldSnapshot));
        }

        internal bool TryReserve(Alis.Reactive.ComponentRegistration registration)
        {
            if (registration == null) throw new System.ArgumentNullException(nameof(registration));

            var componentReadWasAlreadySelected = _componentReads.Contains(registration);
            if (componentReadWasAlreadySelected)
                return false;

            return _payloadSlots.TryClaim(registration.BindingPath);
        }
    }

    internal sealed class GatherPayloadSlots
    {
        private readonly List<Path> _claimedPaths;

        private GatherPayloadSlots(List<Path> claimedPaths)
        {
            _claimedPaths = claimedPaths ?? throw new System.ArgumentNullException(nameof(claimedPaths));
        }

        internal static GatherPayloadSlots From(IEnumerable<GatherPayloadField> fields)
        {
            if (fields == null) throw new System.ArgumentNullException(nameof(fields));

            var payloadSlots = new GatherPayloadSlots(new List<Path>());
            foreach (var field in fields)
                payloadSlots.ClaimDeclared(field.PayloadPath);

            return payloadSlots;
        }

        internal void ClaimDeclared(string payloadPath)
        {
            if (payloadPath == null) throw new System.ArgumentNullException(nameof(payloadPath));
            _claimedPaths.Add(Path.Parse(payloadPath));
        }

        internal bool TryClaim(string payloadPath)
        {
            if (payloadPath == null) throw new System.ArgumentNullException(nameof(payloadPath));

            var incoming = Path.Parse(payloadPath);
            foreach (var claimedPath in _claimedPaths)
            {
                var payloadPathAlreadyClaimed = claimedPath.Overlaps(incoming);
                if (payloadPathAlreadyClaimed)
                    return false;
            }

            _claimedPaths.Add(incoming);
            return true;
        }
    }

    internal sealed class BuildTimeGatherPayloadFields
    {
        private readonly List<GatherPayloadField> _fields;
        private readonly GatherPayloadClaims _claims;

        private BuildTimeGatherPayloadFields(
            List<GatherPayloadField> fields,
            GatherPayloadClaims claims)
        {
            _fields = fields ?? throw new System.ArgumentNullException(nameof(fields));
            _claims = claims ?? throw new System.ArgumentNullException(nameof(claims));
        }

        internal static BuildTimeGatherPayloadFields From(
            List<GatherPayloadField> fields,
            GatherPayloadClaims claims) =>
            new BuildTimeGatherPayloadFields(fields, claims);

        internal void AddRegisteredInput(
            KeyValuePair<string, Alis.Reactive.ComponentRegistration> registration)
        {
            var payloadSlotWasReserved = _claims.TryReserve(registration.Value);
            if (!payloadSlotWasReserved)
                return;

            _fields.Add(FieldFrom(registration));
        }

        private static GatherPayloadField FieldFrom(
            KeyValuePair<string, Alis.Reactive.ComponentRegistration> registration)
        {
            var component = registration.Value;
            var componentValue = ValueProducer.Read(
                ComponentSource.Of(component.ComponentId),
                component.ValueMember,
                shape: component.Shape);

            return GatherPayloadField.Of(registration.Key, componentValue);
        }
    }

    internal sealed class SelectedGatherComponentReads
    {
        private readonly HashSet<string> _componentKeys;

        private SelectedGatherComponentReads(HashSet<string> componentKeys)
        {
            _componentKeys = componentKeys ?? throw new System.ArgumentNullException(nameof(componentKeys));
        }

        internal static SelectedGatherComponentReads From(IEnumerable<GatherPayloadField> fields)
        {
            if (fields == null) throw new System.ArgumentNullException(nameof(fields));

            var componentKeys = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var field in fields)
            {
                GatherPayloadFieldComponentRead
                    .From(field)
                    .RecordIn(componentKeys);
            }

            return new SelectedGatherComponentReads(componentKeys);
        }

        internal bool Contains(Alis.Reactive.ComponentRegistration registration)
        {
            if (registration == null) throw new System.ArgumentNullException(nameof(registration));
            return _componentKeys.Contains(registration.ComponentId);
        }
    }

    internal abstract class GatherPayloadFieldComponentRead
    {
        private GatherPayloadFieldComponentRead() { }

        internal static GatherPayloadFieldComponentRead From(GatherPayloadField field)
        {
            if (field == null) throw new System.ArgumentNullException(nameof(field));

            return From(field.Value);
        }

        internal abstract void RecordIn(HashSet<string> componentKeys);

        private static GatherPayloadFieldComponentRead From(ValueProducer value)
        {
            if (!(value is ReadProducer read))
                return NoComponentRead.Instance;

            if (!(read.From is ComponentSource componentSource))
                return NoComponentRead.Instance;

            return new ComponentRead(
                Alis.Reactive.PlanModel.ComponentKey.Of(componentSource.Component));
        }

        private sealed class NoComponentRead : GatherPayloadFieldComponentRead
        {
            internal static NoComponentRead Instance { get; } = new NoComponentRead();

            internal override void RecordIn(HashSet<string> componentKeys)
            {
            }
        }

        private sealed class ComponentRead : GatherPayloadFieldComponentRead
        {
            private readonly ComponentKey _componentKey;

            internal ComponentRead(ComponentKey componentKey)
            {
                _componentKey = componentKey ?? throw new System.ArgumentNullException(nameof(componentKey));
            }

            internal override void RecordIn(HashSet<string> componentKeys)
            {
                if (componentKeys == null) throw new System.ArgumentNullException(nameof(componentKeys));
                componentKeys.Add(_componentKey.Value);
            }
        }
    }

    /// <summary>Sends a single evaluated value as the request body.</summary>
    public sealed class ValueInput : RequestInput
    {
        private readonly RequestTransport _transport;

        /// <summary>Gets the kind. Always <c>"value"</c>.</summary>
        public string Kind => "value";
        /// <summary>Gets the value expression to send as the body.</summary>
        public ObjectProducer Value { get; }
        /// <summary>Gets the transport format (json or form).</summary>
        public string Transport => _transport.Value;

        internal ValueInput(ObjectProducer value, RequestTransport transport)
        {
            Value = value ?? throw new System.ArgumentNullException(nameof(value));
            _transport = transport ?? throw new System.ArgumentNullException(nameof(transport));
        }
    }

    /// <summary>Maps an HTTP payload path to a value expression evaluated at request time.</summary>
    internal sealed class GatherPayloadField
    {
        private readonly BindingPath _payloadPath;

        /// <summary>HTTP payload path (from model binding path or explicit override).</summary>
        public string PayloadPath => _payloadPath.Value;
        /// <summary>How to read the value. Carries source, member, and shape.</summary>
        public ValueProducer Value { get; }

        internal GatherPayloadField(string payloadPath, ValueProducer value)
        {
            _payloadPath = BindingPath.Of(payloadPath);
            Value = value ?? throw new System.ArgumentNullException(nameof(value));
        }

        internal static GatherPayloadField Of(string payloadPath, ValueProducer value)
            => new GatherPayloadField(payloadPath, value);
    }

    /// <summary>Maps an HTTP response match to a reaction.</summary>
    public sealed class ResponseHandler
    {
        /// <summary>Gets the response status match that selects this handler.</summary>
        public ResponseStatusMatch Match { get; }
        /// <summary>Gets the reaction to execute when the status matches.</summary>
        public Reaction Reaction { get; }

        private ResponseHandler(Reaction reaction, ResponseStatusMatch match)
        {
            Reaction = reaction ?? throw new System.ArgumentNullException(nameof(reaction));
            Match = match ?? throw new System.ArgumentNullException(nameof(match));
        }

        internal static ResponseHandler AnyStatus(Reaction reaction) =>
            new ResponseHandler(reaction, ResponseStatusMatch.Any);

        internal static ResponseHandler ForStatus(Reaction reaction, int statusCode) =>
            new ResponseHandler(
                reaction,
                ResponseStatusMatch.Exact(HttpResponseStatusCode.FromDeveloperStatus(statusCode)));
    }

    /// <summary>Base class for HTTP response status matching. Not constructed in application code.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ResponseStatusMatch>))]
    public abstract class ResponseStatusMatch
    {
        private protected ResponseStatusMatch() { }

        internal static ResponseStatusMatch Any { get; } =
            new AnyResponseStatusMatch();

        /// <summary>Gets the response match kind.</summary>
        public abstract string Kind { get; }

        internal static ResponseStatusMatch Exact(HttpResponseStatusCode statusCode) =>
            new ExactResponseStatusMatch(statusCode);
    }

    /// <summary>Matches any HTTP response status in the current success or error group.</summary>
    public sealed class AnyResponseStatusMatch : ResponseStatusMatch
    {
        /// <summary>Gets the kind. Always <c>"any"</c>.</summary>
        public override string Kind => "any";
    }

    /// <summary>Matches one exact HTTP response status code.</summary>
    public sealed class ExactResponseStatusMatch : ResponseStatusMatch
    {
        private readonly HttpResponseStatusCode _statusCode;

        internal ExactResponseStatusMatch(HttpResponseStatusCode statusCode)
        {
            _statusCode = statusCode ?? throw new System.ArgumentNullException(nameof(statusCode));
        }

        /// <summary>Gets the kind. Always <c>"status"</c>.</summary>
        public override string Kind => "status";

        /// <summary>Gets the HTTP status code to match.</summary>
        public int Status => _statusCode.Value;
    }
}
