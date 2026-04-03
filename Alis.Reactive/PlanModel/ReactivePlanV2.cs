using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    internal sealed class ReactivePlanV2Document
    {
        public int Version { get; } = 2;
        public string PlanId { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SourceId { get; set; }

        public Dictionary<string, CapabilityContract> Contracts { get; }
        public Dictionary<string, RuntimeObject> Objects { get; }
        public Dictionary<string, FieldBinding> Bindings { get; }
        public List<Workflow> Workflows { get; }

        internal ReactivePlanV2Document(
            string planId,
            Dictionary<string, CapabilityContract> contracts,
            Dictionary<string, RuntimeObject> objects,
            Dictionary<string, FieldBinding> bindings,
            List<Workflow> workflows)
        {
            PlanId = planId;
            Contracts = contracts;
            Objects = objects;
            Bindings = bindings;
            Workflows = workflows;
        }
    }

    internal sealed class CapabilityContract
    {
        public string Kind { get; }
        public string Resolver { get; }
        public Dictionary<string, ContractMember> Members { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, EventContract>? Events { get; set; }

        internal CapabilityContract(string kind, string resolver)
        {
            Kind = kind;
            Resolver = resolver;
            Members = new Dictionary<string, ContractMember>();
        }
    }

    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ContractMember>))]
    internal abstract class ContractMember
    {
    }

    internal sealed class PropertyMember : ContractMember
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "property";

        public List<PathSegment> Path { get; }
        public ValueShape Shape { get; set; }
        public string Access { get; set; }

        internal PropertyMember(List<PathSegment> path, ValueShape shape, string access)
        {
            Path = path;
            Shape = shape;
            Access = access;
        }
    }

    internal sealed class MethodMember : ContractMember
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "method";

        public List<PathSegment> Path { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ValueShape>? Args { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Returns { get; set; }

        internal MethodMember(List<PathSegment> path)
        {
            Path = path;
        }
    }

    internal sealed class EventContract
    {
        public string Channel { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public EventObjectReference? EventObject { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, ValueExpr>? Data { get; set; }

        internal EventContract(string channel)
        {
            Channel = channel;
        }
    }

    internal sealed class EventObjectReference
    {
        public string Contract { get; internal set; }

        internal EventObjectReference(string contract)
        {
            Contract = contract;
        }
    }

    internal sealed class RuntimeObject
    {
        public string Contract { get; internal set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ElementId { get; set; }

        internal RuntimeObject(string contract, string? elementId = null)
        {
            Contract = contract;
            ElementId = elementId;
        }
    }

    internal sealed class FieldBinding
    {
        public string Object { get; }
        public string ValueMember { get; }
        public ValueShape Shape { get; }

        internal FieldBinding(string @object, string valueMember, ValueShape shape)
        {
            Object = @object;
            ValueMember = valueMember;
            Shape = shape;
        }
    }

    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ValueShape>))]
    internal abstract class ValueShape
    {
    }

    internal sealed class ScalarValueShape : ValueShape
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "scalar";
        public string Type { get; }

        internal ScalarValueShape(string type)
        {
            Type = type;
        }
    }

    internal sealed class ArrayValueShape : ValueShape
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "array";
        public ValueShape Item { get; }

        internal ArrayValueShape(ValueShape item)
        {
            Item = item;
        }
    }

    internal sealed class ObjectValueShape : ValueShape
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "object";

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, ValueShape>? Fields { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Additional { get; set; }
    }

    internal sealed class AnyValueShape : ValueShape
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "any";
    }

    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ValueExpr>))]
    internal abstract class ValueExpr
    {
    }

    internal sealed class LiteralValueExpr : ValueExpr
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "literal";

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Value { get; }

        internal LiteralValueExpr(object? value)
        {
            Value = value;
        }
    }

    internal sealed class BindingValueExpr : ValueExpr
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "binding";
        public string Binding { get; }

        internal BindingValueExpr(string binding)
        {
            Binding = binding;
        }
    }

    internal sealed class MemberValueExpr : ValueExpr
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "member";
        public string Object { get; }
        public string Member { get; }

        internal MemberValueExpr(string @object, string member)
        {
            Object = @object;
            Member = member;
        }
    }

    internal sealed class ContextValueExpr : ValueExpr
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "context";
        public string Scope { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PathSegment>? Path { get; set; }

        internal ContextValueExpr(string scope, List<PathSegment>? path = null)
        {
            Scope = scope;
            Path = path;
        }
    }

    internal sealed class ObjectValueExpr : ValueExpr
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "object";
        public Dictionary<string, ValueExpr> Fields { get; }

        internal ObjectValueExpr(Dictionary<string, ValueExpr> fields)
        {
            Fields = fields;
        }
    }

    internal sealed class ArrayValueExpr : ValueExpr
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "array";
        public List<ValueExpr> Items { get; }

        internal ArrayValueExpr(List<ValueExpr> items)
        {
            Items = items;
        }
    }

    internal sealed class ConvertValueExpr : ValueExpr
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "convert";
        public ValueExpr Value { get; }
        public ValueShape To { get; }

        internal ConvertValueExpr(ValueExpr value, ValueShape to)
        {
            Value = value;
            To = to;
        }
    }

    internal sealed class BindingMapValueExpr
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "binding-map";
        public object Include { get; }

        internal BindingMapValueExpr(object include)
        {
            Include = include;
        }
    }

    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<PlanPredicate>))]
    internal abstract class PlanPredicate
    {
    }

    internal sealed class ComparePredicate : PlanPredicate
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "compare";
        public ValueExpr Left { get; }
        public string Op { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValueExpr? Right { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValueShape? As { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValueShape? ItemAs { get; set; }

        internal ComparePredicate(ValueExpr left, string op)
        {
            Left = left;
            Op = op;
        }
    }

    internal sealed class AllPredicate : PlanPredicate
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "all";
        public List<PlanPredicate> Terms { get; }

        internal AllPredicate(List<PlanPredicate> terms)
        {
            Terms = terms;
        }
    }

    internal sealed class AnyPredicate : PlanPredicate
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "any";
        public List<PlanPredicate> Terms { get; }

        internal AnyPredicate(List<PlanPredicate> terms)
        {
            Terms = terms;
        }
    }

    internal sealed class NotPredicate : PlanPredicate
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "not";
        public PlanPredicate Term { get; }

        internal NotPredicate(PlanPredicate term)
        {
            Term = term;
        }
    }

    internal sealed class ConfirmPredicate : PlanPredicate
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "confirm";
        public string Message { get; }

        internal ConfirmPredicate(string message)
        {
            Message = message;
        }
    }

    internal sealed class Workflow
    {
        public PlanSubscription When { get; }
        public PlanAction Run { get; }

        internal Workflow(PlanSubscription when, PlanAction run)
        {
            When = when;
            Run = run;
        }
    }

    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<PlanSubscription>))]
    internal abstract class PlanSubscription
    {
    }

    internal sealed class DomReadySubscription : PlanSubscription
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "dom-ready";
    }

    internal sealed class DocumentEventSubscription : PlanSubscription
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "document-event";
        public string Name { get; }

        internal DocumentEventSubscription(string name)
        {
            Name = name;
        }
    }

    internal sealed class ObjectEventSubscription : PlanSubscription
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "object-event";
        public string Object { get; }
        public string Event { get; }

        internal ObjectEventSubscription(string @object, string @event)
        {
            Object = @object;
            Event = @event;
        }
    }

    internal sealed class ServerPushSubscription : PlanSubscription
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "server-push";
        public string Url { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EventType { get; set; }

        internal ServerPushSubscription(string url)
        {
            Url = url;
        }
    }

    internal sealed class SignalRSubscription : PlanSubscription
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "signalr";
        public string HubUrl { get; }
        public string Method { get; }

        internal SignalRSubscription(string hubUrl, string method)
        {
            HubUrl = hubUrl;
            Method = method;
        }
    }

    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<PlanAction>))]
    internal abstract class PlanAction
    {
    }

    internal sealed class SequenceAction : PlanAction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "sequence";
        public List<PlanAction> Steps { get; }

        internal SequenceAction(List<PlanAction> steps)
        {
            Steps = steps;
        }
    }

    internal sealed class BranchAction : PlanAction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "branch";
        public List<BranchCase> Cases { get; }

        internal BranchAction(List<BranchCase> cases)
        {
            Cases = cases;
        }
    }

    internal sealed class BranchCase
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PlanPredicate? When { get; set; }

        public PlanAction Run { get; }

        internal BranchCase(PlanAction run)
        {
            Run = run;
        }
    }

    internal sealed class ParallelAction : PlanAction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "parallel";
        public List<PlanAction> Steps { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PlanAction? OnSettled { get; set; }

        internal ParallelAction(List<PlanAction> steps)
        {
            Steps = steps;
        }
    }

    internal sealed class SetAction : PlanAction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "set";
        public ActionTarget Target { get; }
        public ValueExpr Value { get; }

        internal SetAction(ActionTarget target, ValueExpr value)
        {
            Target = target;
            Value = value;
        }
    }

    internal sealed class CallAction : PlanAction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "call";
        public ActionTarget Target { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ValueExpr>? Args { get; set; }

        internal CallAction(ActionTarget target)
        {
            Target = target;
        }
    }

    internal sealed class DispatchAction : PlanAction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "dispatch";
        public string Name { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValueExpr? Detail { get; set; }

        internal DispatchAction(string name)
        {
            Name = name;
        }
    }

    internal sealed class RequestAction : PlanAction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "request";
        public RequestPlan Request { get; }

        internal RequestAction(RequestPlan request)
        {
            Request = request;
        }
    }

    internal sealed class InjectAction : PlanAction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "inject";
        public string Object { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValueExpr? Value { get; set; }

        internal InjectAction(string @object)
        {
            Object = @object;
        }
    }

    internal sealed class ShowValidationErrorsAction : PlanAction
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "show-validation-errors";
        public string FormId { get; }

        internal ShowValidationErrorsAction(string formId)
        {
            FormId = formId;
        }
    }

    internal sealed class ActionTarget
    {
        public string Object { get; }
        public string Member { get; }

        internal ActionTarget(string @object, string member)
        {
            Object = @object;
            Member = member;
        }
    }

    internal sealed class RequestPlan
    {
        public string Method { get; }
        public string Url { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RequestInput? Input { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RequestValidation? Validation { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PlanAction>? Before { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ResponseHandlerPlan>? OnSuccess { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ResponseHandlerPlan>? OnError { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<PlanAction>? OnSettled { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RequestPlan? Next { get; set; }

        internal RequestPlan(string method, string url)
        {
            Method = method;
            Url = url;
        }
    }

    internal sealed class RequestInput
    {
        public string Transport { get; }
        public object Value { get; }

        internal RequestInput(string transport, object value)
        {
            Transport = transport;
            Value = value;
        }
    }

    internal sealed class RequestValidation
    {
        public string FormId { get; }
        public IReadOnlyList<RequestValidationField> Fields { get; }

        internal RequestValidation(string formId, List<RequestValidationField> fields)
        {
            FormId = formId;
            Fields = fields.ToArray();
        }
    }

    internal sealed class RequestValidationField
    {
        public string Binding { get; }
        public IReadOnlyList<RequestValidationRule> Rules { get; }

        internal RequestValidationField(string binding, List<RequestValidationRule> rules)
        {
            Binding = binding;
            Rules = rules.ToArray();
        }
    }

    internal sealed class RequestValidationRule
    {
        public string Rule { get; }
        public string Message { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Constraint { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? OtherBinding { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValueShape? As { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PlanPredicate? When { get; set; }

        internal RequestValidationRule(string rule, string message)
        {
            Rule = rule;
            Message = message;
        }
    }

    internal sealed class ResponseHandlerPlan
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? StatusCode { get; set; }

        public PlanAction Run { get; }

        internal ResponseHandlerPlan(PlanAction run)
        {
            Run = run;
        }
    }

    internal sealed class PathSegment
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Prop { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Index { get; set; }

        internal static PathSegment FromProp(string prop)
        {
            return new PathSegment { Prop = prop };
        }

        internal static PathSegment FromIndex(int index)
        {
            return new PathSegment { Index = index };
        }
    }
}
