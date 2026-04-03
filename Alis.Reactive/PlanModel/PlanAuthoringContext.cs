using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Alis.Reactive.Builders.Requests;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    internal sealed class PlanAuthoringContext
    {
        private readonly Dictionary<string, ComponentRegistration> _components =
            new Dictionary<string, ComponentRegistration>(StringComparer.Ordinal);

        private readonly Dictionary<string, ComponentRegistration> _componentsById =
            new Dictionary<string, ComponentRegistration>(StringComparer.Ordinal);

        private readonly Dictionary<RequestPlan, Type> _pendingValidators =
            new Dictionary<RequestPlan, Type>();

        internal PlanAuthoringContext(string planId, string? sourceId = null)
        {
            Document = new ReactivePlanV2Document(
                planId,
                new Dictionary<string, CapabilityContract>(StringComparer.Ordinal),
                new Dictionary<string, RuntimeObject>(StringComparer.Ordinal),
                new Dictionary<string, FieldBinding>(StringComparer.Ordinal),
                new List<Workflow>())
            {
                SourceId = sourceId
            };
        }

        internal ReactivePlanV2Document Document { get; }
        internal IReadOnlyDictionary<string, ComponentRegistration> Components => _components;
        internal bool HasPendingValidators => _pendingValidators.Count > 0;

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
            string vendor,
            string? bindingPath,
            string? valueMemberPath,
            string eventName)
        {
            var registration = GetRegistration(componentId, bindingPath);
            var objectName = EnsureComponentObject(componentId, vendor, registration);
            var contractKey = Document.Objects[objectName].Contract;
            var eventContract = EnsureEvent(contractKey, eventName, eventName);

            if (vendor == "native" && !string.IsNullOrEmpty(valueMemberPath))
            {
                var readShape = registration != null && registration.ValueMemberPath == valueMemberPath
                    ? ShapeFromCoerce(registration.CoerceAs)
                    : new AnyValueShape() as ValueShape;

                var eventObjectContractKey = EnsureSharedEventObjectContract(contractKey, eventName, eventContract);
                var eventMemberName = EnsurePropertyMember(
                    eventObjectContractKey,
                    LastPathSegment(valueMemberPath),
                    ParsePath("currentTarget." + valueMemberPath),
                    readShape,
                    "read");

                if (eventContract.Data == null)
                    eventContract.Data = new Dictionary<string, ValueExpr>(StringComparer.Ordinal);

                eventContract.Data[LastPathSegment(valueMemberPath)] = new MemberValueExpr("$eventObject", eventMemberName);
            }

            return new WorkflowScope(
                new ObjectEventSubscription(objectName, eventName),
                eventContract,
                objectName,
                eventName,
                contractKey);
        }

        internal void AddWorkflow(WorkflowScope scope, PlanAction action)
        {
            Document.Workflows.Add(new Workflow(scope.Subscription, action));
        }

        internal string EnsureElementObjectForAction(string elementId)
        {
            return EnsureElementObject(elementId);
        }

        internal void RegisterComponent(string bindingPath, ComponentRegistration registration)
        {
            _components[bindingPath] = registration;

            if (!_componentsById.ContainsKey(registration.ComponentId))
                _componentsById[registration.ComponentId] = registration;

            var objectName = EnsureRegisteredComponentObject(registration);
            var shape = ShapeFromCoerce(registration.CoerceAs);
            var memberName = EnsurePropertyMember(
                Document.Objects[objectName].Contract,
                registration.ValueMemberPath,
                ParsePath(registration.ValueMemberPath),
                shape,
                "read");

            Document.Bindings[bindingPath] = new FieldBinding(objectName, memberName, shape);
        }

        internal void TrackPendingValidator(RequestPlan request, Type validatorType)
        {
            _pendingValidators[request] = validatorType;
        }

        internal void ResolveValidation(IFormValidationExtractor? extractor)
        {
            if (_pendingValidators.Count == 0)
                return;

            if (extractor == null)
            {
                throw new InvalidOperationException(
                    "One or more requests use Validate<TValidator>() but no validation extractor is registered. " +
                    "Call ReactivePlanConfig.UseFormValidationExtractor(...) at app startup.");
            }

            foreach (var kvp in _pendingValidators)
            {
                var request = kvp.Key;
                var validatorType = kvp.Value;
                var existingValidation = request.Validation;
                if (existingValidation == null)
                    continue;

                var extracted = extractor.ExtractRules(validatorType, existingValidation.FormId);
                if (extracted == null)
                {
                    throw new InvalidOperationException(
                        $"Validator '{validatorType.Name}' produced no client rules for form '{existingValidation.FormId}'. " +
                        "Ensure the validator is registered in the factory and has extractable rules.");
                }

                request.Validation = ConvertValidation(extracted);
            }

            _pendingValidators.Clear();
        }

        internal object BuildRequestValue(IReadOnlyList<RequestValuePart> requestValues)
        {
            if (requestValues.Count == 1 && requestValues[0] is IncludeAllBindingsRequestValue)
                return new BindingMapValueExpr("all");

            var fields = new Dictionary<string, ValueExpr>(StringComparer.Ordinal);
            foreach (var requestValue in requestValues)
            {
                if (requestValue is IncludeAllBindingsRequestValue)
                {
                    foreach (var binding in Document.Bindings.Keys)
                        AddNestedField(fields, binding, new BindingValueExpr(binding));
                    continue;
                }

                if (requestValue is LiteralRequestValue literal)
                {
                    AddNestedField(fields, literal.Key, new LiteralValueExpr(literal.Value));
                    continue;
                }

                if (requestValue is ContextRequestValue context)
                {
                    AddNestedField(fields, context.Key, CreateContextValue(context.Path));
                    continue;
                }

                if (requestValue is ComponentRequestValue component)
                {
                    AddNestedField(
                        fields,
                        component.Key,
                        CreateComponentMemberValue(component.ComponentId, component.Vendor, component.ValueMemberPath, component.Key));
                }
            }

            return new ObjectValueExpr(fields);
        }

        internal RequestValidation ConvertValidation(FormValidation validation)
        {
            var fields = new List<RequestValidationField>();
            foreach (var field in validation.Fields)
            {
                var rules = new List<RequestValidationRule>();
                foreach (var rule in field.Rules)
                {
                    var converted = new RequestValidationRule(rule.Rule, rule.Message)
                    {
                        Constraint = rule.Constraint,
                        OtherBinding = rule.Field,
                        As = string.IsNullOrEmpty(rule.CoerceAs) ? null : ShapeFromCoerce(rule.CoerceAs),
                        When = ConvertValidationCondition(rule.When)
                    };

                    rules.Add(converted);
                }

                fields.Add(new RequestValidationField(field.FieldName, rules));
            }

            return new RequestValidation(validation.FormId, fields);
        }

        internal ValueExpr CreateComponentMemberValue(
            string componentId,
            string vendor,
            string valueMemberPath,
            string? bindingPath = null)
        {
            var registration = GetRegistration(componentId, bindingPath);
            var objectName = EnsureComponentObject(componentId, vendor, registration);
            var contractKey = Document.Objects[objectName].Contract;
            var shape = registration != null && registration.ValueMemberPath == valueMemberPath
                ? ShapeFromCoerce(registration.CoerceAs)
                : new AnyValueShape() as ValueShape;

            var memberName = EnsurePropertyMember(
                contractKey,
                valueMemberPath,
                ParsePath(valueMemberPath),
                shape,
                "read");

            return new MemberValueExpr(objectName, memberName);
        }

        internal ValueExpr CreateContextValue(string path)
        {
            return ConvertContextPath(path);
        }

        internal PlanAction CreateSetActionForElement(
            string targetId,
            string? vendor,
            string memberPath,
            object? literal = null,
            ValueExpr? valueExpr = null,
            string? valueCoerceAs = null,
            string? coerceAs = null)
        {
            var targetObject = vendor != null
                ? EnsureComponentObject(targetId, vendor, GetRegistration(targetId, null))
                : EnsureElementObject(targetId);

            return CreateSetAction(
                targetObject,
                Document.Objects[targetObject].Contract,
                memberPath,
                literal,
                valueExpr,
                valueCoerceAs,
                coerceAs);
        }

        internal PlanAction CreateCallActionForElement(
            string targetId,
            string? vendor,
            string memberPath,
            IReadOnlyList<ValueExpr>? args = null,
            IReadOnlyList<ValueShape>? argShapes = null)
        {
            var targetObject = vendor != null
                ? EnsureComponentObject(targetId, vendor, GetRegistration(targetId, null))
                : EnsureElementObject(targetId);

            return CreateCallAction(
                targetObject,
                Document.Objects[targetObject].Contract,
                memberPath,
                args,
                argShapes);
        }

        internal PlanAction CreateSetActionForEvent(
            WorkflowScope scope,
            string memberPath,
            object? literal = null,
            ValueExpr? valueExpr = null,
            string? valueCoerceAs = null,
            string? coerceAs = null)
        {
            if (scope.EventContract == null)
                throw new InvalidOperationException("Event object mutations require an object-event workflow.");

            return CreateSetAction(
                "$eventObject",
                EnsureSharedEventObjectContract(scope),
                memberPath,
                literal,
                valueExpr,
                valueCoerceAs,
                coerceAs);
        }

        internal PlanAction CreateCallActionForEvent(
            WorkflowScope scope,
            string memberPath,
            IReadOnlyList<ValueExpr>? args = null,
            IReadOnlyList<ValueShape>? argShapes = null)
        {
            if (scope.EventContract == null)
                throw new InvalidOperationException("Event object mutations require an object-event workflow.");

            return CreateCallAction(
                "$eventObject",
                EnsureSharedEventObjectContract(scope),
                memberPath,
                args,
                argShapes);
        }

        internal static ValueShape ShapeFromCoerce(string coerceAs)
        {
            switch (coerceAs)
            {
                case "string":
                    return new ScalarValueShape("string");
                case "number":
                    return new ScalarValueShape("number");
                case "boolean":
                    return new ScalarValueShape("boolean");
                case "date":
                    return new ScalarValueShape("date");
                case "raw":
                    return new ScalarValueShape("raw");
                case "array":
                    return new ArrayValueShape(new AnyValueShape());
                default:
                    return new AnyValueShape();
            }
        }

        internal static ValueShape InferShape(object? value)
        {
            if (value == null)
                return new AnyValueShape();

            if (value is string)
                return new ScalarValueShape("string");

            if (value is bool)
                return new ScalarValueShape("boolean");

            if (value is DateTime || value is DateTimeOffset || value is DateOnly)
                return new ScalarValueShape("date");

            if (value is byte || value is sbyte || value is short || value is ushort ||
                value is int || value is uint || value is long || value is ulong ||
                value is float || value is double || value is decimal)
                return new ScalarValueShape("number");

            if (!(value is string) && value is IEnumerable enumerable)
            {
                ValueShape? firstShape = null;
                foreach (var item in enumerable)
                {
                    firstShape = InferShape(item);
                    break;
                }

                return new ArrayValueShape(firstShape ?? new AnyValueShape());
            }

            return new AnyValueShape();
        }

        internal static List<PathSegment> ParsePath(string path)
        {
            var segments = new List<PathSegment>();
            foreach (var rawSegment in path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(rawSegment, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
                    segments.Add(PathSegment.FromIndex(index));
                else
                    segments.Add(PathSegment.FromProp(rawSegment));
            }

            return segments;
        }

        internal static PlanAction SequenceOrSingle(List<PlanAction> actions)
        {
            if (actions.Count == 1)
                return actions[0];

            return new SequenceAction(actions);
        }

        internal static ValueShape ResolveAssignedShape(string? valueCoerceAs, object? literal, string? coerceAs)
        {
            if (!string.IsNullOrEmpty(coerceAs))
                return ShapeFromCoerce(coerceAs);

            if (!string.IsNullOrEmpty(valueCoerceAs))
                return ShapeFromCoerce(valueCoerceAs);

            return InferShape(literal);
        }

        private PlanAction CreateSetAction(
            string targetObject,
            string contractKey,
            string memberPath,
            object? literal,
            ValueExpr? valueExpr,
            string? valueCoerceAs,
            string? coerceAs)
        {
            var memberName = EnsurePropertyMember(
                contractKey,
                memberPath,
                ParsePath(memberPath),
                ResolveAssignedShape(valueCoerceAs, literal, coerceAs),
                "write");

            return new SetAction(
                new ActionTarget(targetObject, memberName),
                CreateAssignedValue(valueExpr, literal, coerceAs));
        }

        private PlanAction CreateCallAction(
            string targetObject,
            string contractKey,
            string memberPath,
            IReadOnlyList<ValueExpr>? args,
            IReadOnlyList<ValueShape>? argShapes)
        {
            var methodShapes = argShapes != null && argShapes.Count > 0
                ? new List<ValueShape>(argShapes)
                : null;

            var memberName = EnsureMethodMember(
                contractKey,
                BuildMemberName(memberPath),
                ParsePath(memberPath),
                methodShapes);

            var action = new CallAction(new ActionTarget(targetObject, memberName));
            if (args != null && args.Count > 0)
                action.Args = new List<ValueExpr>(args);

            return action;
        }

        private PlanPredicate? ConvertValidationCondition(ValidationCondition? condition)
        {
            if (condition == null)
                return null;

            var predicate = new ComparePredicate(new BindingValueExpr(condition.Field), condition.Op);
            if (condition.Value != null)
                predicate.Right = new LiteralValueExpr(condition.Value);

            return predicate;
        }

        private static ValueExpr CreateAssignedValue(ValueExpr? valueExpr, object? literal, string? coerceAs)
        {
            var expr = valueExpr ?? new LiteralValueExpr(literal);
            if (string.IsNullOrEmpty(coerceAs))
                return expr;

            return new ConvertValueExpr(expr, ShapeFromCoerce(coerceAs));
        }

        private ValueExpr ConvertContextPath(string path)
        {
            if (path.StartsWith("evt.", StringComparison.Ordinal))
                return new ContextValueExpr("event", ParsePath(path.Substring(4)));

            if (path == "evt")
                return new ContextValueExpr("event");

            if (path.StartsWith("responseBody.", StringComparison.Ordinal))
                return new ContextValueExpr("response", ParsePath(path.Substring("responseBody.".Length)));

            if (path == "responseBody")
                return new ContextValueExpr("response");

            if (path.StartsWith("request.", StringComparison.Ordinal))
                return new ContextValueExpr("request", ParsePath(path.Substring("request.".Length)));

            return new ContextValueExpr("local", ParsePath(path));
        }

        private string EnsureRegisteredComponentObject(ComponentRegistration registration)
        {
            var objectName = ComponentObjectName(registration.ComponentId);
            var contractKey = ContractKey(registration.Vendor, registration.ComponentType, registration.ComponentId);
            EnsureContract(contractKey, "component", ResolverForVendor(registration.Vendor));

            if (Document.Objects.TryGetValue(objectName, out var existingObject))
            {
                PromoteObjectContract(existingObject, contractKey);
                existingObject.ElementId ??= registration.ComponentId;
            }
            else
            {
                Document.Objects[objectName] = new RuntimeObject(contractKey, registration.ComponentId);
            }

            return objectName;
        }

        private string EnsureComponentObject(string componentId, string vendor, ComponentRegistration? registration)
        {
            if (registration != null)
                return EnsureRegisteredComponentObject(registration);

            var objectName = ComponentObjectName(componentId);
            var contractKey = ContractKey(vendor, null, componentId);
            EnsureObject(objectName, contractKey, "component", ResolverForVendor(vendor), componentId);
            return objectName;
        }

        private string EnsureElementObject(string elementId)
        {
            var objectName = ElementObjectName(elementId);
            var contractKey = "element." + Sanitize(elementId);
            EnsureObject(objectName, contractKey, "element", "native-element", elementId);
            return objectName;
        }

        private void EnsureObject(string objectName, string contractKey, string kind, string resolver, string? elementId)
        {
            EnsureContract(contractKey, kind, resolver);

            if (!Document.Objects.ContainsKey(objectName))
                Document.Objects[objectName] = new RuntimeObject(contractKey, elementId);
        }

        private void EnsureContract(string contractKey, string kind, string resolver)
        {
            if (!Document.Contracts.ContainsKey(contractKey))
                Document.Contracts[contractKey] = new CapabilityContract(kind, resolver);
        }

        private string EnsureSharedEventObjectContract(WorkflowScope scope)
        {
            if (scope.EventContract == null || string.IsNullOrEmpty(scope.EventName) || string.IsNullOrEmpty(scope.ObjectContractKey))
                throw new InvalidOperationException("Event object contract requires an object-event workflow with a registered contract.");

            return EnsureSharedEventObjectContract(scope.ObjectContractKey, scope.EventName, scope.EventContract);
        }

        private string EnsureSharedEventObjectContract(string contractKey, string eventName, EventContract eventContract)
        {
            var eventObjectContractKey = "event-object."
                + Sanitize(contractKey)
                + "."
                + Sanitize(eventName);

            if (!Document.Contracts.ContainsKey(eventObjectContractKey))
                Document.Contracts[eventObjectContractKey] = new CapabilityContract("event-object", "event-object");

            eventContract.EventObject = new EventObjectReference(eventObjectContractKey);

            return eventObjectContractKey;
        }

        private EventContract EnsureEvent(string contractKey, string eventName, string channel)
        {
            var contract = Document.Contracts[contractKey];
            if (contract.Events == null)
                contract.Events = new Dictionary<string, EventContract>(StringComparer.Ordinal);

            if (!contract.Events.TryGetValue(eventName, out var eventContract))
            {
                eventContract = new EventContract(channel);
                contract.Events[eventName] = eventContract;
            }
            else
            {
                eventContract.Channel = channel;
            }

            return eventContract;
        }

        private string EnsurePropertyMember(
            string contractKey,
            string preferredName,
            List<PathSegment> path,
            ValueShape shape,
            string access)
        {
            var memberName = BuildMemberName(preferredName);
            var contract = Document.Contracts[contractKey];
            if (contract.Members.TryGetValue(memberName, out var existingMember))
            {
                if (existingMember is PropertyMember property)
                {
                    property.Shape = MergeShape(property.Shape, shape);
                    property.Access = MergeAccess(property.Access, access);
                    return memberName;
                }
            }

            contract.Members[memberName] = new PropertyMember(path, shape, access);
            return memberName;
        }

        private string EnsureMethodMember(
            string contractKey,
            string preferredName,
            List<PathSegment> path,
            List<ValueShape>? args)
        {
            var memberName = BuildMemberName(preferredName);
            var contract = Document.Contracts[contractKey];
            if (contract.Members.TryGetValue(memberName, out var existingMember))
            {
                if (existingMember is MethodMember method)
                {
                    if (args != null && args.Count > 0)
                        method.Args = MergeArgShapes(method.Args, args);

                    if (method.Returns == null)
                        method.Returns = "void";

                    return memberName;
                }
            }

            var created = new MethodMember(path)
            {
                Args = args,
                Returns = "void"
            };

            contract.Members[memberName] = created;
            return memberName;
        }

        private ComponentRegistration? GetRegistration(string componentId, string? bindingPath)
        {
            if (bindingPath != null && _components.TryGetValue(bindingPath, out var byBinding))
                return byBinding;

            if (_componentsById.TryGetValue(componentId, out var byId))
                return byId;

            return null;
        }

        private static List<ValueShape>? MergeArgShapes(List<ValueShape>? current, List<ValueShape> next)
        {
            if (current == null || current.Count == 0)
                return next;

            if (current.Count != next.Count)
                return current;

            var merged = new List<ValueShape>();
            for (var i = 0; i < current.Count; i++)
                merged.Add(MergeShape(current[i], next[i]));

            return merged;
        }

        private static string ComponentObjectName(string componentId)
        {
            return "component::" + componentId;
        }

        private static string ElementObjectName(string elementId)
        {
            return "element::" + elementId;
        }

        private static string ContractKey(string vendor, string? componentType, string componentId)
        {
            if (!string.IsNullOrEmpty(componentType))
                return vendor + "." + componentType;

            return vendor + ".component." + Sanitize(componentId);
        }

        private static string ResolverForVendor(string vendor)
        {
            return vendor == "fusion" ? "fusion-instance" : "native-element";
        }

        private void PromoteObjectContract(RuntimeObject runtimeObject, string contractKey)
        {
            var currentContractKey = runtimeObject.Contract;
            if (currentContractKey == contractKey)
                return;

            MergeContracts(currentContractKey, contractKey);
            runtimeObject.Contract = contractKey;

            if (!IsContractReferenced(currentContractKey))
                Document.Contracts.Remove(currentContractKey);
        }

        private void MergeContracts(string sourceKey, string targetKey)
        {
            if (sourceKey == targetKey
                || !Document.Contracts.TryGetValue(sourceKey, out var source)
                || !Document.Contracts.TryGetValue(targetKey, out var target))
                return;

            foreach (var member in source.Members)
            {
                if (!target.Members.TryGetValue(member.Key, out var existing))
                {
                    target.Members[member.Key] = member.Value;
                    continue;
                }

                if (member.Value is PropertyMember sourceProperty
                    && existing is PropertyMember targetProperty)
                {
                    targetProperty.Shape = MergeShape(targetProperty.Shape, sourceProperty.Shape);
                    targetProperty.Access = MergeAccess(targetProperty.Access, sourceProperty.Access);
                    continue;
                }

                if (member.Value is MethodMember sourceMethod
                    && existing is MethodMember targetMethod)
                {
                    if (sourceMethod.Args != null && sourceMethod.Args.Count > 0)
                        targetMethod.Args = MergeArgShapes(targetMethod.Args, sourceMethod.Args);

                    targetMethod.Returns ??= sourceMethod.Returns;
                }
            }

            if (source.Events == null || source.Events.Count == 0)
                return;

            target.Events ??= new Dictionary<string, EventContract>(StringComparer.Ordinal);
            foreach (var item in source.Events)
            {
                if (!target.Events.TryGetValue(item.Key, out var existingEvent))
                {
                    target.Events[item.Key] = item.Value;
                    continue;
                }

                existingEvent.Channel = item.Value.Channel;
                existingEvent.EventObject ??= item.Value.EventObject;

                if (item.Value.Data == null || item.Value.Data.Count == 0)
                    continue;

                existingEvent.Data ??= new Dictionary<string, ValueExpr>(StringComparer.Ordinal);
                foreach (var dataItem in item.Value.Data)
                    existingEvent.Data[dataItem.Key] = dataItem.Value;
            }
        }

        private bool IsContractReferenced(string contractKey)
        {
            foreach (var runtimeObject in Document.Objects.Values)
            {
                if (runtimeObject.Contract == contractKey)
                    return true;
            }

            return false;
        }

        private static string MergeAccess(string left, string right)
        {
            if (left == right)
                return left;

            return "readwrite";
        }

        private static ValueShape MergeShape(ValueShape left, ValueShape right)
        {
            if (left is AnyValueShape)
                return right;

            if (right is AnyValueShape)
                return left;

            if (left.GetType() == right.GetType())
                return left;

            return new AnyValueShape();
        }

        private static string BuildMemberName(string rawPath)
        {
            if (rawPath == "textContent")
                return "text";
            if (rawPath == "innerHTML")
                return "html";
            if (rawPath == "classList.add")
                return "classAdd";
            if (rawPath == "classList.remove")
                return "classRemove";
            if (rawPath == "classList.toggle")
                return "classToggle";

            return rawPath;
        }

        private static string LastPathSegment(string path)
        {
            var idx = path.LastIndexOf('.');
            return idx >= 0 ? path.Substring(idx + 1) : path;
        }

        private static string Sanitize(string value)
        {
            return value
                .Replace("::", "_")
                .Replace(".", "_")
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace(" ", "_");
        }

        private static void AddNestedField(Dictionary<string, ValueExpr> fields, string path, ValueExpr value)
        {
            var segments = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
                return;

            var current = fields;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                if (!current.TryGetValue(segments[i], out var existing) || !(existing is ObjectValueExpr objectExpr))
                {
                    objectExpr = new ObjectValueExpr(new Dictionary<string, ValueExpr>(StringComparer.Ordinal));
                    current[segments[i]] = objectExpr;
                }

                current = objectExpr.Fields;
            }

            current[segments[segments.Length - 1]] = value;
        }
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
