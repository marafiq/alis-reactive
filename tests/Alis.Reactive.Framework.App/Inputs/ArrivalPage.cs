using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Inputs;

/// <summary>A visitor's arrival at the desk: a field rendered with each option a field wrapper takes, and with none.</summary>
public sealed class ArrivalPage
{
    /// <summary>Required, and labelled.</summary>
    public string? Visitor { get; set; }

    /// <summary>Labelled.</summary>
    public string? Room { get; set; }

    /// <summary>Rendered with no options.</summary>
    public string? Note { get; set; }

    /// <summary>The pass the desk gives, carried hidden.</summary>
    public string? Pass { get; set; }
}

/// <summary>What the endpoint bound, in its own words.</summary>
public sealed class ArrivalEcho
{
    public string Summary { get; set; } = "";
}

/// <summary>The rules an arrival is checked by, written once: the browser checks what it can.</summary>
public sealed class ArrivalRules : ReactiveValidator<ArrivalPage>
{
    public ArrivalRules()
    {
        RuleFor(x => x.Note).MaximumLength(12).WithMessage("Keep the note to 12 letters.");
        RuleFor(x => x.Pass).NotEmpty().WithMessage("The desk gives every visitor a pass.");
    }
}
