
namespace Alis.Reactive.Framework.App.Bindings;

public sealed class RoundPage
{
    public string? Room { get; set; }
}

public sealed class RoundRules : ReactiveValidator<RoundPage>
{
    public RoundRules()
    {
        RuleFor(x => x.Room).NotEmpty();
    }
}

public sealed class Finding
{
    public string? Desk { get; set; }

    public string? Found { get; set; }

    public string? Standby { get; set; }
}

public sealed class Answer
{
    public string Summary { get; set; } = "";
}
