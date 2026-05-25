using System.Collections.Generic;
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
