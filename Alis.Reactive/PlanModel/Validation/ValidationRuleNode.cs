using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    internal sealed class ValidationRuleNode
    {
        private readonly RuleName _name;
        private readonly ValidationMessage _message;
        private readonly ValidationRuleExecution _execution;

        public string Name => _name.Value;
        public string Message => _message.Value;
        internal ValidationRuleExecution Execution => _execution;

        internal ValidationRuleNode(
            RuleName name,
            ValidationMessage message,
            ValidationRuleExecution execution)
        {
            _name = name ?? throw new System.ArgumentNullException(nameof(name));
            _message = message ?? throw new System.ArgumentNullException(nameof(message));
            _execution = execution ?? throw new System.ArgumentNullException(nameof(execution));
        }
    }

}
