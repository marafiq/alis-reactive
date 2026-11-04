
namespace Alis.Reactive.Framework.App.Inputs;

public sealed class ArrivalPage
{
    public string? Visitor { get; set; }

    public string? Room { get; set; }

    public string? Note { get; set; }

    public string? Pass { get; set; }
}

public sealed class ArrivalEcho
{
    public string Summary { get; set; } = "";
}

public sealed class ArrivalRules : ReactiveValidator<ArrivalPage>
{
    public ArrivalRules()
    {
        RuleFor(x => x.Note).MaximumLength(12).WithMessage("Keep the note to 12 letters.");
        RuleFor(x => x.Pass).NotEmpty().WithMessage("The desk gives every visitor a pass.");
    }
}
