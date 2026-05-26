using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>An HTTP request definition in the reactive plan.</summary>
    public sealed class Request
    {
        private readonly RequestEndpoint _endpoint;
        private readonly RequestInput _input;
        private readonly IReadOnlyList<Reaction> _before;
        private readonly IReadOnlyList<ResponseHandler> _success;
        private readonly IReadOnlyList<ResponseHandler> _error;
        private readonly IReadOnlyList<Reaction> _complete;
        private readonly RequestChain _chain;
        private readonly IReadOnlyDictionary<string, ValueProducer> _headers;
        private readonly IReadOnlyDictionary<string, ValueProducer> _routeParams;
        private readonly RequestValidationTarget _validationTarget;

        /// <summary>Gets the HTTP method (GET, POST, PUT, DELETE, PATCH).</summary>
        public string Method => _endpoint.Method.Value;
        /// <summary>Gets the request URL, which may contain template placeholders like <c>{id}</c>.</summary>
        public string Url => _endpoint.Url.Value;
        /// <summary>Gets the validation target used before sending this request.</summary>
        public RequestValidationTarget Validation => _validationTarget;
        /// <summary>Gets the request body strategy. Bodiless requests use <see cref="NoRequestInput"/>.</summary>
        public RequestInput Input => _input;
        /// <summary>Gets reactions to execute before the request is sent.</summary>
        public IReadOnlyList<Reaction> Before => _before;
        /// <summary>Gets the success response handlers.</summary>
        public IReadOnlyList<ResponseHandler> Success => _success;
        /// <summary>Gets the error response handlers.</summary>
        public IReadOnlyList<ResponseHandler> Error => _error;
        /// <summary>Gets reactions to execute after the request completes regardless of outcome.</summary>
        public IReadOnlyList<Reaction> Complete => _complete;
        /// <summary>Gets the request chain: terminal or followed by another request.</summary>
        public RequestChain Chain => _chain;
        /// <summary>Gets the custom HTTP headers. Each value is evaluated at request time.</summary>
        public IReadOnlyDictionary<string, ValueProducer> Headers => _headers;
        /// <summary>Gets the URL template parameters. Each value is evaluated and URI-encoded before replacing placeholders in the URL.</summary>
        public IReadOnlyDictionary<string, ValueProducer> RouteParams => _routeParams;

        private Request(
            RequestEndpoint endpoint,
            RequestInput input,
            IReadOnlyList<Reaction> before,
            IReadOnlyList<ResponseHandler> success,
            IReadOnlyList<ResponseHandler> error,
            IReadOnlyList<Reaction> complete,
            RequestChain chain,
            IReadOnlyDictionary<string, ValueProducer> headers,
            IReadOnlyDictionary<string, ValueProducer> routeParams,
            RequestValidationTarget validationTarget)
        {
            _endpoint = endpoint ?? throw new System.ArgumentNullException(nameof(endpoint));
            _input = input ?? throw new System.ArgumentNullException(nameof(input));
            _before = Snapshot(before);
            _success = Snapshot(success);
            _error = Snapshot(error);
            _complete = Snapshot(complete);
            _chain = chain ?? throw new System.ArgumentNullException(nameof(chain));
            _headers = Snapshot(headers);
            _routeParams = Snapshot(routeParams);
            _validationTarget = validationTarget ?? throw new System.ArgumentNullException(nameof(validationTarget));
        }

        internal static Request Create(
            RequestEndpoint endpoint,
            RequestInput input,
            IReadOnlyList<Reaction> before,
            IReadOnlyList<ResponseHandler> success,
            IReadOnlyList<ResponseHandler> error,
            IReadOnlyList<Reaction> complete,
            RequestChain chain,
            IReadOnlyDictionary<string, ValueProducer> headers,
            IReadOnlyDictionary<string, ValueProducer> routeParams,
            RequestValidationTarget validationTarget) =>
            new Request(
                endpoint,
                input,
                before,
                success,
                error,
                complete,
                chain,
                headers,
                routeParams,
                validationTarget);

        /// <summary>Returns a copy of this request with <see cref="Before"/> replaced.</summary>
        internal Request WithBefore(IReadOnlyList<Reaction> before) =>
            new Request(
                _endpoint,
                _input,
                before,
                _success,
                _error,
                _complete,
                _chain,
                _headers,
                _routeParams,
                _validationTarget);

        private static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> items)
        {
            var hasNoItems = items.Count == 0;
            if (hasNoItems) return System.Array.Empty<T>();
            return new List<T>(items);
        }

        private static IReadOnlyDictionary<string, ValueProducer> Snapshot(
            IReadOnlyDictionary<string, ValueProducer> values)
        {
            if (values.Count == 0)
                return new Dictionary<string, ValueProducer>();

            return new Dictionary<string, ValueProducer>(values, System.StringComparer.Ordinal);
        }
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
