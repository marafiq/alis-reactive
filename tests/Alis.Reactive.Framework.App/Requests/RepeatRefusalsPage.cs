namespace Alis.Reactive.Framework.App.Requests;

public sealed class RepeatRefusalsPage
{
    public RepeatedPart Part { get; set; }

    public string? Visitor { get; set; }

    public int Party { get; set; }
}

public enum RepeatedPart
{
    Gather,
    Response,
    WhileLoading,
    Finally,
    Validate,
    BodyFormat,
    OnAllSettled,
}

public sealed class RepeatRules : ReactiveValidator<RepeatRefusalsPage>
{
    public RepeatRules()
    {
        RuleFor(x => x.Visitor).NotEmpty().WithMessage("Name the visitor.");
    }
}
