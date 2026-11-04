namespace Alis.Reactive.Framework.App.Validation;

public sealed class MedicationRound
{
    public string? Cart { get; set; } = "C-1";

    public string? Nurse { get; set; } = "RN-Ada";
}

public sealed class MedicationRoundRules : ReactiveValidator<MedicationRound>
{
    public MedicationRoundRules()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Cart).Cascade(CascadeMode.Continue)
            .Must(cart => cart != null && cart.StartsWith("C-")).WithMessage("Carts start with C-.")
            .Must(cart => cart != null && cart.Length == 3).WithMessage("Three characters a cart.");
        RuleFor(x => x.Nurse)
            .Must(nurse => nurse != null && nurse.StartsWith("RN-")).WithMessage("A nurse's badge starts with RN-.")
            .Must(nurse => nurse != null && nurse.Length <= 6).WithMessage("Six characters a badge.");
    }
}
