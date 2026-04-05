using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    internal static class NativeActionLinkSerializer
    {
        private static readonly JsonSerializerOptions CompactOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        internal static NativeActionLinkContract CreateContract<TModel>(
            string href,
            Action<PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var plan = Plan.Create("action-link", null);
            var context = new PlanBuildContext(plan, new Dictionary<string, ComponentRegistration>());
            var pb = new PipelineBuilder<TModel>(context);
            pipeline(pb);

            var reaction = pb.BuildReaction();
            var state = new RequestProjectionState();
            var projectedReaction = ProjectReaction(reaction, href, state);
            if (state.RequestCount != 1)
            {
                throw new InvalidOperationException(
                    "NativeActionLink supports exactly one request in its click reaction tree.");
            }

            var payloadJson = JsonSerializer.Serialize(
                new NativeActionLinkPayload(projectedReaction),
                CompactOptions);

            return new NativeActionLinkContract(payloadJson);
        }

        private static Reaction ProjectReaction(
            Reaction reaction,
            string href,
            RequestProjectionState state)
        {
            if (reaction is SequenceReaction sequential)
            {
                return Reaction.Sequence(sequential.Steps);
            }

            if (reaction is BranchReaction conditional)
            {
                var projectedCases = new List<BranchCase>();
                foreach (var branchCase in conditional.Cases)
                {
                    projectedCases.Add(new BranchCase(branchCase.When, ProjectReaction(branchCase.Reaction, href, state)));
                }

                return Reaction.Branch(projectedCases);
            }

            if (reaction is RequestReaction http)
            {
                state.RequestCount++;
                if (state.RequestCount > 1)
                {
                    throw new InvalidOperationException(
                        "NativeActionLink supports exactly one request in its click reaction tree.");
                }

                if (!string.Equals(href, http.Request.Url, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "NativeActionLink href must match the request URL in the configured request chain.");
                }

                var request = ProjectRequest(http.Request);
                return Reaction.Request(request);
            }

            if (reaction is ParallelReaction)
            {
                throw new InvalidOperationException(
                    "NativeActionLink does not support Parallel(...) request chains.");
            }

            throw new InvalidOperationException("Unsupported NativeActionLink reaction shape.");
        }

        private static Request ProjectRequest(Request request)
        {
            if (request.Next != null)
            {
                throw new InvalidOperationException(
                    "NativeActionLink supports exactly one request. Response.Chained(...) is not supported.");
            }

            if (request.ValidatorType != null)
            {
                throw new InvalidOperationException(
                    "NativeActionLink does not support validation. Use a plan-backed trigger for validated flows.");
            }

            if (request.Input is GatherInput gather)
            {
                foreach (var field in gather.Components)
                {
                    if (field.Component == "*" && field.Key == "*")
                    {
                        throw new InvalidOperationException(
                            "NativeActionLink does not support IncludeAll(). Use explicit gather instead.");
                    }
                }
            }

            var projected = new Request(request.Method, string.Empty);
            projected.Input = request.Input;
            projected.Before = request.Before;
            projected.Success = ProjectHandlers(request.Success, new RequestProjectionState { RequestCount = 1 });
            projected.Error = ProjectHandlers(request.Error, new RequestProjectionState { RequestCount = 1 });
            return projected;
        }

        private static List<ResponseHandler> ProjectHandlers(List<ResponseHandler> handlers, RequestProjectionState state)
        {
            if (handlers == null || handlers.Count == 0)
            {
                return null;
            }

            var projected = new List<ResponseHandler>();
            foreach (var handler in handlers)
            {
                if (handler.Reaction != null)
                {
                    var reaction = ProjectReaction(handler.Reaction, string.Empty, state);
                    var projectedHandler = new ResponseHandler(reaction);
                    projectedHandler.Status = handler.Status;
                    projected.Add(projectedHandler);
                }
            }

            return projected.Count == 0 ? null : projected;
        }

        private sealed class RequestProjectionState
        {
            public int RequestCount { get; set; }
        }
    }

    internal sealed class NativeActionLinkContract
    {
        internal NativeActionLinkContract(string payloadJson)
        {
            PayloadJson = payloadJson;
        }

        internal string PayloadJson { get; }
    }

    internal sealed class NativeActionLinkPayload
    {
        public NativeActionLinkPayload(Reaction reaction)
        {
            Reaction = reaction;
        }

        public Reaction Reaction { get; }
    }
}
