namespace Alis.Reactive.Framework.App.Validation;

public sealed class ResidentReturn
{
    public string? Resident { get; set; }

    public string? Reason { get; set; }
}

public sealed class ResidentReturnRules : ReactiveValidator<ResidentReturn>
{
    public ResidentReturnRules()
    {
        RuleFor(x => x.Reason).Must(GivesAReason).WithMessage("Say why the resident came back late.");
    }

    private static bool GivesAReason(string? reason) => !string.IsNullOrWhiteSpace(reason);
}
