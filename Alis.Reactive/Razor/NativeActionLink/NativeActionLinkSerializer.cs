using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Builders;
using Alis.Reactive.Grammar;
using Alis.Reactive.Grammar.Lowering;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Builds the plan a NativeActionLink carries in its own plan script: a handler of the anchor's
    /// click that leaves a modified click (a new tab, a download) to the browser and otherwise
    /// prevents the navigation and runs the link's pipeline. The href is the request's URL.
    /// </summary>
    internal static class NativeActionLinkSerializer
    {
        internal static string CreatePlanJson<TModel>(
            string elementId,
            string expectedRequestUrl,
            Action<PipelineBuilder<TModel>> pipeline)
            where TModel : class
        {
            var context = new PlanBuildContext(
                PlanIdentity.Root(PlanId.Of(elementId)), new RegisteredInputComponents());
            var pipelineBuilder = new PipelineBuilder<TModel>(context);
            pipeline(pipelineBuilder);
            var reaction = pipelineBuilder.BuildReaction();

            // A request registers its validation while the reaction is built, so the check follows it.
            if (context.ValidationJobs.Count > 0)
                throw new InvalidOperationException(
                    "NativeActionLink does not support validation.");

            var requestCount = 0;
            RequireOneLinkRequest(reaction, expectedRequestUrl, ref requestCount);
            if (requestCount != 1)
            {
                throw new InvalidOperationException(
                    "NativeActionLink supports exactly one request in its click reaction tree.");
            }

            var plan = PlanLowering.LowerHandler(
                context.BuildPlan(),
                lowering => lowering.Objects.DomElement(elementId),
                "click",
                (lowering, reactions, scope) => new[]
                {
                    G.If(UnmodifiedLeftClick(scope.EventParam), ClickBody(reactions.Block(reaction, scope), scope.EventParam), G.NoStatements),
                });
            return GrammarSerializer.Serialize(plan);
        }

        // An unmodified left click the page has not already handled; anything else stays the browser's.
        private static Expr UnmodifiedLeftClick(string click)
        {
            Expr Member(string name) => G.Get(G.Ref(click), new[] { name }, Shape.None);
            return G.And(
                G.Eq(Member("button"), G.Lit(0)),
                G.Not(G.Or(Member("defaultPrevented"), Member("metaKey"), Member("ctrlKey"), Member("shiftKey"), Member("altKey"))));
        }

        private static IReadOnlyList<Statement> ClickBody(IReadOnlyList<Statement> pipeline, string click) =>
            new[] { G.Do(G.Call(G.Ref(click), "preventDefault")) }.Concat(pipeline).ToList();

        private static void RequireOneLinkRequest(ReactionGraph reaction, string expectedRequestUrl, ref int requestCount)
        {
            switch (reaction)
            {
                case SequenceReaction sequential:
                    foreach (var step in sequential.Steps)
                        RequireOneLinkRequest(step, expectedRequestUrl, ref requestCount);
                    return;

                case BranchReaction conditional:
                    foreach (var branchCase in conditional.Cases)
                        RequireOneLinkRequest(branchCase.Reaction, expectedRequestUrl, ref requestCount);
                    return;

                case RequestReaction requestReaction:
                    requestCount++;
                    if (requestCount > 1)
                        throw new InvalidOperationException(
                            "NativeActionLink supports exactly one request.");
                    if (!string.Equals(expectedRequestUrl, requestReaction.Request.Url, StringComparison.Ordinal))
                        throw new InvalidOperationException(
                            "NativeActionLink href must match the request URL.");
                    RequireLinkRequest(requestReaction.Request);
                    return;

                case ParallelReaction _:
                    throw new InvalidOperationException(
                        "NativeActionLink does not support Parallel.");
            }
        }

        // Body fields and headers travel with the request. A route parameter would need a {placeholder} in the
        // href, which a modifier-click opens as written; IncludeAll would gather the page's registered inputs,
        // which the link's own plan does not see.
        private static void RequireLinkRequest(RequestPlan request)
        {
            if (request.Chain.HasFollowUp)
                throw new InvalidOperationException(
                    "NativeActionLink does not support chained requests.");

            if (!(request.Input is GatherRequestInput gather))
                return;

            var gathersRouteParameter = gather.Assignments.Any(assignment => assignment.Target is RequestRouteParameterTarget);
            if (gathersRouteParameter)
                throw new InvalidOperationException(
                    "NativeActionLink does not support route parameters: its href must be a URL a modifier-click " +
                    "can open. Write the resolved URL into the href and the request.");

            if (gather.RegisteredInputs.SelectsRegisteredInputs)
                throw new InvalidOperationException(
                    "NativeActionLink does not support IncludeAll: the link's request does not see the page's " +
                    "registered inputs.");
        }
    }
}
