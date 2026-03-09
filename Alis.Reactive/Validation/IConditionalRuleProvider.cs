using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Implemented by validators that declare explicit conditional rules
    /// (rules with a When condition that cannot be introspected from FluentValidation).
    /// </summary>
    public interface IConditionalRuleProvider
    {
        IReadOnlyList<ConditionalRuleMetadata> GetConditionalRules();
    }
}
