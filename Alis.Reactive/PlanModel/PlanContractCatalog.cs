using System;
using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    internal sealed class PlanContractCatalog
    {
        private readonly AuthoredPlan _plan;
        private readonly Dictionary<string, ComponentRegistration> _components =
            new Dictionary<string, ComponentRegistration>(StringComparer.Ordinal);
        private readonly Dictionary<string, ComponentRegistration> _componentsById =
            new Dictionary<string, ComponentRegistration>(StringComparer.Ordinal);

        internal PlanContractCatalog(AuthoredPlan plan)
        {
            _plan = plan;
        }

        internal IReadOnlyDictionary<string, ComponentRegistration> Components => _components;

        internal void RegisterComponent(ComponentRegistration registration)
        {
            _components[registration.BindingPath] = registration;
            _componentsById[registration.ComponentId] = registration;

            var objectName = EnsureRegisteredComponentObject(registration);
            var memberName = EnsurePropertyMember(
                _plan.Objects[objectName].Contract,
                registration.Binding,
                registration.BindingShape,
                "read");

            _plan.Bindings[registration.BindingPath] = new FieldBinding(objectName, memberName, registration.BindingShape);
        }

        internal ComponentRegistration? GetRegistration(string componentId)
        {
            if (_componentsById.TryGetValue(componentId, out var registration))
                return registration;

            return null;
        }

        internal string EnsureRegisteredComponentObject(ComponentRegistration registration)
        {
            var objectName = ComponentObjectName(registration.ComponentId);
            var contractKey = ContractKey(registration.Component, registration.ComponentId);
            EnsureContract(contractKey, "component", ResolverForVendor(registration.Component.Vendor));

            if (_plan.Objects.TryGetValue(objectName, out var existingObject))
            {
                PromoteObjectContract(existingObject, contractKey);
                existingObject.ElementId ??= registration.ComponentId;
            }
            else
            {
                _plan.Objects[objectName] = new RuntimeObject(contractKey, registration.ComponentId);
            }

            return objectName;
        }

        internal string EnsureComponentObject(string componentId, ComponentMetadata component, ComponentRegistration? registration)
        {
            if (registration != null)
                return EnsureRegisteredComponentObject(registration);

            var objectName = ComponentObjectName(componentId);
            var contractKey = ContractKey(component, componentId);
            EnsureObject(objectName, contractKey, "component", ResolverForVendor(component.Vendor), componentId);
            return objectName;
        }

        internal string EnsureElementObject(string elementId)
        {
            var objectName = ElementObjectName(elementId);
            var contractKey = "element." + Sanitize(elementId);
            EnsureObject(objectName, contractKey, "element", "native-element", elementId);
            return objectName;
        }

        internal EventContract EnsureEvent(string contractKey, string eventName, string channel)
        {
            var contract = _plan.Contracts[contractKey];
            if (contract.Events == null)
                contract.Events = new Dictionary<string, EventContract>(StringComparer.Ordinal);

            if (!contract.Events.TryGetValue(eventName, out var eventContract))
            {
                eventContract = new EventContract(channel);
                contract.Events[eventName] = eventContract;
            }
            else
            {
                if (!string.Equals(eventContract.Channel, channel, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Contract '{contractKey}' event '{eventName}' channel '{eventContract.Channel}' conflicts with '{channel}'.");
                }
            }

            return eventContract;
        }

        internal string EnsureSharedEventObjectContract(string contractKey, string eventName, EventContract eventContract)
        {
            var eventObjectContractKey = "event-object."
                + Sanitize(contractKey)
                + "."
                + Sanitize(eventName);

            if (!_plan.Contracts.ContainsKey(eventObjectContractKey))
                _plan.Contracts[eventObjectContractKey] = new CapabilityContract("event-object", "event-object");

            if (eventContract.EventObject != null
                && !string.Equals(eventContract.EventObject.Contract, eventObjectContractKey, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Event object contract '{eventContract.EventObject.Contract}' conflicts with '{eventObjectContractKey}'.");
            }

            eventContract.EventObject = new EventObjectReference(eventObjectContractKey);
            return eventObjectContractKey;
        }

        internal string EnsurePropertyMember(
            string contractKey,
            CapabilityProperty member,
            ValueShape shape,
            string access)
        {
            return EnsurePropertyMember(contractKey, member, shape, access, allowAccessPromotion: true);
        }

        internal string EnsureEventPropertyMember(
            string contractKey,
            CapabilityProperty member,
            ValueShape shape,
            string access)
        {
            return EnsurePropertyMember(contractKey, member, shape, access, allowAccessPromotion: false);
        }

        private string EnsurePropertyMember(
            string contractKey,
            CapabilityProperty member,
            ValueShape shape,
            string access,
            bool allowAccessPromotion)
        {
            var memberName = member.Name;
            var contract = _plan.Contracts[contractKey];
            if (contract.Members.TryGetValue(memberName, out var existingMember))
            {
                if (existingMember is PropertyMember property)
                {
                    if (!CapabilityPath.Same(property.Path, member.Path))
                    {
                        throw new InvalidOperationException(
                            $"Contract '{contractKey}' already defines property '{memberName}' with path '{CapabilityPath.Format(property.Path)}', which conflicts with '{CapabilityPath.Format(member.Path)}'.");
                    }

                    property.Shape = MergeShape(property.Shape, shape);
                    property.Access = MergeAccess(
                        property.Access,
                        access,
                        allowAccessPromotion,
                        contractKey,
                        memberName);
                    return memberName;
                }

                throw new InvalidOperationException(
                    $"Contract '{contractKey}' already defines member '{memberName}' as a method.");
            }

            contract.Members[memberName] = new PropertyMember(CapabilityPath.Clone(member.Path), shape, access);
            return memberName;
        }

        internal string EnsureMethodMember(
            string contractKey,
            CapabilityMethod member,
            List<ValueShape>? args)
        {
            var memberName = member.Name;
            var contract = _plan.Contracts[contractKey];
            if (contract.Members.TryGetValue(memberName, out var existingMember))
            {
                if (existingMember is MethodMember method)
                {
                    if (args != null && args.Count > 0)
                        method.Args = MergeArgShapes(method.Args, args);

                    method.Returns = MergeMethodReturn(method.Returns, "void", contractKey, memberName);
                    return memberName;
                }

                throw new InvalidOperationException(
                    $"Contract '{contractKey}' already defines member '{memberName}' as a property.");
            }

            var created = new MethodMember(CapabilityPath.Clone(member.Path))
            {
                Args = args,
                Returns = "void"
            };

            contract.Members[memberName] = created;
            return memberName;
        }

        private void EnsureObject(string objectName, string contractKey, string kind, string resolver, string? elementId)
        {
            EnsureContract(contractKey, kind, resolver);

            if (!_plan.Objects.ContainsKey(objectName))
                _plan.Objects[objectName] = new RuntimeObject(contractKey, elementId);
        }

        private void EnsureContract(string contractKey, string kind, string resolver)
        {
            if (!_plan.Contracts.ContainsKey(contractKey))
                _plan.Contracts[contractKey] = new CapabilityContract(kind, resolver);
        }

        private static string ComponentObjectName(string componentId)
        {
            return "component::" + componentId;
        }

        private static string ElementObjectName(string elementId)
        {
            return "element::" + elementId;
        }

        private static string ContractKey(ComponentMetadata component, string componentId)
        {
            if (!string.IsNullOrWhiteSpace(component.Kind))
                return component.Vendor + "." + component.Kind;

            return component.Vendor + ".component." + Sanitize(componentId);
        }

        private static string ResolverForVendor(string vendor)
        {
            switch (vendor)
            {
                case "fusion":
                    return "fusion-instance";
                case "native":
                    return "native-element";
                default:
                    throw new InvalidOperationException(
                        $"Unsupported component vendor '{vendor}'.");
            }
        }

        private void PromoteObjectContract(RuntimeObject runtimeObject, string contractKey)
        {
            var currentContractKey = runtimeObject.Contract;
            if (currentContractKey == contractKey)
                return;

            MergeContracts(currentContractKey, contractKey);
            runtimeObject.Contract = contractKey;

            if (!IsContractReferenced(currentContractKey))
                _plan.Contracts.Remove(currentContractKey);
        }

        private void MergeContracts(string sourceKey, string targetKey)
        {
            if (sourceKey == targetKey
                || !_plan.Contracts.TryGetValue(sourceKey, out var source)
                || !_plan.Contracts.TryGetValue(targetKey, out var target))
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
                    if (!CapabilityPath.Same(targetProperty.Path, sourceProperty.Path))
                    {
                        throw new InvalidOperationException(
                            $"Contract '{targetKey}' property '{member.Key}' path '{CapabilityPath.Format(targetProperty.Path)}' conflicts with merged path '{CapabilityPath.Format(sourceProperty.Path)}'.");
                    }

                    targetProperty.Shape = MergeShape(targetProperty.Shape, sourceProperty.Shape);
                    targetProperty.Access = MergeAccess(
                        targetProperty.Access,
                        sourceProperty.Access,
                        allowAccessPromotion: !IsEventObjectContract(source) && !IsEventObjectContract(target),
                        targetKey,
                        member.Key);
                    continue;
                }

                if (member.Value is MethodMember sourceMethod
                    && existing is MethodMember targetMethod)
                {
                    if (sourceMethod.Args != null && sourceMethod.Args.Count > 0)
                        targetMethod.Args = MergeArgShapes(targetMethod.Args, sourceMethod.Args);

                    targetMethod.Returns = MergeMethodReturn(
                        targetMethod.Returns,
                        sourceMethod.Returns,
                        targetKey,
                        member.Key);
                    continue;
                }

                throw new InvalidOperationException(
                    $"Contract '{targetKey}' cannot merge member '{member.Key}' with conflicting member kinds.");
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

                if (!string.Equals(existingEvent.Channel, item.Value.Channel, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Contract '{targetKey}' event '{item.Key}' channel '{existingEvent.Channel}' conflicts with '{item.Value.Channel}'.");
                }

                if (existingEvent.EventObject == null)
                {
                    existingEvent.EventObject = item.Value.EventObject;
                }
                else if (item.Value.EventObject != null
                    && !string.Equals(existingEvent.EventObject.Contract, item.Value.EventObject.Contract, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Contract '{targetKey}' event '{item.Key}' event object '{existingEvent.EventObject.Contract}' conflicts with '{item.Value.EventObject.Contract}'.");
                }

                if (item.Value.Data == null || item.Value.Data.Count == 0)
                    continue;

                existingEvent.Data ??= new Dictionary<string, ValueExpr>(StringComparer.Ordinal);
                foreach (var dataItem in item.Value.Data)
                {
                    if (existingEvent.Data.TryGetValue(dataItem.Key, out var existingExpr)
                        && !ValueExprEquivalent(existingExpr, dataItem.Value))
                    {
                        throw new InvalidOperationException(
                            $"Contract '{targetKey}' event '{item.Key}' data member '{dataItem.Key}' conflicts with an existing value expression.");
                    }

                    existingEvent.Data[dataItem.Key] = dataItem.Value;
                }
            }
        }

        private bool IsContractReferenced(string contractKey)
        {
            return _plan.Objects.Values.Any(runtimeObject => runtimeObject.Contract == contractKey);
        }

        private static List<ValueShape>? MergeArgShapes(List<ValueShape>? current, List<ValueShape> next)
        {
            if (current == null || current.Count == 0)
                return next;

            if (current.Count != next.Count)
            {
                throw new InvalidOperationException(
                    $"Method argument shape mismatch. Existing arity {current.Count}, incoming arity {next.Count}.");
            }

            var merged = new List<ValueShape>();
            for (var i = 0; i < current.Count; i++)
                merged.Add(MergeShape(current[i], next[i]));

            return merged;
        }

        private static bool ValueExprEquivalent(ValueExpr left, ValueExpr right)
        {
            if (ReferenceEquals(left, right))
                return true;

            switch (left)
            {
                case LiteralValueExpr leftLiteral when right is LiteralValueExpr rightLiteral:
                    return Equals(leftLiteral.Value, rightLiteral.Value);

                case BindingValueExpr leftBinding when right is BindingValueExpr rightBinding:
                    return string.Equals(leftBinding.Binding, rightBinding.Binding, StringComparison.Ordinal);

                case MemberValueExpr leftMember when right is MemberValueExpr rightMember:
                    return string.Equals(leftMember.Object, rightMember.Object, StringComparison.Ordinal)
                        && string.Equals(leftMember.Member, rightMember.Member, StringComparison.Ordinal);

                case ContextValueExpr leftContext when right is ContextValueExpr rightContext:
                    return string.Equals(leftContext.Scope, rightContext.Scope, StringComparison.Ordinal)
                        && PathsEquivalent(leftContext.Path, rightContext.Path);

                case AccessValueExpr leftAccess when right is AccessValueExpr rightAccess:
                    return ValueExprEquivalent(leftAccess.Value, rightAccess.Value)
                        && PathsEquivalent(leftAccess.Path, rightAccess.Path);

                case ObjectValueExpr leftObject when right is ObjectValueExpr rightObject:
                    return DictionaryEquivalent(leftObject.Fields, rightObject.Fields);

                case ArrayValueExpr leftArray when right is ArrayValueExpr rightArray:
                    return ListEquivalent(leftArray.Items, rightArray.Items);

                case ConvertValueExpr leftConvert when right is ConvertValueExpr rightConvert:
                    return ValueExprEquivalent(leftConvert.Value, rightConvert.Value)
                        && ValueShapeFactory.AreEquivalent(leftConvert.To, rightConvert.To);

                case BindingMapValueExpr leftBindingMap when right is BindingMapValueExpr rightBindingMap:
                    return BindingMapEquivalent(leftBindingMap.Include, rightBindingMap.Include);

                default:
                    return false;
            }
        }

        private static bool PathsEquivalent(IReadOnlyList<PathSegment>? left, IReadOnlyList<PathSegment>? right)
        {
            if (left == null || right == null)
                return left == right;

            return CapabilityPath.Same(left, right);
        }

        private static bool DictionaryEquivalent(
            IReadOnlyDictionary<string, ValueExpr> left,
            IReadOnlyDictionary<string, ValueExpr> right)
        {
            if (left.Count != right.Count)
                return false;

            foreach (var item in left)
            {
                if (!right.TryGetValue(item.Key, out var rightExpr)
                    || !ValueExprEquivalent(item.Value, rightExpr))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ListEquivalent(IReadOnlyList<ValueExpr> left, IReadOnlyList<ValueExpr> right)
        {
            if (left.Count != right.Count)
                return false;

            for (var index = 0; index < left.Count; index++)
            {
                if (!ValueExprEquivalent(left[index], right[index]))
                    return false;
            }

            return true;
        }

        private static bool BindingMapEquivalent(object left, object right)
        {
            if (left is string leftString && right is string rightString)
                return string.Equals(leftString, rightString, StringComparison.Ordinal);

            if (left is IReadOnlyList<string> leftList && right is IReadOnlyList<string> rightList)
            {
                if (leftList.Count != rightList.Count)
                    return false;

                for (var index = 0; index < leftList.Count; index++)
                {
                    if (!string.Equals(leftList[index], rightList[index], StringComparison.Ordinal))
                        return false;
                }

                return true;
            }

            return Equals(left, right);
        }

        private static object? MergeMethodReturn(object? current, object? incoming, string contractKey, string memberName)
        {
            if (current == null)
                return incoming;

            if (incoming == null)
                return current;

            if (ReferenceEquals(current, incoming) || Equals(current, incoming))
                return current;

            if (current is ValueShape currentShape && incoming is ValueShape incomingShape)
                return MergeShape(currentShape, incomingShape);

            throw new InvalidOperationException(
                $"Contract '{contractKey}' declares incompatible return shapes for method '{memberName}'.");
        }

        private static bool IsEventObjectContract(CapabilityContract contract)
        {
            return string.Equals(contract.Kind, "event-object", StringComparison.Ordinal);
        }

        private static string MergeAccess(
            string left,
            string right,
            bool allowAccessPromotion,
            string contractKey,
            string memberName)
        {
            if (left == right)
                return left;

            if (allowAccessPromotion
                && ((left == "read" && right == "write")
                || (left == "write" && right == "read")
                || (left == "readwrite" && (right == "read" || right == "write"))
                || (right == "readwrite" && (left == "read" || left == "write"))))
            {
                return "readwrite";
            }

            throw new InvalidOperationException(
                $"Contract '{contractKey}' property '{memberName}' declares incompatible access '{left}' and '{right}'.");
        }

        private static ValueShape MergeShape(ValueShape left, ValueShape right)
        {
            if (ValueShapeFactory.AreEquivalent(left, right))
                return left;

            throw new InvalidOperationException(
                $"Conflicting value shapes '{ValueShapeFactory.Describe(left)}' and '{ValueShapeFactory.Describe(right)}' cannot be merged.");
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
    }
}
