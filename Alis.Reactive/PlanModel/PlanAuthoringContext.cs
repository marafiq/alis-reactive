using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    internal sealed class PlanAuthoringContext
    {
        private readonly ValidationPlanCompiler _validation = new ValidationPlanCompiler();
        private readonly PlanContractCatalog _contracts;

        internal PlanAuthoringContext(string planId, string? sourceId = null)
        {
            Plan = new AuthoredPlan(
                planId,
                new Dictionary<string, CapabilityContract>(StringComparer.Ordinal),
                new Dictionary<string, RuntimeObject>(StringComparer.Ordinal),
                new Dictionary<string, FieldBinding>(StringComparer.Ordinal),
                new List<Workflow>())
            {
                SourceId = sourceId
            };

            _contracts = new PlanContractCatalog(Plan);
            Values = new PlanValueCompiler(this);
        }

        internal AuthoredPlan Plan { get; }
        internal PlanValueCompiler Values { get; }
        internal IReadOnlyDictionary<string, ComponentRegistration> Components => _contracts.Components;
        internal bool HasPendingValidators => _validation.HasPendingValidators;

        internal WorkflowScope CreateDomReadyScope()
        {
            return new WorkflowScope(new DomReadySubscription());
        }

        internal WorkflowScope CreateDocumentEventScope(string eventName)
        {
            return new WorkflowScope(new DocumentEventSubscription(eventName));
        }

        internal WorkflowScope CreateServerPushScope(string url, string? eventType)
        {
            var subscription = new ServerPushSubscription(url)
            {
                EventType = eventType
            };

            return new WorkflowScope(subscription);
        }

        internal WorkflowScope CreateSignalRScope(string hubUrl, string method)
        {
            return new WorkflowScope(new SignalRSubscription(hubUrl, method));
        }

        internal WorkflowScope CreateObjectEventScope(
            string componentId,
            ComponentMetadata component,
            string eventName,
            EventContractAuthoring? contractAuthoring = null)
        {
            var registration = GetRegistration(componentId);
            var objectName = EnsureComponentObject(componentId, component, registration);
            var contractKey = Plan.Objects[objectName].Contract;
            var eventContract = EnsureEvent(contractKey, eventName, eventName);
            contractAuthoring?.Invoke(_contracts, contractKey, eventName, eventContract);

            return new WorkflowScope(
                new ObjectEventSubscription(objectName, eventName),
                eventContract,
                objectName,
                eventName,
                contractKey);
        }

        internal void AddWorkflow(WorkflowScope scope, PlanAction action)
        {
            Plan.Workflows.Add(new Workflow(scope.Subscription, action));
        }

        internal string EnsureElementObjectForAction(string elementId)
        {
            return EnsureElementObject(elementId);
        }

        internal void RegisterComponent(string bindingPath, ComponentRegistration registration)
        {
            _contracts.RegisterComponent(registration);
        }

        internal void TrackPendingValidator(RequestPlan request, Type validatorType)
        {
            _validation.TrackPendingValidator(request, validatorType);
        }

        internal void ResolveValidation(IFormValidationExtractor? extractor)
        {
            _validation.ResolvePending(extractor);
        }

        internal ValueExpr BuildRequestValue(IReadOnlyList<RequestValuePart> requestValues)
        {
            return PlanRequestValueBuilder.Build(requestValues, Plan, Values);
        }

        internal RequestValidation ConvertValidation(FormValidation validation)
        {
            return ValidationPlanCompiler.Convert(validation);
        }

        internal PlanAction CreateSetActionForElement(
            string targetId,
            ComponentMetadata? component,
            CapabilityProperty member,
            object? literal = null,
            ValueExpr? valueExpr = null,
            ValueShape? sourceShape = null,
            ValueShape? assignedShape = null)
        {
            var targetObject = component != null
                ? EnsureComponentObject(targetId, component, GetRegistration(targetId))
                : EnsureElementObject(targetId);

            return CreateSetAction(
                targetObject,
                Plan.Objects[targetObject].Contract,
                member,
                literal,
                valueExpr,
                sourceShape,
                assignedShape);
        }

        internal PlanAction CreateCallActionForElement(
            string targetId,
            ComponentMetadata? component,
            CapabilityMethod member,
            IReadOnlyList<ValueExpr>? args = null,
            IReadOnlyList<ValueShape>? argShapes = null)
        {
            var targetObject = component != null
                ? EnsureComponentObject(targetId, component, GetRegistration(targetId))
                : EnsureElementObject(targetId);

            return CreateCallAction(
                targetObject,
                Plan.Objects[targetObject].Contract,
                member,
                args,
                argShapes);
        }

        internal PlanAction CreateSetActionForEvent(
            WorkflowScope scope,
            CapabilityProperty member,
            object? literal = null,
            ValueExpr? valueExpr = null,
            ValueShape? sourceShape = null,
            ValueShape? assignedShape = null)
        {
            if (scope.EventContract == null)
                throw new InvalidOperationException("Event object mutations require an object-event workflow.");

            var contractKey = EnsureSharedEventObjectContract(scope);
            var memberName = _contracts.EnsureEventPropertyMember(
                contractKey,
                member,
                ResolveAssignedShape(sourceShape, literal, assignedShape),
                "write");

            return new SetAction(
                new ActionTarget("$eventObject", memberName),
                CreateAssignedValue(valueExpr, literal, assignedShape));
        }

        internal PlanAction CreateCallActionForEvent(
            WorkflowScope scope,
            CapabilityMethod member,
            IReadOnlyList<ValueExpr>? args = null,
            IReadOnlyList<ValueShape>? argShapes = null)
        {
            if (scope.EventContract == null)
                throw new InvalidOperationException("Event object mutations require an object-event workflow.");

            return CreateCallAction(
                "$eventObject",
                EnsureSharedEventObjectContract(scope),
                member,
                args,
                argShapes);
        }

        internal static ValueShape InferShape(object? value)
        {
            return ValueShapeFactory.FromLiteral(value);
        }

        internal static PlanAction SequenceOrSingle(List<PlanAction> actions)
        {
            if (actions.Count == 1)
                return actions[0];

            return new SequenceAction(actions);
        }

        internal static ValueShape ResolveAssignedShape(ValueShape? sourceShape, object? literal, ValueShape? assignedShape)
        {
            if (assignedShape != null)
                return assignedShape;

            if (sourceShape != null)
                return sourceShape;

            return InferShape(literal);
        }

        private PlanAction CreateSetAction(
            string targetObject,
            string contractKey,
            CapabilityProperty member,
            object? literal,
            ValueExpr? valueExpr,
            ValueShape? sourceShape,
            ValueShape? assignedShape)
        {
            var memberName = EnsurePropertyMember(
                contractKey,
                member,
                ResolveAssignedShape(sourceShape, literal, assignedShape),
                "write");

            return new SetAction(
                new ActionTarget(targetObject, memberName),
                CreateAssignedValue(valueExpr, literal, assignedShape));
        }

        private PlanAction CreateCallAction(
            string targetObject,
            string contractKey,
            CapabilityMethod member,
            IReadOnlyList<ValueExpr>? args,
            IReadOnlyList<ValueShape>? argShapes)
        {
            var methodShapes = argShapes != null && argShapes.Count > 0
                ? new List<ValueShape>(argShapes)
                : null;

            var memberName = EnsureMethodMember(
                contractKey,
                member,
                methodShapes);

            var action = new CallAction(new ActionTarget(targetObject, memberName));
            if (args != null && args.Count > 0)
                action.Args = new List<ValueExpr>(args);

            return action;
        }

        private static ValueExpr CreateAssignedValue(ValueExpr? valueExpr, object? literal, ValueShape? assignedShape)
        {
            var expr = valueExpr ?? new LiteralValueExpr(literal);
            if (assignedShape == null)
                return expr;

            return new ConvertValueExpr(expr, assignedShape);
        }

        private string EnsureSharedEventObjectContract(WorkflowScope scope)
        {
            if (scope.EventContract == null || string.IsNullOrEmpty(scope.EventName) || string.IsNullOrEmpty(scope.ObjectContractKey))
                throw new InvalidOperationException("Event object contract requires an object-event workflow with a registered contract.");

            return _contracts.EnsureSharedEventObjectContract(scope.ObjectContractKey, scope.EventName, scope.EventContract);
        }

        private EventContract EnsureEvent(string contractKey, string eventName, string channel) =>
            _contracts.EnsureEvent(contractKey, eventName, channel);

        internal string EnsurePropertyMember(
            string contractKey,
            CapabilityProperty member,
            ValueShape shape,
            string access) =>
            _contracts.EnsurePropertyMember(contractKey, member, shape, access);

        private string EnsureMethodMember(
            string contractKey,
            CapabilityMethod member,
            List<ValueShape>? args) =>
            _contracts.EnsureMethodMember(contractKey, member, args);

        internal ComponentRegistration? GetRegistration(string componentId) =>
            _contracts.GetRegistration(componentId);

        internal string EnsureComponentObject(string componentId, ComponentMetadata component, ComponentRegistration? registration) =>
            _contracts.EnsureComponentObject(componentId, component, registration);

        private string EnsureElementObject(string elementId) =>
            _contracts.EnsureElementObject(elementId);

    }

    internal sealed class WorkflowScope
    {
        internal WorkflowScope(
            PlanSubscription subscription,
            EventContract? eventContract = null,
            string? objectName = null,
            string? eventName = null,
            string? objectContractKey = null)
        {
            Subscription = subscription;
            EventContract = eventContract;
            ObjectName = objectName;
            EventName = eventName;
            ObjectContractKey = objectContractKey;
        }

        internal PlanSubscription Subscription { get; }
        internal EventContract? EventContract { get; }
        internal string? ObjectName { get; }
        internal string? EventName { get; }
        internal string? ObjectContractKey { get; }
    }
}
