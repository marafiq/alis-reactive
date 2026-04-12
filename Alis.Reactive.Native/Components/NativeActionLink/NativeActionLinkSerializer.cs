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

            // Carry the full plan context so the runtime can resolve
            // all component references in the reaction tree.
            var payloadJson = JsonSerializer.Serialize(
                new NativeActionLinkPayload(plan, projectedReaction),
                CompactOptions);

            return new NativeActionLinkContract(payloadJson);
        }

        private static Reaction ProjectReaction(Reaction reaction, string href, RequestProjectionState state)
        {
            switch (reaction)
            {
                case SequenceReaction sequential:
                    return Reaction.Sequence(new List<Reaction>(sequential.Steps));

                case BranchReaction conditional:
                    var projectedCases = new List<BranchCase>();
                    foreach (var c in conditional.Cases)
                        projectedCases.Add(new BranchCase(c.When, ProjectReaction(c.Reaction, href, state)));
                    return Reaction.Branch(projectedCases);

                case RequestReaction http:
                    state.RequestCount++;
                    if (state.RequestCount > 1)
                        throw new InvalidOperationException(
                            "NativeActionLink supports exactly one request.");
                    if (!string.Equals(href, http.Request.Url, StringComparison.Ordinal))
                        throw new InvalidOperationException(
                            "NativeActionLink href must match the request URL.");
                    return Reaction.Request(ProjectRequest(http.Request));

                case ParallelReaction _:
                    throw new InvalidOperationException(
                        "NativeActionLink does not support Parallel.");

                default:
                    return reaction;
            }
        }

        private static Request ProjectRequest(Request request)
        {
            if (request.Next != null)
                throw new InvalidOperationException(
                    "NativeActionLink does not support chained requests.");

            if (request.ValidatorType != null)
                throw new InvalidOperationException(
                    "NativeActionLink does not support validation.");

            var projected = new Request(request.Method, string.Empty);
            projected.Input = request.Input;
            projected.Before = request.Before;
            projected.Success = request.Success;
            projected.Error = request.Error;
            return projected;
        }

        private sealed class RequestProjectionState
        {
            public int RequestCount { get; set; }
        }
    }

    internal sealed class NativeActionLinkContract
    {
        internal NativeActionLinkContract(string payloadJson) { PayloadJson = payloadJson; }
        internal string PayloadJson { get; }
    }

    internal sealed class NativeActionLinkPayload
    {
        public NativeActionLinkPayload(Plan plan, Reaction reaction)
        {
            Plan = plan;
            Reaction = reaction;
        }
        public Plan Plan { get; }
        public Reaction Reaction { get; }
    }
}
