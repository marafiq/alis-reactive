using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// The conditions a rule was declared under, outermost first. A condition with no browser form makes
    /// the rule server-only.
    /// </summary>
    internal sealed class ClientScope
    {
        private readonly IReadOnlyList<ClientCondition> _conditions;

        private ClientScope(IReadOnlyList<ClientCondition> conditions, bool isServerOnly)
        {
            _conditions = conditions;
            IsServerOnly = isServerOnly;
        }

        internal static ClientScope Unconditional { get; } = new ClientScope(Array.Empty<ClientCondition>(), false);

        internal bool IsServerOnly { get; }

        internal bool IsUnconditional => !IsServerOnly && _conditions.Count == 0;

        /// <summary>The scope of a rule declared now: <paramref name="open"/> enumerates innermost first.</summary>
        internal static ClientScope Of(IEnumerable<ClientCondition?> open)
        {
            var outermostFirst = open.Reverse().ToArray();
            if (outermostFirst.Any(condition => condition == null))
                return new ClientScope(Array.Empty<ClientCondition>(), true);

            return outermostFirst.Length == 0
                ? Unconditional
                : new ClientScope(outermostFirst.Select(condition => condition!).ToArray(), false);
        }

        /// <summary>
        /// The activation of a browser rule in this scope. The fields the conditions read join the rule
        /// set first, so the browser can read them.
        /// </summary>
        internal ClientRuleActivation Activation(ClientValidationRuleSet rules)
        {
            if (IsServerOnly) throw new InvalidOperationException("A server-only rule has no browser activation.");
            if (_conditions.Count == 0) return ClientRuleActivation.Always;

            var condition = ClientCondition.AllOf(_conditions);
            rules.EnsureFields(condition.Fields);
            return ClientRuleActivation.When(condition.Condition);
        }
    }
}
