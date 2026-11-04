namespace Alis.Reactive.Framework.App.Validation;

public sealed class Nameplate
{
    public string? Resident { get; set; } = "Ada";
}

public enum NameplateName
{
    Spoken,
    Empty,
    Spaces,
}

public sealed class SpokenNameplateRules : ReactiveValidator<Nameplate>
{
    public SpokenNameplateRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithName("Resident's name");
    }
}

public sealed class EmptyNameplateRules : ReactiveValidator<Nameplate>
{
    public EmptyNameplateRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithName("");
    }
}

public sealed class SpacedNameplateRules : ReactiveValidator<Nameplate>
{
    public SpacedNameplateRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithName("   ");
    }
}
