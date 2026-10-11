
namespace Alis.Reactive.PlanModel
{
    internal abstract class ValidationRuleActivation
    {
        private protected ValidationRuleActivation() { }

        internal static ValidationRuleActivation Always { get; } =
            new AlwaysActiveValidationRule();


        /// <summary>The condition the rule waits for; an always-active rule has none.</summary>
        internal abstract bool TryGetCondition(out ConditionGraph condition);

        internal static ValidationRuleActivation When(ConditionGraph condition) =>
            new ConditionallyActiveValidationRule(condition);

        private sealed class AlwaysActiveValidationRule : ValidationRuleActivation
        {


            internal override bool TryGetCondition(out ConditionGraph condition)
            {
                condition = null!;
                return false;
            }
        }

        private sealed class ConditionallyActiveValidationRule : ValidationRuleActivation
        {
            private readonly ConditionGraph _condition;

            internal ConditionallyActiveValidationRule(ConditionGraph condition)
            {
                _condition = condition ?? throw new System.ArgumentNullException(nameof(condition));
            }


            internal override bool TryGetCondition(out ConditionGraph condition)
            {
                condition = _condition;
                return true;
            }
        }
    }

}
