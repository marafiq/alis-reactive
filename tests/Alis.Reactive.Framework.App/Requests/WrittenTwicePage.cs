using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Requests;

/// <summary>A visit booked by a request that writes one of its parts twice, so it is refused when it renders.</summary>
public sealed class WrittenTwicePage
{
    public RequestPart Part { get; set; }

    public string? Visitor { get; set; }
}

/// <summary>The parts of a request, each written once.</summary>
public enum RequestPart
{
    Gather,
    Validate,
    WhileLoading,
    Response,
    Finally,
    BodyFormat,
}

/// <summary>The booking's rules: a visit names its visitor.</summary>
public sealed class WrittenTwiceRules : ReactiveValidator<WrittenTwicePage>
{
    public WrittenTwiceRules()
    {
        RuleFor(x => x.Visitor).NotEmpty().WithMessage("Name the visitor.");
    }
}
