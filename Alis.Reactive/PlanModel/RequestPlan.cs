using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>An HTTP request definition in the reactive plan.</summary>
    public sealed class RequestPlan
    {
        private readonly RequestEndpoint _endpoint;
        private readonly RequestInput _input;
        private readonly RequestReactions _reactions;
        private readonly ResponseRouting _responseRouting;
        private readonly RequestValidationTarget _validationTarget;

        /// <summary>Gets the HTTP method (GET, POST, PUT, DELETE).</summary>
        public string Method => _endpoint.Method.Value;
        /// <summary>Gets the request URL, which may contain template placeholders like <c>{id}</c>.</summary>
        public string Url => _endpoint.Url.Value;
        /// <summary>Gets the validation target used before sending this request.</summary>
        public RequestValidationTarget Validation => _validationTarget;
        /// <summary>Gets the request body strategy. Bodiless requests use <see cref="NoRequestInput"/>.</summary>
        public RequestInput Input => _input;
        /// <summary>Gets reactions to execute while the request is loading.</summary>
        public IReadOnlyList<ReactionGraph> WhileLoading => _reactions.WhileLoading;
        /// <summary>Gets the success response routes.</summary>
        public IReadOnlyList<ResponseRoute> Success => _responseRouting.Success;
        /// <summary>Gets the error response routes.</summary>
        public IReadOnlyList<ResponseRoute> Error => _responseRouting.Error;
        /// <summary>Gets reactions to execute after the request settles regardless of outcome.</summary>
        public IReadOnlyList<ReactionGraph> Finally => _reactions.Finally;
        /// <summary>Gets the request chain: terminal or followed by another request.</summary>
        public RequestChain Chain => _responseRouting.Chain;
        private RequestPlan(
            RequestEndpoint endpoint,
            RequestInput input,
            RequestReactions reactions,
            ResponseRouting responseRouting,
            RequestValidationTarget validationTarget)
        {
            _endpoint = endpoint;
            _input = input;
            _reactions = reactions;
            _responseRouting = responseRouting;
            _validationTarget = validationTarget;
        }

        internal static RequestPlan Create(
            RequestEndpoint endpoint,
            RequestInput input,
            RequestReactions reactions,
            ResponseRouting responseRouting,
            RequestValidationTarget validationTarget) =>
            new RequestPlan(
                endpoint,
                input,
                reactions,
                responseRouting,
                validationTarget);

    }

    internal sealed class RequestReactions
    {
        private RequestReactions(
            IReadOnlyList<ReactionGraph> whileLoading,
            IReadOnlyList<ReactionGraph> finallyReactions)
        {
            WhileLoading = OrderedPlanItems.Snapshot(whileLoading);
            Finally = OrderedPlanItems.Snapshot(finallyReactions);
        }

        internal IReadOnlyList<ReactionGraph> WhileLoading { get; }
        internal IReadOnlyList<ReactionGraph> Finally { get; }

        internal static RequestReactions From(
            IReadOnlyList<ReactionGraph> whileLoading,
            IReadOnlyList<ReactionGraph> finallyReactions) =>
            new RequestReactions(whileLoading, finallyReactions);
    }

    internal sealed class ResponseRouting
    {
        private ResponseRouting(
            IReadOnlyList<ResponseRoute> success,
            IReadOnlyList<ResponseRoute> error,
            RequestChain chain)
        {
            Success = OrderedPlanItems.Snapshot(success);
            Error = OrderedPlanItems.Snapshot(error);
            Chain = chain;
        }

        internal IReadOnlyList<ResponseRoute> Success { get; }
        internal IReadOnlyList<ResponseRoute> Error { get; }
        internal RequestChain Chain { get; }

        internal static ResponseRouting From(
            IReadOnlyList<ResponseRoute> success,
            IReadOnlyList<ResponseRoute> error,
            RequestChain chain) =>
            new ResponseRouting(success, error, chain);
    }

    internal static class OrderedPlanItems
    {
        internal static IReadOnlyList<T> Snapshot<T>(IReadOnlyList<T> items)
        {
            var hasNoItems = items.Count == 0;
            if (hasNoItems) return System.Array.Empty<T>();
            return new List<T>(items);
        }
    }

    internal sealed class RequestEndpoint
    {
        private RequestEndpoint(HttpMethodName method, RequestUrl url)
        {
            Method = method;
            Url = url;
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

        internal static RequestChain ContinueWith(RequestPlan next) =>
            new FollowUpRequestChain(next);
    }

    /// <summary>Represents a request with no chained follow-up request.</summary>
    public sealed class TerminalRequestChain : RequestChain
    {
        /// <summary>Gets the kind. Always <c>"terminal"</c>.</summary>
        public override string Kind => "terminal";
    }

    /// <summary>Represents a request followed by another request after successful completion.</summary>
    public sealed class FollowUpRequestChain : RequestChain
    {
        private readonly RequestPlan _next;

        internal FollowUpRequestChain(RequestPlan next)
        {
            _next = next;
        }

        /// <summary>Gets the kind. Always <c>"follow-up"</c>.</summary>
        public override string Kind => "follow-up";

        /// <summary>Gets the request to run after the current request succeeds.</summary>
        public RequestPlan Next => _next;
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
            _container = container;
        }

        /// <summary>Gets the kind. Always <c>"container"</c>.</summary>
        public override string Kind => "container";

        /// <summary>Gets the container component ID to validate.</summary>
        public string Container => _container.Value;
    }

    /// <summary>Routes an HTTP response status match to a reaction.</summary>
    public sealed class ResponseRoute
    {
        /// <summary>Gets the response status match that selects this route.</summary>
        public ResponseStatusMatch Match { get; }
        /// <summary>Gets the reaction to execute when the status matches.</summary>
        public ReactionGraph Reaction { get; }

        private ResponseRoute(ReactionGraph reaction, ResponseStatusMatch match)
        {
            Reaction = reaction;
            Match = match;
        }

        internal static ResponseRoute AnyStatus(ReactionGraph reaction) =>
            new ResponseRoute(reaction, ResponseStatusMatch.Any);

        internal static ResponseRoute ForStatus(ReactionGraph reaction, int statusCode) =>
            new ResponseRoute(
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
            _statusCode = statusCode;
        }

        /// <summary>Gets the kind. Always <c>"status"</c>.</summary>
        public override string Kind => "status";

        /// <summary>Gets the HTTP status code to match.</summary>
        public int Status => _statusCode.Value;
    }
}
