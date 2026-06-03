using System;

namespace Alis.Reactive.PlanModel
{
    internal sealed class RegisteredInputValueRead
    {
        private readonly ComponentId _componentId;
        private readonly string _componentName;
        private readonly string _registrationExample;
        private readonly string _usage;

        private RegisteredInputValueRead(
            ComponentId componentId,
            MemberName valueMember,
            string componentName,
            string registrationExample,
            string usage)
        {
            _componentId = componentId;
            ValueMember = valueMember;
            _componentName = componentName;
            _registrationExample = registrationExample;
            _usage = usage;
        }

        internal MemberName ValueMember { get; }

        internal static RegisteredInputValueRead ForFusionInPlaceEditorValueRead(string componentId) =>
            new RegisteredInputValueRead(
                ComponentId.Of(componentId),
                MemberName.Of("value"),
                "FusionInPlaceEditor",
                "Render the editor with Html.InputField(plan, m => m.X).FusionInPlaceEditor(...)",
                "reading .Value() in a pipeline");

        internal static RegisteredInputValueRead ForGatherValueRead(string componentId, string valueMember) =>
            new RegisteredInputValueRead(
                ComponentId.Of(componentId),
                MemberName.Of(valueMember),
                "input component",
                "Render it with a registered input helper or use a typed component source",
                "gathering '" + valueMember + "'");

        internal InvalidOperationException MissingRegistrationException() =>
            new InvalidOperationException(
                $"{_componentName} '{_componentId.Value}' is not registered. " +
                $"{_registrationExample} before {_usage}; " +
                "the registered value contract drives the typed read.");
    }
}
