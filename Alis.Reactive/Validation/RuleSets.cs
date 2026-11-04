using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alis.Reactive.Validation
{
    internal sealed class RuleSets
    {
        private static readonly char[] Separators = { ',', ';' };

        private RuleSets(IReadOnlyList<string> names)
        {
            Names = names;
        }

        internal static RuleSets None { get; } = new RuleSets(Array.Empty<string>());

        internal static RuleSets Named(string ruleSet)
        {
            if (ruleSet == null) throw new ArgumentNullException(nameof(ruleSet));
            var names = ruleSet.Split(Separators).Select(name => name.Trim()).ToArray();
            if (names.Any(name => name.Length == 0))
                throw new ArgumentException("A rule set has a name people read: RuleSet(\"Issue\", ..) or RuleSet(\"Issue, Renew\", ..).", nameof(ruleSet));
            return new RuleSets(names);
        }

        internal IReadOnlyList<string> Names { get; }

        internal bool Any => Names.Count > 0;

        internal bool Contains(string name) => Names.Contains(name, StringComparer.Ordinal);

        public override string ToString() => string.Join(", ", Names);
    }

    internal sealed class RuleSetSelection
    {
        private readonly string? _ruleSet;

        private RuleSetSelection(string? ruleSet)
        {
            _ruleSet = ruleSet;
        }

        internal static RuleSetSelection Default { get; } = new RuleSetSelection(null);

        internal static RuleSetSelection Of(string ruleSet) => new RuleSetSelection(ruleSet ?? throw new ArgumentNullException(nameof(ruleSet)));

        internal bool IsNamed => _ruleSet != null;

        internal bool Selects(RuleSets sets) => _ruleSet == null ? !sets.Any : sets.Contains(_ruleSet);

        internal ValidationResult Validate<TModel>(IValidator<TModel> validator, TModel instance)
            => _ruleSet == null ? validator.Validate(instance) : validator.Validate(instance, _ruleSet);

        internal Task<ValidationResult> ValidateAsync<TModel>(IValidator<TModel> validator, TModel instance, CancellationToken cancellation)
            => _ruleSet == null ? validator.ValidateAsync(instance, cancellation) : validator.ValidateAsync(instance, _ruleSet, cancellation);

        public override string ToString() => _ruleSet ?? "no rule set";
    }
}
