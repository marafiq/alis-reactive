using System;
using System.Threading;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.Validation;

internal static class RuleSetChecks
{
    internal static void RunsARuleUnder<T>(ReactiveValidator<T> validator, Type validatorType, string ruleSet)
    {
        if (string.Equals(ruleSet, BrowserRegistrations.DefaultRuleSet, StringComparison.OrdinalIgnoreCase)) return;
        if (validator.RunsARule(new ValidationRun(true, CancellationToken.None, ruleSet, [], []))) return;
        throw new InvalidOperationException(
            $"{ClassNames.AsCSharpWritesIt(validatorType)} runs no rule under the rule set '{ruleSet}': declare the rules it checks there inside RuleSet(\"{ruleSet}\", ..), a rule that holds a child validator among them.");
    }
}
