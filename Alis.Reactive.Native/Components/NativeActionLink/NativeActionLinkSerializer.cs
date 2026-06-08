using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    internal static class NativeActionLinkSerializer
    {
        private static readonly JsonSerializerOptions CompactOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        internal static NativeActionLinkContract CreateContract<TModel>(
            string expectedRequestUrl,
            Action<PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var planIdentity = PlanIdentity.Root(PlanId.Of("action-link"));
            var context = new PlanBuildContext(
                planIdentity, new RegisteredInputComponents());
            var pipelineBuilder = new PipelineBuilder<TModel>(context);
            pipeline(pipelineBuilder);

            if (context.ValidationJobs.Count > 0)
                throw new InvalidOperationException(
                    "NativeActionLink does not support validation.");

            var reaction = pipelineBuilder.BuildReaction();
            var requestCount = 0;
            var actionLinkReaction = BuildActionLinkReaction(reaction, expectedRequestUrl, ref requestCount);
            if (requestCount != 1)
            {
                throw new InvalidOperationException(
                    "NativeActionLink supports exactly one request in its click reaction tree.");
            }

            // Carry the full plan context so the runtime can resolve
            // all component references in the reaction tree.
            var payloadJson = JsonSerializer.Serialize(
                new NativeActionLinkPayload(context.BuildPlan(), actionLinkReaction),
                CompactOptions);

            return new NativeActionLinkContract(payloadJson);
        }

        private static ReactionGraph BuildActionLinkReaction(
            ReactionGraph reaction,
            string expectedRequestUrl,
            ref int requestCount)
        {
            switch (reaction)
            {
                case SequenceReaction sequential:
                    var actionLinkSteps = new List<ReactionGraph>();
                    foreach (var step in sequential.Steps)
                        actionLinkSteps.Add(BuildActionLinkReaction(step, expectedRequestUrl, ref requestCount));
                    return ReactionGraph.Sequence(actionLinkSteps);

                case BranchReaction conditional:
                    var actionLinkCases = new List<BranchCase>();
                    foreach (var branchCase in conditional.Cases)
                        actionLinkCases.Add(branchCase.WithReaction(BuildActionLinkReaction(branchCase.Reaction, expectedRequestUrl, ref requestCount)));
                    return ReactionGraph.Branch(actionLinkCases);

                case RequestReaction requestReaction:
                    requestCount++;
                    if (requestCount > 1)
                        throw new InvalidOperationException(
                            "NativeActionLink supports exactly one request.");
                    if (!string.Equals(expectedRequestUrl, requestReaction.Request.Url, StringComparison.Ordinal))
                        throw new InvalidOperationException(
                            "NativeActionLink href must match the request URL.");
                    return ReactionGraph.Request(BuildActionLinkRequest(requestReaction.Request));

                case ParallelReaction _:
                    throw new InvalidOperationException(
                        "NativeActionLink does not support Parallel.");

                default:
                    return reaction;
            }
        }

        private static RequestPlan BuildActionLinkRequest(RequestPlan request)
        {
            var requestHasFollowUp = request.Chain.HasFollowUp;
            if (requestHasFollowUp)
                throw new InvalidOperationException(
                    "NativeActionLink does not support chained requests.");

            return RequestPlan.Create(
                RequestEndpoint.To(HttpMethodName.From(request.Method), RequestUrl.Of(string.Empty)),
                BuildActionLinkInput(request.Input),
                RequestReactions.From(request.WhileLoading, Array.Empty<ReactionGraph>()),
                ResponseRouting.From(request.Success, request.Error, RequestChain.Terminal),
                RequestValidationTarget.None);
        }

        private static RequestInput BuildActionLinkInput(RequestInput input)
        {
            if (input is not GatherRequestInput gather)
                return input;

            var payloadAssignments = gather.Assignments
                .Where(assignment => assignment.Target is RequestPayloadTarget)
                .ToList();

            var hasNoActionLinkInput =
                payloadAssignments.Count == 0
                && !gather.RegisteredInputs.SelectsRegisteredInputs;
            if (hasNoActionLinkInput)
                return RequestInput.None;

            return GatherRequestInput.From(
                payloadAssignments,
                RequestBodyFormat.From(gather.BodyFormat),
                gather.RegisteredInputs);
        }
    }

    internal sealed class NativeActionLinkContract
    {
        internal NativeActionLinkContract(string payloadJson) { PayloadJson = payloadJson; }
        internal string PayloadJson { get; }
    }

    internal sealed class NativeActionLinkPayload
    {
        public NativeActionLinkPayload(PlanDocument plan, ReactionGraph reaction)
        {
            Plan = plan;
            Reaction = reaction;
        }
        public PlanDocument Plan { get; }
        public ReactionGraph Reaction { get; }
    }
}
