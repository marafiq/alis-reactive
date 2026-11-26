using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class LeastDoseRound
{
    public int Least { get; set; }

    public List<int> Amounts { get; set; } = [];
}

public sealed class LeastDoseRoundRules : ReactiveValidator<LeastDoseRound>
{
    public LeastDoseRoundRules() => RuleForEach(round => round.Amounts).GreaterThanOrEqualTo(round => round.Least).WithMessage("Give at least the least dose.");
}
