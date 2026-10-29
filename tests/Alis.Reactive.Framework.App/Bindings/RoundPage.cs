using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Bindings;

/// <summary>The night round: a slate of residents, and the room the round reports from.</summary>
public sealed class RoundPage
{
    public string? Room { get; set; }
}

/// <summary>A round reports from a room.</summary>
public sealed class RoundRules : ReactiveValidator<RoundPage>
{
    public RoundRules()
    {
        RuleFor(x => x.Room).NotEmpty();
    }
}

/// <summary>What the round tells a desk: what it found, and what the standby slate found.</summary>
public sealed class Finding
{
    public string? Desk { get; set; }

    public string? Found { get; set; }

    public string? Standby { get; set; }
}

/// <summary>What a desk answered, in its own words.</summary>
public sealed class Answer
{
    public string Summary { get; set; } = "";
}
