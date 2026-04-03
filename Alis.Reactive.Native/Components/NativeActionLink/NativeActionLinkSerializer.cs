using System;
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
            var authoring = new PlanAuthoringContext(typeof(TModel).FullName ?? typeof(TModel).Name);
            var scope = authoring.CreateDomReadyScope();
            var pb = new PipelineBuilder<TModel>(authoring, scope);
            pipeline(pb);

            var action = pb.BuildAction();
            var state = new RequestInspectionState();
            InspectAction(action, href, state);

            if (state.RequestCount != 1)
            {
                throw new InvalidOperationException(
                    "NativeActionLink supports exactly one request in its click workflow tree.");
            }

            var payloadJson = JsonSerializer.Serialize(
                new NativeActionLinkPayload(authoring.Document, action),
                CompactOptions);

            return new NativeActionLinkContract(payloadJson);
        }

        private static void InspectAction(PlanAction action, string href, RequestInspectionState state)
        {
            switch (action)
            {
                case SequenceAction sequence:
                    foreach (var step in sequence.Steps)
                        InspectAction(step, href, state);
                    return;

                case BranchAction branch:
                    foreach (var @case in branch.Cases)
                        InspectAction(@case.Run, href, state);
                    return;

                case RequestAction requestAction:
                    state.RequestCount++;
                    if (state.RequestCount > 1)
                    {
                        throw new InvalidOperationException(
                            "NativeActionLink supports exactly one request in its click workflow tree.");
                    }

                    ValidateRequest(requestAction.Request, href);
                    return;

                case ParallelAction _:
                    throw new InvalidOperationException(
                        "NativeActionLink does not support Parallel(...) request chains.");

                default:
                    return;
            }
        }

        private static void ValidateRequest(RequestPlan request, string href)
        {
            if (!string.Equals(href, request.Url, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "NativeActionLink href must match the request URL in the configured request chain.");
            }

            if (request.Next != null)
            {
                throw new InvalidOperationException(
                    "NativeActionLink supports exactly one request. Response.Chained(...) is not supported.");
            }

            if (request.Validation != null)
            {
                throw new InvalidOperationException(
                    "NativeActionLink does not support validation. Use a plan-backed trigger for validated flows.");
            }

            if (request.Input?.Value is BindingMapValueExpr map && Equals(map.Include, "all"))
            {
                throw new InvalidOperationException(
                    "NativeActionLink does not support IncludeAll(). Use explicit gather instead.");
            }

            EnsureHandlersDoNotStartRequest(request.OnSuccess);
            EnsureHandlersDoNotStartRequest(request.OnError);
        }

        private static void EnsureHandlersDoNotStartRequest(System.Collections.Generic.List<ResponseHandlerPlan>? handlers)
        {
            if (handlers == null)
                return;

            foreach (var handler in handlers)
                EnsureNoNestedRequests(handler.Run);
        }

        private static void EnsureNoNestedRequests(PlanAction action)
        {
            switch (action)
            {
                case SequenceAction sequence:
                    foreach (var step in sequence.Steps)
                        EnsureNoNestedRequests(step);
                    return;

                case BranchAction branch:
                    foreach (var @case in branch.Cases)
                        EnsureNoNestedRequests(@case.Run);
                    return;

                case RequestAction _:
                case ParallelAction _:
                    throw new InvalidOperationException(
                        "NativeActionLink response handlers cannot start a second HTTP request.");

                default:
                    return;
            }
        }

        private sealed class RequestInspectionState
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
        public NativeActionLinkPayload(ReactivePlanV2Document plan, PlanAction action)
        {
            Plan = plan;
            Action = action;
        }

        public ReactivePlanV2Document Plan { get; }
        public PlanAction Action { get; }
    }
}
