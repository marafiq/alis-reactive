using System;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    internal abstract class ClientConditionProjection
    {
        private protected ClientConditionProjection() { }

        internal static ClientConditionProjection Project(FieldCondition condition) =>
            new ProjectedClientCondition(condition);

        internal static ClientConditionProjection Skip(ClientRuleExtractionSkipReason reason) =>
            new SkippedClientCondition(reason);

        internal abstract TResult Match<TResult>(
            Func<FieldCondition, TResult> projected,
            Func<ClientRuleExtractionSkipReason, TResult> skipped);

        private sealed class ProjectedClientCondition : ClientConditionProjection
        {
            private readonly FieldCondition _condition;

            internal ProjectedClientCondition(FieldCondition condition)
            {
                _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            }

            internal override TResult Match<TResult>(
                Func<FieldCondition, TResult> projected,
                Func<ClientRuleExtractionSkipReason, TResult> skipped)
            {
                if (projected == null) throw new ArgumentNullException(nameof(projected));
                if (skipped == null) throw new ArgumentNullException(nameof(skipped));
                return projected(_condition);
            }
        }

        private sealed class SkippedClientCondition : ClientConditionProjection
        {
            private readonly ClientRuleExtractionSkipReason _reason;

            internal SkippedClientCondition(ClientRuleExtractionSkipReason reason)
            {
                _reason = reason;
            }

            internal override TResult Match<TResult>(
                Func<FieldCondition, TResult> projected,
                Func<ClientRuleExtractionSkipReason, TResult> skipped)
            {
                if (projected == null) throw new ArgumentNullException(nameof(projected));
                if (skipped == null) throw new ArgumentNullException(nameof(skipped));
                return skipped(_reason);
            }
        }
    }
}
