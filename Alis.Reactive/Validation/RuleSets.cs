using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alis.Reactive.Validation
{
    /// <summary>The sets a rule is declared in: none, or the names one <c>RuleSet("Issue, Renew")</c> lists.</summary>
    internal sealed class RuleSets
    {
        private static readonly char[] Separators = { ',', ';' };

        private RuleSets(IReadOnlyList<string> names)
        {
            Names = names;
        }

        internal static RuleSets None { get; } = new RuleSets(Array.Empty<string>());

        /// <summary>The names one <c>RuleSet</c> declares: split on ',' or ';' as FluentValidation reads them, trimmed; a blank piece is refused.</summary>
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

    /// <summary>Which rules one validation runs: those in no set (<c>Validate(instance)</c>), or those in one named set.</summary>
    internal sealed class RuleSetSelection
    {
        // null: no set is selected. A blank name is refused as a set's name (RuleSets.Named, ReactiveValidator.Select),
        // so no text could stand for the absence without becoming a sentinel.
        private readonly string? _ruleSet;

        private RuleSetSelection(string? ruleSet)
        {
            _ruleSet = ruleSet;
        }

        internal static RuleSetSelection Default { get; } = new RuleSetSelection(null);

        internal static RuleSetSelection Of(string ruleSet) => new RuleSetSelection(ruleSet ?? throw new ArgumentNullException(nameof(ruleSet)));

        internal bool IsNamed => _ruleSet != null;

        /// <summary>Whether a rule in these sets answers this selection: a rule in no set answers no selection, a rule in a set the one that names it.</summary>
        internal bool Selects(RuleSets sets) => _ruleSet == null ? !sets.Any : sets.Contains(_ruleSet);

        // A validator written another way than ReactiveValidator is asked through IValidator<T>, by the overload the selection needs.
        internal ValidationResult Validate<TModel>(IValidator<TModel> validator, TModel instance) =>
            _ruleSet == null ? validator.Validate(instance) : validator.Validate(instance, _ruleSet);

        internal Task<ValidationResult> ValidateAsync<TModel>(IValidator<TModel> validator, TModel instance, CancellationToken cancellation) =>
            _ruleSet == null ? validator.ValidateAsync(instance, cancellation) : validator.ValidateAsync(instance, _ruleSet, cancellation);

        public override string ToString() => _ruleSet ?? "no rule set";
    }
}
