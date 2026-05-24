using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Authoritative TypeScript projection for the plan JSON contract executed by the browser runtime.
    /// Kept next to the plan domain so runtime types are generated from the plan model, not from the stale schema.
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

            contract.Declare(Interface("Plan")
                .Requires("version", "3")
                .Requires("planId", "string")
                .Requires("scope", "PlanScope")
                .Requires("types", "Record<string, JsType>")
                .Requires("components", "Record<string, Component>")
                .Requires("behaviors", "Behavior[]"));

            contract.Declare(Union("PlanScope", "RootPlanScope", "PartialPlanScope"));

            contract.Declare(Interface("RootPlanScope")
                .Requires("kind", Literal("root")));

            contract.Declare(Interface("PartialPlanScope")
                .Requires("kind", Literal("partial"))
                .Requires("partId", "string"));

            contract.Declare(Interface("JsType")
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

            contract.Declare(Interface("Component")
                .Requires("id", "string")
                .Requires("vendor", "Vendor")
                .Requires("type", "string")
                .Requires("contribution", "ComponentContributionIntent")
                .Requires("binding", "ComponentBinding")
                .Requires("container", "ComponentContainer"));

            contract.Declare(Union(
                "ComponentContributionIntent",
                "ObjectTargetComponentContribution",
                "OwnedDefinitionComponentContribution",
                "ValidationContainerComponentContribution",
                "LayoutObjectComponentContribution"));
            contract.Declare(Interface("ObjectTargetComponentContribution")
                .Requires("kind", Literal("object-target")));
            contract.Declare(Interface("OwnedDefinitionComponentContribution")
                .Requires("kind", Literal("owned-definition")));
            contract.Declare(Interface("ValidationContainerComponentContribution")
                .Requires("kind", Literal("validation-container")));
            contract.Declare(Interface("LayoutObjectComponentContribution")
                .Requires("kind", Literal("layout-object")));

            contract.Declare(Union("ComponentBinding", "UnboundComponentBinding", "RegisteredInputBinding"));
            contract.Declare(Interface("UnboundComponentBinding")
                .Requires("kind", Literal("none")));
            contract.Declare(Interface("RegisteredInputBinding")
                .Requires("kind", Literal("registered-input"))
                .Requires("bindingPath", "string")
                .Requires("valueMember", "string"));

            contract.Declare(Union(
                "ComponentContainer",
                "UnscopedComponentContainer",
                "ValidationContainerComponent"));
            contract.Declare(Interface("UnscopedComponentContainer")
                .Requires("kind", Literal("none")));
            contract.Declare(Interface("ValidationContainerComponent")
                .Requires("kind", Literal("validation-container"))
                .Requires("components", "string[]")
                .Requires("validationRules", "ComponentValidation[]"));

            contract.Declare(Interface("ComponentValidation")
                .Requires("component", "string")
                .Requires("value", "ValueProducer")
                .Requires("serverFieldName", "string")
                .Requires("rules", "ValidationRule[]"));

            contract.Declare(LiteralUnion("ValidationRuleName", ValidationRuleName.Values));

            contract.Declare(Interface("ValidationRule")
                .Requires("name", "ValidationRuleName")
                .Requires("message", "string")
                .Requires("execution", "ValidationRuleExecution"));

            contract.Declare(Interface("ValidationRuleExecution")
                .Requires("constraint", "ValidationRuleOperand")
                .Requires("otherValue", "ValidationRuleOperand")
                .Requires("activation", "ValidationRuleActivation")
                .Requires("comparisonShape", "Shape"));

            contract.Declare(Union(
                "ValidationRuleOperand",
                "NoValidationRuleOperand",
                "ValueValidationRuleOperand"));

            contract.Declare(Interface("NoValidationRuleOperand")
                .Requires("kind", Literal("none")));

            contract.Declare(Interface("ValueValidationRuleOperand")
                .Requires("kind", Literal("value"))
                .Requires("value", "ValueProducer"));

            contract.Declare(Union(
                "ValidationRuleActivation",
                "AlwaysValidationRuleActivation",
                "ConditionalValidationRuleActivation"));

            contract.Declare(Interface("AlwaysValidationRuleActivation")
                .Requires("kind", Literal("always")));

            contract.Declare(Interface("ConditionalValidationRuleActivation")
                .Requires("kind", Literal("when"))
                .Requires("condition", "Condition"));

            contract.Declare(Union("Source", "ComponentSource", "PayloadSource", "UrlSource", "PluginSource"));

            contract.Declare(Interface("ComponentSource")
                .Requires("kind", Literal("component"))
                .Requires("component", "string"));

            contract.Declare(Interface("UrlSource")
                .Requires("kind", Literal("url")));

            contract.Declare(Interface("PluginSource")
                .Requires("kind", Literal("plugin"))
                .Requires("name", "string"));

            contract.Declare(LiteralUnion("PayloadScope", PayloadScope.Values));

            contract.Declare(Interface("PayloadSource")
                .Requires("kind", Literal("payload"))
                .Requires("scope", "PayloadScope")
                .Requires("type", "PayloadContract"));

            contract.Declare(Interface("Behavior")
                .Requires("startsWhen", "StartsWhen")
                .Requires("reaction", "Reaction"));

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
                "Reaction",
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
                .Requires("steps", "Reaction[]"));

            contract.Declare(Interface("ParallelReaction")
                .Requires("kind", Literal("parallel"))
                .Requires("steps", "Reaction[]")
                .Requires("completion", "ParallelCompletion"));

            contract.Declare(Union(
                "ParallelCompletion",
                "NoParallelCompletion",
                "SettledParallelCompletion"));

            contract.Declare(Interface("NoParallelCompletion")
                .Requires("kind", Literal("none")));

            contract.Declare(Interface("SettledParallelCompletion")
                .Requires("kind", Literal("on-settled"))
                .Requires("reaction", "Reaction"));

            contract.Declare(Interface("BranchReaction")
                .Requires("kind", Literal("branch"))
                .Requires("cases", "BranchCase[]"));

            contract.Declare(Interface("BranchCase")
                .Requires("guard", "BranchGuard")
                .Requires("reaction", "Reaction"));

            contract.Declare(Union("BranchGuard", "DefaultBranchGuard", "ConditionalBranchGuard"));
            contract.Declare(Interface("DefaultBranchGuard")
                .Requires("kind", Literal("default")));
            contract.Declare(Interface("ConditionalBranchGuard")
                .Requires("kind", Literal("when"))
                .Requires("condition", "Condition"));

            contract.Declare(Interface("SetReaction")
                .Requires("kind", Literal("set"))
                .Requires("on", "Source")
                .Requires("property", "string")
                .Requires("value", "ValueProducer"));

            contract.Declare(Interface("CallReaction")
                .Requires("kind", Literal("call"))
                .Requires("on", "Source")
                .Requires("method", "string")
                .Requires("args", "ValueProducer[]"));

            contract.Declare(Interface("RequestReaction")
                .Requires("kind", Literal("request"))
                .Requires("request", "Request"));

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
                .Requires("data", "ValueProducer")
                .Requires("payloadType", "PayloadContract"));

            contract.Declare(Interface("InjectReaction")
                .Requires("kind", Literal("inject"))
                .Requires("target", "InjectionTarget")
                .Requires("value", "ValueProducer"));

            contract.Declare(Union(
                "InjectionTarget",
                "PartialSlotInjectionTarget"));

            contract.Declare(Interface("PartialSlotInjectionTarget")
                .Requires("kind", Literal("partial-slot"))
                .Requires("component", "string")
                .Requires("slotId", "string"));

            contract.Declare(Interface("ShowValidationErrorsReaction")
                .Requires("kind", Literal("show-validation-errors"))
                .Requires("container", "string"));

            contract.Declare(LiteralUnion("HttpMethod", HttpMethodName.Values));

            contract.Declare(Interface("Request")
                .Requires("method", "HttpMethod")
                .Requires("url", "string")
                .Requires("headers", "Record<string, ValueProducer>")
                .Requires("routeParams", "Record<string, ValueProducer>")
                .Requires("validation", "RequestValidationTarget")
                .Requires("input", "RequestInput")
                .Requires("before", "Reaction[]")
                .Requires("success", "ResponseHandler[]")
                .Requires("error", "ResponseHandler[]")
                .Requires("complete", "Reaction[]")
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
                .Requires("next", "Request"));

            contract.Declare(Union("RequestInput", "NoRequestInput", "GatherInput", "ValueInput"));
            contract.Declare(LiteralUnion("Transport", RequestTransport.Values));

            contract.Declare(Interface("NoRequestInput")
                .Requires("kind", Literal("none")));

            contract.Declare(Interface("GatherInput")
                .Requires("kind", Literal("gather"))
                .Requires("components", "GatherField[]")
                .Requires("transport", "Transport")
                .Requires("statics", "GatherStatics")
                .Requires("selection", "GatherSelection"));

            contract.Declare(Union(
                "GatherStatics",
                "NoGatherStatics",
                "StaticGatherValue"));

            contract.Declare(Interface("NoGatherStatics")
                .Requires("kind", Literal("none")));

            contract.Declare(Interface("StaticGatherValue")
                .Requires("kind", Literal("value"))
                .Requires("value", "ObjectProducer"));

            contract.Declare(Union(
                "GatherSelection",
                "ExplicitGatherSelection",
                "AllRegisteredInputsGatherSelection"));

            contract.Declare(Interface("ExplicitGatherSelection")
                .Requires("kind", Literal("explicit")));

            contract.Declare(Interface("AllRegisteredInputsGatherSelection")
                .Requires("kind", Literal("all-registered-inputs")));

            contract.Declare(Interface("ValueInput")
                .Requires("kind", Literal("value"))
                .Requires("value", "ObjectProducer")
                .Requires("transport", "Transport"));

            contract.Declare(Interface("GatherField")
                .Requires("key", "string")
                .Requires("value", "ValueProducer"));

            contract.Declare(Interface("ResponseHandler")
                .Requires("match", "ResponseStatusMatch")
                .Requires("reaction", "Reaction"));

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
                "ValueProducer",
                "LiteralProducer",
                "ReadProducer",
                "ObjectProducer",
                "ArrayProducer"));

            contract.Declare(Interface("LiteralProducer")
                .Requires("kind", Literal("literal"))
                .Requires("value", "JsonValue")
                .Requires("shape", "Shape"));

            contract.Declare(Interface("ReadProducer")
                .Requires("kind", Literal("read"))
                .Requires("from", "Source")
                .Requires("member", "string")
                .Requires("path", "Path")
                .Requires("shape", "Shape")
                .Requires("access", "ValueReadAccess"));

            contract.Declare(Union(
                "ValueReadAccess",
                "PropertyValueReadAccess",
                "MethodValueReadAccess"));

            contract.Declare(Interface("PropertyValueReadAccess")
                .Requires("kind", Literal("property")));

            contract.Declare(Interface("MethodValueReadAccess")
                .Requires("kind", Literal("method"))
                .Requires("args", "ValueProducer[]"));

            contract.Declare(Interface("ObjectProducer")
                .Requires("kind", Literal("object"))
                .Requires("fields", "Record<string, ValueProducer>")
                .Requires("shape", "Shape"));

            contract.Declare(Interface("ArrayProducer")
                .Requires("kind", Literal("array"))
                .Requires("items", "ValueProducer[]")
                .Requires("shape", "Shape"));

            contract.Declare(Union(
                "Condition",
                "CompareCondition",
                "AllCondition",
                "AnyCondition",
                "NotCondition",
                "ConfirmCondition"));

            contract.Declare(LiteralUnion("CompareOp", CompareOperator.Values));

            contract.Declare(Interface("CompareCondition")
                .Requires("kind", Literal("compare"))
                .Requires("left", "ValueProducer")
                .Requires("op", "CompareOp")
                .Requires("right", "ComparisonRightOperand")
                .Requires("shape", "Shape")
                .Requires("itemShape", "Shape"));

            contract.Declare(Union(
                "ComparisonRightOperand",
                "NoComparisonRightOperand",
                "PresentComparisonRightOperand"));

            contract.Declare(Interface("NoComparisonRightOperand")
                .Requires("kind", Literal("none")));

            contract.Declare(Interface("PresentComparisonRightOperand")
                .Requires("kind", Literal("value"))
                .Requires("value", "ValueProducer"));

            contract.Declare(Interface("AllCondition")
                .Requires("kind", Literal("all"))
                .Requires("terms", "Condition[]"));

            contract.Declare(Interface("AnyCondition")
                .Requires("kind", Literal("any"))
                .Requires("terms", "Condition[]"));

            contract.Declare(Interface("NotCondition")
                .Requires("kind", Literal("not"))
                .Requires("term", "Condition"));

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
