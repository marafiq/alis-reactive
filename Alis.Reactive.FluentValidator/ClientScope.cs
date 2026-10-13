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

        /// <summary>The condition a browser rule in this scope applies under.</summary>
        internal ClientCondition Condition
        {
            get
            {
                if (IsServerOnly) throw new InvalidOperationException("A server-only rule has no browser condition.");
                return _conditions.Count == 0 ? ClientCondition.Always : BrowserCondition.AllOf(_conditions);
            }
        }
    }
}
