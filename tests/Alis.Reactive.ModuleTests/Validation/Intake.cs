using System.Collections.Generic;
using Alis.Reactive.Validation;

namespace Alis.Reactive.ModuleTests.Validation;

public sealed class Supplies
{
    public int Count { get; set; }
}

public sealed class SuppliesRules : ReactiveValidator<Supplies>
{
    public SuppliesRules() => RuleFor(supplies => supplies.Count).GreaterThan(0);
}

public sealed class DeputyCard
{
    public Deputy? Deputy { get; set; }
}

public sealed class Deputy
{
    public string? Name { get; set; } = "Unnamed";
    public string? Phone { get; set; }
}

public sealed class DeputyRules : ReactiveValidator<Deputy>
{
    public DeputyRules() => RuleFor(deputy => deputy.Name).NotEmpty().WithMessage("Name the deputy.");
}

public sealed class DeputyCardRules : ReactiveValidator<DeputyCard>
{
    public DeputyCardRules() => RuleFor(card => card.Deputy!).SetValidator(new DeputyRules());
}

public sealed class IntakeCard
{
    public string? Name { get; set; } = "Unnamed";
    public string? Status { get; set; }
    public bool Urgent { get; set; }
    public List<Carer> Carers { get; set; } = [new Carer { Name = "Ruth" }];
}

public sealed class Carer
{
    public string? Name { get; set; }
}

public sealed class IntakeCardRules : ReactiveValidator<IntakeCard>
{
    public IntakeCardRules()
    {
        RuleFor(card => card.Status).NotEmpty().When(card => card.Urgent);
        RuleFor(card => card.Name).NotEmpty().WithMessage("Name the resident.");
        RuleForEach(card => card.Carers).ChildRules(carer => carer.RuleFor(c => c.Name).NotEmpty().WithMessage("Name the carer."));
        RuleFor(card => card.Name).Must(name => name != "Nobody").WithMessage("Only the server reads this.");
    }
}

public sealed class HomeCard
{
    public Lead Lead { get; set; } = new() { Name = "Ruth" };
}

public sealed class PopulatedHomeCard
{
    [System.Text.Json.Serialization.JsonObjectCreationHandling(System.Text.Json.Serialization.JsonObjectCreationHandling.Populate)]
    public Lead Lead { get; set; } = new() { Name = "Ruth" };
}

public sealed class Lead
{
    public string? Name { get; set; } = "";
    public string? Phone { get; set; }
}

public sealed class HomeCardRules : ReactiveValidator<HomeCard>
{
    public HomeCardRules() => RuleFor(card => card.Lead.Name).NotEmpty().WithMessage("Lead name is required.");
}

public sealed class PopulatedHomeCardRules : ReactiveValidator<PopulatedHomeCard>
{
    public PopulatedHomeCardRules() => RuleFor(card => card.Lead.Name).NotEmpty().WithMessage("Lead name is required.");
}

public sealed class RoundCard
{
    public string? Ward { get; set; }
    public int Beds { get; set; }
    public int Staff { get; set; }
}

public sealed class RoundCardRules : ReactiveValidator<RoundCard>
{
    public RoundCardRules()
    {
        RuleFor(card => card.Ward).Matches("^[A-Z][0-9]$").MaximumLength(2);
        RuleFor(card => card.Beds).InclusiveBetween(1, 40).WhenOnServer(card => card.Staff > 0);
        RuleFor(card => card.Staff).LessThanOrEqualTo(card => card.Beds).WithMessage("No more staff than beds.");
        RuleFor(card => card.Ward).NotEmpty().ServerOnly();
    }
}

public sealed class UnreadRoundRules : ReactiveValidator<RoundCard>
{
    public UnreadRoundRules() => RuleFor(card => card.Ward).NotEmpty().When(card => card.Ward!.GetHashCode() > 0);
}

public sealed class FoldedRoundRules : ReactiveValidator<RoundCard>
{
    public FoldedRoundRules() => RuleFor(card => card.Ward).Matches("^a[0-9]$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
}

public sealed class WardCard
{
    public string? Ward { get; set; }
}

public sealed class WardCardRules : ReactiveValidator<WardCard>
{
    public WardCardRules() => RuleFor(card => card.Ward).NotNull();
}
