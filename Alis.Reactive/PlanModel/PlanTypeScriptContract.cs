using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Authoritative TypeScript contract for the plan JSON executed by the browser runtime.
    /// Kept next to the plan domain so runtime types are generated from the plan model.
    /// </summary>
    internal static class PlanTypeScriptContract
    {
        internal static string Render()
        {
            return CreateContract().Render();
        }

        private static TypeScriptContract CreateContract()
        {
            var contract = TypeScriptContract.GeneratedBy(
                "Alis.Reactive.PlanModel.PlanTypeScriptContract",
                "npm run generate:plan-types -w Alis.Reactive.Assets");

            contract.Declare(Interface("PlanDocument")
                .Requires("version", "3")
                .Requires("planId", "string")
                .Requires("scope", "PlanScope")
                .Requires("types", "Record<string, BrowserObjectContract>")
                .Requires("components", "Record<string, ComponentObject>")
                .Requires("behaviors", "Behavior[]"));

            contract.Declare(Union("PlanScope", "RootPlanScope", "PartialPlanScope"));

            contract.Declare(Interface("RootPlanScope")
                .Requires("kind", Literal("root")));

            contract.Declare(Interface("PartialPlanScope")
                .Requires("kind", Literal("partial")));

            contract.Declare(Interface("BrowserObjectContract")
                .Requires("properties", "Record<string, Property>")
                .Requires("methods", "Record<string, Method>")
                .Requires("events", "Record<string, Event>"));

            contract.Declare(Interface("Property")
                .Requires("path", "Path")
                .Requires("shape", "Shape")
                .Requires("access", "MemberAccess"));

            contract.Declare(LiteralUnion("MemberAccess", MemberAccess.Values));

            contract.Declare(Interface("Method")
                .Requires("path", "Path")
                .Requires("arguments", "MethodArgumentContract")
                .Requires("returns", "Shape"));

            contract.Declare(Union(
                "MethodArgumentContract",
                "OpenMethodArgumentContract",
                "ExactMethodArgumentContract"));
            contract.Declare(Interface("OpenMethodArgumentContract")
                .Requires("kind", Literal("open")));
            contract.Declare(Interface("ExactMethodArgumentContract")
                .Requires("kind", Literal("exact"))
                .Requires("shapes", "Shape[]"));

            contract.Declare(Union("PayloadContract", "UntypedPayloadContract", "TypedPayloadContract"));
            contract.Declare(Interface("UntypedPayloadContract")
                .Requires("kind", Literal("untyped")));
            contract.Declare(Interface("TypedPayloadContract")
                .Requires("kind", Literal("typed"))
                .Requires("type", "string"));

            contract.Declare(Interface("Event")
                .Requires("channel", "string")
                .Requires("payloadType", "PayloadContract"));

            contract.Declare(Alias("Vendor", "string"));

            contract.Declare(Union(
                "ComponentObject",
                "ObjectTargetComponent",
                "PlanInputComponent",
                "ValidationContainerComponentDefinition",
                "LayoutObjectComponent"));
            contract.Declare(ComponentVariant(
                "ObjectTargetComponent",
                "ObjectTargetComponentRole",
                "NoInputBinding",
                "NoValidationContainer"));
            contract.Declare(ComponentVariant(
                "PlanInputComponent",
                "PlanInputComponentRole",
                "RegisteredInputBinding",
                "NoValidationContainer"));
            contract.Declare(ComponentVariant(
                "ValidationContainerComponentDefinition",
                "ValidationContainerComponentRole",
                "InputBinding",
                "ValidationContainerScope"));
            contract.Declare(ComponentVariant(
                "LayoutObjectComponent",
                "LayoutObjectComponentRole",
                "NoInputBinding",
                "NoValidationContainer"));

            contract.Declare(Union(
                "ComponentRole",
                "ObjectTargetComponentRole",
                "PlanInputComponentRole",
                "ValidationContainerComponentRole",
                "LayoutObjectComponentRole"));
            contract.Declare(Interface("ObjectTargetComponentRole")
                .Requires("kind", Literal("object-target")));
            contract.Declare(Interface("PlanInputComponentRole")
                .Requires("kind", Literal("plan-input")));
            contract.Declare(Interface("ValidationContainerComponentRole")
                .Requires("kind", Literal("validation-container")));
            contract.Declare(Interface("LayoutObjectComponentRole")
                .Requires("kind", Literal("layout-object")));

            contract.Declare(Union("InputBinding", "NoInputBinding", "RegisteredInputBinding"));
            contract.Declare(Interface("NoInputBinding")
                .Requires("kind", Literal("none")));
            contract.Declare(Interface("RegisteredInputBinding")
                .Requires("kind", Literal("registered-input"))
                .Requires("bindingPath", "string")
                .Requires("path", "StructuredPath")
                .Requires("valueMember", "string"));

            contract.Declare(Union(
                "ValidationContainerBinding",
                "NoValidationContainer",
                "ValidationContainerScope"));
            contract.Declare(Interface("NoValidationContainer")
                .Requires("kind", Literal("none")));
            contract.Declare(Interface("ValidationContainerScope")
                .Requires("kind", Literal("validation-container"))
                .Requires("validationRules", "ComponentValidation[]"));

            contract.Declare(Interface("ComponentValidation")
                .Requires("component", "string")
                .Requires("value", "ValueExpression")
                .Requires("serverFieldName", "string")
                .Requires("rules", "ValidationRule[]"));

            contract.Declare(LiteralUnion("ValidationRuleName", ValidationRuleName.Values));

            contract.Declare(Union(
                "ValidationRule",
                "NoOperandValidationRule",
                "LengthValidationRule",
                "RegexValidationRule",
                "RangeValidationRule",
                "OrderedComparisonValidationRule",
                "PeerOrderedComparisonValidationRule",
                "LiteralEqualityValidationRule",
                "PeerEqualityValidationRule"));

            contract.Declare(LiteralUnion(
                "NoOperandValidationRuleName",
                new[] { "required", "empty", "email", "url", "creditCard", "atLeastOne" }));
            contract.Declare(LiteralUnion("LengthValidationRuleName", new[] { "minLength", "maxLength" }));
            contract.Declare(LiteralUnion("RegexValidationRuleName", new[] { "regex" }));
            contract.Declare(LiteralUnion("RangeValidationRuleName", new[] { "range", "exclusiveRange" }));
            contract.Declare(LiteralUnion("OrderedComparisonValidationRuleName", new[] { "min", "max", "gt", "lt" }));
            contract.Declare(LiteralUnion("PeerOrderedComparisonValidationRuleName", new[] { "min", "max", "gt", "lt" }));
            contract.Declare(LiteralUnion("LiteralEqualityValidationRuleName", new[] { "equalTo", "notEqual" }));
            contract.Declare(LiteralUnion("PeerEqualityValidationRuleName", new[] { "equalTo", "notEqualTo" }));

            contract.Declare(Interface("NoOperandValidationRule")
                .Requires("name", "NoOperandValidationRuleName")
                .Requires("message", "string")
                .Requires("execution", "NoOperandValidationRuleExecution"));

            contract.Declare(Interface("LengthValidationRule")
                .Requires("name", "LengthValidationRuleName")
                .Requires("message", "string")
                .Requires("execution", "NumericConstraintValidationRuleExecution"));

            contract.Declare(Interface("RegexValidationRule")
                .Requires("name", "RegexValidationRuleName")
                .Requires("message", "string")
                .Requires("execution", "TextConstraintValidationRuleExecution"));

            contract.Declare(Interface("RangeValidationRule")
                .Requires("name", "RangeValidationRuleName")
                .Requires("message", "string")
                .Requires("execution", "RangeConstraintValidationRuleExecution"));

            contract.Declare(Interface("OrderedComparisonValidationRule")
                .Requires("name", "OrderedComparisonValidationRuleName")
                .Requires("message", "string")
                .Requires("execution", "ScalarConstraintValidationRuleExecution"));

            contract.Declare(Interface("PeerOrderedComparisonValidationRule")
                .Requires("name", "PeerOrderedComparisonValidationRuleName")
                .Requires("message", "string")
                .Requires("execution", "PeerValidationRuleExecution"));

            contract.Declare(Interface("LiteralEqualityValidationRule")
                .Requires("name", "LiteralEqualityValidationRuleName")
                .Requires("message", "string")
                .Requires("execution", "ScalarConstraintValidationRuleExecution"));

            contract.Declare(Interface("PeerEqualityValidationRule")
                .Requires("name", "PeerEqualityValidationRuleName")
                .Requires("message", "string")
                .Requires("execution", "PeerValidationRuleExecution"));

            contract.Declare(Union(
                "ValidationRuleExecution",
                "NoOperandValidationRuleExecution",
                "ScalarConstraintValidationRuleExecution",
                "NumericConstraintValidationRuleExecution",
                "TextConstraintValidationRuleExecution",
                "RangeConstraintValidationRuleExecution",
                "PeerValidationRuleExecution"));

            contract.Declare(Interface("NoOperandValidationRuleExecution")
                .Requires("kind", Literal("none"))
                .Requires("activation", "ValidationRuleActivation")
                .Requires("comparisonShape", "Shape"));

            contract.Declare(Interface("ScalarConstraintValidationRuleExecution")
                .Requires("kind", Literal("constraint"))
                .Requires("value", "LiteralExpression")
                .Requires("activation", "ValidationRuleActivation")
                .Requires("comparisonShape", "Shape"));

            contract.Declare(Interface("NumericConstraintValidationRuleExecution")
                .Requires("kind", Literal("constraint"))
                .Requires("value", "NumericLiteralExpression")
                .Requires("activation", "ValidationRuleActivation")
                .Requires("comparisonShape", "Shape"));

            contract.Declare(Interface("TextConstraintValidationRuleExecution")
                .Requires("kind", Literal("constraint"))
                .Requires("value", "TextLiteralExpression")
                .Requires("activation", "ValidationRuleActivation")
                .Requires("comparisonShape", "Shape"));

            contract.Declare(Interface("RangeConstraintValidationRuleExecution")
                .Requires("kind", Literal("constraint"))
                .Requires("value", "RangeLiteralExpression")
                .Requires("activation", "ValidationRuleActivation")
                .Requires("comparisonShape", "Shape"));

            contract.Declare(Interface("PeerValidationRuleExecution")
                .Requires("kind", Literal("peer"))
                .Requires("value", "ReadExpression")
                .Requires("activation", "ValidationRuleActivation")
                .Requires("comparisonShape", "Shape"));

            contract.Declare(Union(
                "ValidationRuleActivation",
                "AlwaysValidationRuleActivation",
                "ConditionalValidationRuleActivation"));

            contract.Declare(Interface("AlwaysValidationRuleActivation")
                .Requires("kind", Literal("always")));

            contract.Declare(Interface("ConditionalValidationRuleActivation")
                .Requires("kind", Literal("when"))
                .Requires("condition", "ValidationCondition"));

            contract.Declare(Union("Source", "ComponentSource", "PayloadSource", "UrlSource", "PluginSource"));
            contract.Declare(Union("RuntimeObjectSource", "ComponentSource", "PluginSource"));
            contract.Declare(Union("SetTargetSource", "ComponentSource", "PayloadSource"));
            contract.Declare(Union("CallTargetSource", "ComponentSource", "PayloadSource", "PluginSource"));

            contract.Declare(Interface("ComponentSource")
                .Requires("kind", Literal("component"))
                .Requires("component", "string"));

            contract.Declare(Interface("UrlSource")
                .Requires("kind", Literal("url")));

            contract.Declare(Interface("PluginSource")
                .Requires("kind", Literal("plugin"))
                .Requires("name", "string")
                .Requires("type", "string"));

            contract.Declare(LiteralUnion("PayloadScope", PayloadScope.Values));

            contract.Declare(Interface("PayloadSource")
                .Requires("kind", Literal("payload"))
                .Requires("scope", "PayloadScope")
                .Requires("type", "PayloadContract"));

            contract.Declare(Interface("Behavior")
                .Requires("startsWhen", "StartsWhen")
                .Requires("reaction", "ReactionGraph"));

            contract.Declare(Union(
                "StartsWhen",
                "PageReadyTrigger",
                "DocumentEventTrigger",
                "ComponentEventTrigger",
                "ServerPushTrigger",
                "SignalRTrigger"));

            contract.Declare(Interface("PageReadyTrigger")
                .Requires("kind", Literal("page-ready")));

            contract.Declare(Interface("DocumentEventTrigger")
                .Requires("kind", Literal("document-event"))
                .Requires("event", "string")
                .Requires("payloadType", "PayloadContract"));

            contract.Declare(Interface("ComponentEventTrigger")
                .Requires("kind", Literal("component-event"))
                .Requires("component", "string")
                .Requires("event", "string"));

            contract.Declare(Interface("ServerPushTrigger")
                .Requires("kind", Literal("server-push"))
                .Requires("url", "string")
                .Requires("eventFilter", "ServerPushEventFilter"));

            contract.Declare(Union(
                "ServerPushEventFilter",
                "AnyServerPushEventFilter",
                "NamedServerPushEventFilter"));
            contract.Declare(Interface("AnyServerPushEventFilter")
                .Requires("kind", Literal("any"))
                .Requires("payloadType", "PayloadContract"));
            contract.Declare(Interface("NamedServerPushEventFilter")
                .Requires("kind", Literal("named"))
                .Requires("event", "string")
                .Requires("payloadType", "PayloadContract"));

            contract.Declare(Interface("SignalRTrigger")
                .Requires("kind", Literal("signalr"))
                .Requires("hubUrl", "string")
                .Requires("method", "string")
                .Requires("payloadType", "PayloadContract"));

            contract.Declare(Union(
                "ReactionGraph",
                "SequenceReaction",
                "ParallelReaction",
                "BranchReaction",
                "SetReaction",
                "CallReaction",
                "RequestReaction",
                "DispatchReaction",
                "InjectReaction",
                "ShowValidationErrorsReaction"));

            contract.Declare(Interface("SequenceReaction")
                .Requires("kind", Literal("sequence"))
                .Requires("steps", "ReactionGraph[]"));

            contract.Declare(Interface("ParallelReaction")
                .Requires("kind", Literal("parallel"))
                .Requires("steps", "ReactionGraph[]")
                .Requires("completion", "ParallelCompletion"));

            contract.Declare(Union(
                "ParallelCompletion",
                "NoParallelCompletion",
                "SettledParallelCompletion"));

            contract.Declare(Interface("NoParallelCompletion")
                .Requires("kind", Literal("none")));

            contract.Declare(Interface("SettledParallelCompletion")
                .Requires("kind", Literal("on-settled"))
                .Requires("reaction", "ReactionGraph"));

            contract.Declare(Interface("BranchReaction")
                .Requires("kind", Literal("branch"))
                .Requires("cases", "BranchCase[]"));

            contract.Declare(Interface("BranchCase")
                .Requires("guard", "BranchGuard")
                .Requires("reaction", "ReactionGraph"));

            contract.Declare(Union("BranchGuard", "DefaultBranchGuard", "ConditionalBranchGuard"));
            contract.Declare(Interface("DefaultBranchGuard")
                .Requires("kind", Literal("default")));
            contract.Declare(Interface("ConditionalBranchGuard")
                .Requires("kind", Literal("when"))
                .Requires("condition", "ConditionGraph"));

            contract.Declare(Interface("SetReaction")
                .Requires("kind", Literal("set"))
                .Requires("on", "SetTargetSource")
                .Requires("property", "string")
                .Requires("value", "ValueExpression"));

            contract.Declare(Interface("CallReaction")
                .Requires("kind", Literal("call"))
                .Requires("on", "CallTargetSource")
                .Requires("method", "string")
                .Requires("args", "ValueExpression[]"));

            contract.Declare(Interface("RequestReaction")
                .Requires("kind", Literal("request"))
                .Requires("request", "RequestPlan"));

            contract.Declare(Interface("DispatchReaction")
                .Requires("kind", Literal("dispatch"))
                .Requires("event", "string")
                .Requires("payload", "DispatchPayload"));

            contract.Declare(Union(
                "DispatchPayload",
                "NoDispatchPayload",
                "PresentDispatchPayload"));

            contract.Declare(Interface("NoDispatchPayload")
                .Requires("kind", Literal("none")));

            contract.Declare(Interface("PresentDispatchPayload")
                .Requires("kind", Literal("value"))
                .Requires("data", "ValueExpression")
                .Requires("payloadType", "PayloadContract"));

            contract.Declare(Interface("InjectReaction")
                .Requires("kind", Literal("inject"))
                .Requires("target", "InjectionTarget")
                .Requires("value", "ValueExpression"));

            contract.Declare(Union(
                "InjectionTarget",
                "PartialSlotInjectionTarget"));

            contract.Declare(Interface("PartialSlotInjectionTarget")
                .Requires("kind", Literal("partial-slot"))
                .Requires("component", "string"));

            contract.Declare(Interface("ShowValidationErrorsReaction")
                .Requires("kind", Literal("show-validation-errors"))
                .Requires("container", "string"));

            contract.Declare(LiteralUnion("HttpMethod", HttpMethodName.Values));

            contract.Declare(Interface("RequestPlan")
                .Requires("method", "HttpMethod")
                .Requires("url", "string")
                .Requires("validation", "RequestValidationTarget")
                .Requires("input", "RequestInput")
                .Requires("whileLoading", "ReactionGraph[]")
                .Requires("success", "ResponseRoute[]")
                .Requires("error", "ResponseRoute[]")
                .Requires("finally", "ReactionGraph[]")
                .Requires("chain", "RequestChain"));

            contract.Declare(Union(
                "RequestValidationTarget",
                "NoRequestValidationTarget",
                "ContainerRequestValidationTarget"));

            contract.Declare(Interface("NoRequestValidationTarget")
                .Requires("kind", Literal("none")));

            contract.Declare(Interface("ContainerRequestValidationTarget")
                .Requires("kind", Literal("container"))
                .Requires("container", "string"));

            contract.Declare(Union("RequestChain", "TerminalRequestChain", "FollowUpRequestChain"));

            contract.Declare(Interface("TerminalRequestChain")
                .Requires("kind", Literal("terminal")));

            contract.Declare(Interface("FollowUpRequestChain")
                .Requires("kind", Literal("follow-up"))
                .Requires("next", "RequestPlan"));

            contract.Declare(Union("RequestInput", "NoRequestInput", "GatheredRequestInput"));
            contract.Declare(LiteralUnion("RequestBodyFormat", RequestBodyFormat.Values));

            contract.Declare(Interface("NoRequestInput")
                .Requires("kind", Literal("none")));

            contract.Declare(Interface("GatheredRequestInput")
                .Requires("kind", Literal("gather"))
                .Requires("assignments", "RequestInputAssignment[]")
                .Requires("bodyFormat", "RequestBodyFormat")
                .Requires("registeredInputs", "RegisteredInputSelection"));

            contract.Declare(Union(
                "RegisteredInputSelection",
                "ExplicitRegisteredInputSelection",
                "AllRegisteredInputsSelection"));

            contract.Declare(Interface("ExplicitRegisteredInputSelection")
                .Requires("kind", Literal("explicit")));

            contract.Declare(Interface("AllRegisteredInputsSelection")
                .Requires("kind", Literal("all-registered-inputs")));

            contract.Declare(Interface("RequestInputAssignment")
                .Requires("target", "RequestInputTarget")
                .Requires("source", "ValueExpression"));

            contract.Declare(Union(
                "RequestInputTarget",
                "RequestPayloadTarget",
                "RequestHeaderTarget",
                "RequestRouteParameterTarget"));

            contract.Declare(Interface("RequestPayloadTarget")
                .Requires("kind", Literal("payload"))
                .Requires("name", "string")
                .Requires("path", "StructuredPath"));

            contract.Declare(Interface("RequestHeaderTarget")
                .Requires("kind", Literal("header"))
                .Requires("name", "string"));

            contract.Declare(Interface("RequestRouteParameterTarget")
                .Requires("kind", Literal("route-param"))
                .Requires("name", "string"));

            contract.Declare(Interface("ResponseRoute")
                .Requires("match", "ResponseStatusMatch")
                .Requires("reaction", "ReactionGraph"));

            contract.Declare(Union(
                "ResponseStatusMatch",
                "AnyResponseStatusMatch",
                "ExactResponseStatusMatch"));

            contract.Declare(Interface("AnyResponseStatusMatch")
                .Requires("kind", Literal("any")));

            contract.Declare(Interface("ExactResponseStatusMatch")
                .Requires("kind", Literal("status"))
                .Requires("status", "number"));

            contract.Declare(Union(
                "JsonValue",
                "string",
                "number",
                "boolean",
                "null",
                "JsonValue[]",
                "{ [key: string]: JsonValue }"));

            contract.Declare(Union(
                "ValueExpression",
                "LiteralExpression",
                "ReadExpression",
                "ObjectExpression",
                "ArrayExpression"));

            contract.Declare(Union(
                "ReadExpression",
                "ObjectPropertyReadExpression",
                "ObjectMethodReadExpression",
                "UrlParameterReadExpression",
                "PayloadPathReadExpression",
                "WholePayloadReadExpression"));

            contract.Declare(Interface("LiteralExpression")
                .Requires("kind", Literal("literal"))
                .Requires("value", "JsonValue")
                .Requires("shape", "Shape"));

            contract.Declare(Interface("NumericLiteralExpression")
                .Requires("kind", Literal("literal"))
                .Requires("value", "number")
                .Requires("shape", "Shape"));

            contract.Declare(Interface("TextLiteralExpression")
                .Requires("kind", Literal("literal"))
                .Requires("value", "string")
                .Requires("shape", "Shape"));

            contract.Declare(Interface("RangeLiteralExpression")
                .Requires("kind", Literal("literal"))
                .Requires("value", "[JsonValue, JsonValue]")
                .Requires("shape", "Shape"));

            contract.Declare(Interface("ObjectPropertyReadExpression")
                .Requires("kind", Literal("read"))
                .Requires("from", "RuntimeObjectSource")
                .Requires("member", "string")
                .Requires("path", "EmptyPath")
                .Requires("shape", "Shape")
                .Requires("access", "PropertyValueReadAccess"));

            contract.Declare(Interface("ObjectMethodReadExpression")
                .Requires("kind", Literal("read"))
                .Requires("from", "RuntimeObjectSource")
                .Requires("member", "string")
                .Requires("path", "EmptyPath")
                .Requires("shape", "Shape")
                .Requires("access", "MethodValueReadAccess"));

            contract.Declare(Interface("UrlParameterReadExpression")
                .Requires("kind", Literal("read"))
                .Requires("from", "UrlSource")
                .Requires("member", "string")
                .Requires("path", "EmptyPath")
                .Requires("shape", "Shape")
                .Requires("access", "PropertyValueReadAccess"));

            contract.Declare(Interface("PayloadPathReadExpression")
                .Requires("kind", Literal("read"))
                .Requires("from", "PayloadSource")
                .Requires("member", "string")
                .Requires("path", "StructuredPath")
                .Requires("shape", "Shape")
                .Requires("access", "PropertyValueReadAccess"));

            contract.Declare(Interface("WholePayloadReadExpression")
                .Requires("kind", Literal("read"))
                .Requires("from", "PayloadSource")
                .Requires("member", Literal("responseBody"))
                .Requires("path", "EmptyPath")
                .Requires("shape", "Shape")
                .Requires("access", "PropertyValueReadAccess"));

            contract.Declare(Union(
                "ValueReadAccess",
                "PropertyValueReadAccess",
                "MethodValueReadAccess"));

            contract.Declare(Interface("PropertyValueReadAccess")
                .Requires("kind", Literal("property")));

            contract.Declare(Interface("MethodValueReadAccess")
                .Requires("kind", Literal("method"))
                .Requires("args", "ValueExpression[]"));

            contract.Declare(Interface("ObjectExpression")
                .Requires("kind", Literal("object"))
                .Requires("fields", "Record<string, ValueExpression>")
                .Requires("shape", "Shape"));

            contract.Declare(Interface("ArrayExpression")
                .Requires("kind", Literal("array"))
                .Requires("items", "ValueExpression[]")
                .Requires("shape", "Shape"));

            contract.Declare(Union(
                "ValidationCondition",
                "CompareCondition",
                "ValidationAllCondition",
                "ValidationAnyCondition",
                "ValidationNotCondition"));

            contract.Declare(Interface("ValidationAllCondition")
                .Requires("kind", Literal("all"))
                .Requires("terms", "ValidationCondition[]"));

            contract.Declare(Interface("ValidationAnyCondition")
                .Requires("kind", Literal("any"))
                .Requires("terms", "ValidationCondition[]"));

            contract.Declare(Interface("ValidationNotCondition")
                .Requires("kind", Literal("not"))
                .Requires("term", "ValidationCondition"));

            contract.Declare(Union(
                "ConditionGraph",
                "CompareCondition",
                "AllCondition",
                "AnyCondition",
                "NotCondition",
                "ConfirmCondition"));

            contract.Declare(LiteralUnion("CompareOp", CompareOperator.Values));

            contract.Declare(Union(
                "CompareCondition",
                "UnaryCompareCondition",
                "EqualityCompareCondition",
                "OrderedCompareCondition",
                "MembershipCompareCondition",
                "RangeCompareCondition",
                "TextCompareCondition",
                "RegexCompareCondition",
                "TextLengthCompareCondition",
                "CollectionItemCompareCondition"));

            contract.Declare(LiteralUnion("UnaryCompareOp", CompareOperator.UnaryValues));
            contract.Declare(LiteralUnion("EqualityCompareOp", CompareOperator.EqualityValues));
            contract.Declare(LiteralUnion("OrderedCompareOp", CompareOperator.OrderedValues));
            contract.Declare(LiteralUnion("MembershipCompareOp", CompareOperator.MembershipValues));
            contract.Declare(LiteralUnion("RangeCompareOp", CompareOperator.RangeValues));
            contract.Declare(LiteralUnion("TextCompareOp", CompareOperator.TextValues));
            contract.Declare(LiteralUnion("RegexCompareOp", CompareOperator.RegexValues));
            contract.Declare(LiteralUnion("TextLengthCompareOp", CompareOperator.TextLengthValues));
            contract.Declare(LiteralUnion("CollectionItemCompareOp", CompareOperator.CollectionItemValues));

            contract.Declare(Interface("UnaryCompareCondition")
                .Requires("kind", Literal("compare"))
                .Requires("left", "ValueExpression")
                .Requires("op", "UnaryCompareOp")
                .Requires("right", "NoComparisonRightOperand")
                .Requires("shape", "Shape")
                .Requires("itemShape", "Shape"));

            contract.Declare(Interface("EqualityCompareCondition")
                .Requires("kind", Literal("compare"))
                .Requires("left", "ValueExpression")
                .Requires("op", "EqualityCompareOp")
                .Requires("right", "PresentComparisonRightOperand")
                .Requires("shape", "Shape")
                .Requires("itemShape", "Shape"));

            contract.Declare(Interface("OrderedCompareCondition")
                .Requires("kind", Literal("compare"))
                .Requires("left", "ValueExpression")
                .Requires("op", "OrderedCompareOp")
                .Requires("right", "PresentComparisonRightOperand")
                .Requires("shape", "Shape")
                .Requires("itemShape", "Shape"));

            contract.Declare(Interface("MembershipCompareCondition")
                .Requires("kind", Literal("compare"))
                .Requires("left", "ValueExpression")
                .Requires("op", "MembershipCompareOp")
                .Requires("right", "CollectionComparisonRightOperand")
                .Requires("shape", "Shape")
                .Requires("itemShape", "Shape"));

            contract.Declare(Interface("RangeCompareCondition")
                .Requires("kind", Literal("compare"))
                .Requires("left", "ValueExpression")
                .Requires("op", "RangeCompareOp")
                .Requires("right", "RangeComparisonRightOperand")
                .Requires("shape", "Shape")
                .Requires("itemShape", "Shape"));

            contract.Declare(Interface("TextCompareCondition")
                .Requires("kind", Literal("compare"))
                .Requires("left", "ValueExpression")
                .Requires("op", "TextCompareOp")
                .Requires("right", "TextComparisonRightOperand")
                .Requires("shape", "Shape")
                .Requires("itemShape", "Shape"));

            contract.Declare(Interface("RegexCompareCondition")
                .Requires("kind", Literal("compare"))
                .Requires("left", "ValueExpression")
                .Requires("op", "RegexCompareOp")
                .Requires("right", "TextComparisonRightOperand")
                .Requires("shape", "Shape")
                .Requires("itemShape", "Shape"));

            contract.Declare(Interface("TextLengthCompareCondition")
                .Requires("kind", Literal("compare"))
                .Requires("left", "ValueExpression")
                .Requires("op", "TextLengthCompareOp")
                .Requires("right", "NumericComparisonRightOperand")
                .Requires("shape", "Shape")
                .Requires("itemShape", "Shape"));

            contract.Declare(Interface("CollectionItemCompareCondition")
                .Requires("kind", Literal("compare"))
                .Requires("left", "ValueExpression")
                .Requires("op", "CollectionItemCompareOp")
                .Requires("right", "LiteralComparisonRightOperand")
                .Requires("shape", "Shape")
                .Requires("itemShape", "Shape"));

            contract.Declare(Union(
                "ComparisonRightOperand",
                "NoComparisonRightOperand",
                "PresentComparisonRightOperand",
                "CollectionComparisonRightOperand",
                "RangeComparisonRightOperand",
                "TextComparisonRightOperand",
                "NumericComparisonRightOperand",
                "LiteralComparisonRightOperand"));

            contract.Declare(Interface("NoComparisonRightOperand")
                .Requires("kind", Literal("none")));

            contract.Declare(Interface("PresentComparisonRightOperand")
                .Requires("kind", Literal("value"))
                .Requires("value", "ValueExpression"));

            contract.Declare(Interface("CollectionComparisonRightOperand")
                .Requires("kind", Literal("value"))
                .Requires("value", "ArrayExpression"));

            contract.Declare(Alias(
                "RangeComparisonExpression",
                "ArrayExpression & { items: [ValueExpression, ValueExpression] }"));

            contract.Declare(Interface("RangeComparisonRightOperand")
                .Requires("kind", Literal("value"))
                .Requires("value", "RangeComparisonExpression"));

            contract.Declare(Interface("TextComparisonRightOperand")
                .Requires("kind", Literal("value"))
                .Requires("value", "TextLiteralExpression"));

            contract.Declare(Interface("NumericComparisonRightOperand")
                .Requires("kind", Literal("value"))
                .Requires("value", "NumericLiteralExpression"));

            contract.Declare(Interface("LiteralComparisonRightOperand")
                .Requires("kind", Literal("value"))
                .Requires("value", "LiteralExpression"));

            contract.Declare(Interface("AllCondition")
                .Requires("kind", Literal("all"))
                .Requires("terms", "ConditionGraph[]"));

            contract.Declare(Interface("AnyCondition")
                .Requires("kind", Literal("any"))
                .Requires("terms", "ConditionGraph[]"));

            contract.Declare(Interface("NotCondition")
                .Requires("kind", Literal("not"))
                .Requires("term", "ConditionGraph"));

            contract.Declare(Interface("ConfirmCondition")
                .Requires("kind", Literal("confirm"))
                .Requires("message", "string"));

            contract.Declare(Union(
                "Shape",
                "StringShape",
                "NumberShape",
                "BooleanShape",
                "DateShape",
                "RawShape",
                "ArrayShape",
                "ObjectShape",
                "NullableShape",
                "AnyShape",
                "NoneShape"));

            contract.Declare(Interface("StringShape").Requires("kind", Literal("string")));
            contract.Declare(Interface("NumberShape").Requires("kind", Literal("number")));
            contract.Declare(Interface("BooleanShape").Requires("kind", Literal("boolean")));
            contract.Declare(Interface("DateShape").Requires("kind", Literal("date")));
            contract.Declare(Interface("RawShape").Requires("kind", Literal("raw")));

            contract.Declare(Interface("ArrayShape")
                .Requires("kind", Literal("array"))
                .Requires("item", "Shape"));

            contract.Declare(Interface("ObjectShape")
                .Requires("kind", Literal("object"))
                .Requires("fields", "Record<string, Shape>")
                .Requires("additional", "boolean"));

            contract.Declare(Interface("NullableShape")
                .Requires("kind", Literal("nullable"))
                .Requires("inner", "Shape"));

            contract.Declare(Interface("AnyShape").Requires("kind", Literal("any")));
            contract.Declare(Interface("NoneShape").Requires("kind", Literal("none")));

            contract.Declare(Alias("Path", "PathSegment[]"));
            contract.Declare(Alias("EmptyPath", "[]"));
            contract.Declare(Alias("StructuredPath", "[PathSegment, ...PathSegment[]]"));
            contract.Declare(Union("PathSegment", "PropertySegment", "IndexSegment"));

            contract.Declare(Interface("PropertySegment")
                .Requires("kind", Literal("property"))
                .Requires("name", "string"));

            contract.Declare(Interface("IndexSegment")
                .Requires("kind", Literal("index"))
                .Requires("index", "number"));

            return contract;
        }

        private static TypeScriptInterface Interface(string name) =>
            new TypeScriptInterface(name);

        private static TypeScriptInterface ComponentVariant(
            string name,
            string role,
            string binding,
            string container) =>
            Interface(name)
                .Requires("id", "string")
                .Requires("vendor", "Vendor")
                .Requires("type", "string")
                .Requires("role", role)
                .Requires("binding", binding)
                .Requires("container", container);

        private static TypeScriptTypeAlias Alias(string name, string type) =>
            new TypeScriptTypeAlias(name, TypeScriptType.Single(type));

        private static TypeScriptTypeAlias Union(string name, params string[] members) =>
            new TypeScriptTypeAlias(name, TypeScriptType.Union(members));

        private static TypeScriptTypeAlias LiteralUnion(string name, IEnumerable<string> values) =>
            Union(name, values.Select(Literal).ToArray());

        private static string Literal(string value) => @"""" + value + @"""";
    }

    internal sealed class TypeScriptContract
    {
        private readonly string _generator;
        private readonly string _command;
        private readonly List<TypeScriptDeclaration> _declarations;

        private TypeScriptContract(string generator, string command)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _command = command ?? throw new ArgumentNullException(nameof(command));
            _declarations = new List<TypeScriptDeclaration>();
        }

        internal static TypeScriptContract GeneratedBy(string generator, string command) =>
            new TypeScriptContract(generator, command);

        internal void Declare(TypeScriptDeclaration declaration)
        {
            if (declaration == null) throw new ArgumentNullException(nameof(declaration));
            _declarations.Add(declaration);
        }

        internal string Render()
        {
            var writer = new TypeScriptWriter();
            writer.Line("// <auto-generated />");
            writer.Line("// Generated by " + _generator + ".");
            writer.Line("// Do not edit by hand. Run `" + _command + "`.");
            writer.BlankLine();

            foreach (var declaration in _declarations)
            {
                declaration.Render(writer);
                writer.BlankLine();
            }

            return writer.ToString();
        }
    }

    internal abstract class TypeScriptDeclaration
    {
        internal abstract void Render(TypeScriptWriter writer);
    }

    internal sealed class TypeScriptInterface : TypeScriptDeclaration
    {
        private readonly string _name;
        private readonly List<TypeScriptProperty> _properties;

        internal TypeScriptInterface(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Interface name is required.", nameof(name));

            _name = name;
            _properties = new List<TypeScriptProperty>();
        }

        internal TypeScriptInterface Requires(string name, string type)
        {
            _properties.Add(TypeScriptProperty.Required(name, type));
            return this;
        }

        internal override void Render(TypeScriptWriter writer)
        {
            writer.Line("export interface " + _name + " {");
            writer.Indent();
            foreach (var property in _properties)
                property.Render(writer);
            writer.Outdent();
            writer.Line("}");
        }
    }

    internal sealed class TypeScriptProperty
    {
        private readonly string _name;
        private readonly string _type;

        private TypeScriptProperty(string name, string type)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Property name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Property type is required.", nameof(type));

            _name = name;
            _type = type;
        }

        internal static TypeScriptProperty Required(string name, string type) =>
            new TypeScriptProperty(name, type);

        internal void Render(TypeScriptWriter writer)
        {
            writer.Line(_name + ": " + _type + ";");
        }
    }

    internal sealed class TypeScriptTypeAlias : TypeScriptDeclaration
    {
        private readonly string _name;
        private readonly TypeScriptType _type;

        internal TypeScriptTypeAlias(string name, TypeScriptType type)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Type alias name is required.", nameof(name));

            _name = name;
            _type = type ?? throw new ArgumentNullException(nameof(type));
        }

        internal override void Render(TypeScriptWriter writer)
        {
            _type.RenderAlias(writer, _name);
        }
    }

    internal abstract class TypeScriptType
    {
        internal static TypeScriptType Single(string type) =>
            new SingleLineType(type);

        internal static TypeScriptType Union(IReadOnlyList<string> members) =>
            new UnionType(members);

        internal abstract void RenderAlias(TypeScriptWriter writer, string aliasName);

        private sealed class SingleLineType : TypeScriptType
        {
            private readonly string _type;

            internal SingleLineType(string type)
            {
                if (string.IsNullOrWhiteSpace(type))
                    throw new ArgumentException("Type is required.", nameof(type));

                _type = type;
            }

            internal override void RenderAlias(TypeScriptWriter writer, string aliasName)
            {
                writer.Line("export type " + aliasName + " = " + _type + ";");
            }
        }

        private sealed class UnionType : TypeScriptType
        {
            private readonly IReadOnlyList<string> _members;

            internal UnionType(IReadOnlyList<string> members)
            {
                if (members == null) throw new ArgumentNullException(nameof(members));
                if (members.Count == 0)
                    throw new ArgumentException("Union must contain at least one member.", nameof(members));

                _members = members;
            }

            internal override void RenderAlias(TypeScriptWriter writer, string aliasName)
            {
                writer.Line("export type " + aliasName + " =");
                writer.Indent();
                for (var i = 0; i < _members.Count; i++)
                {
                    var suffix = i == _members.Count - 1 ? ";" : string.Empty;
                    writer.Line("| " + _members[i] + suffix);
                }
                writer.Outdent();
            }
        }
    }

    internal sealed class TypeScriptWriter
    {
        private readonly StringBuilder _content = new StringBuilder();
        private int _indent;

        internal void Indent()
        {
            _indent++;
        }

        internal void Outdent()
        {
            if (_indent == 0)
                throw new InvalidOperationException("Cannot outdent past the document root.");

            _indent--;
        }

        internal void Line(string text)
        {
            for (var i = 0; i < _indent; i++)
                _content.Append("  ");

            _content.AppendLine(text);
        }

        internal void BlankLine()
        {
            _content.AppendLine();
        }

        public override string ToString()
        {
            return _content
                .ToString()
                .Replace("\r\n", "\n")
                .TrimEnd('\n') + "\n";
        }
    }
}
