namespace Alis.Reactive.ClassNames.App.Notices;

public sealed class Notice
{
    public string? Title { get; set; }
}

public sealed class NoticeRules : ReactiveValidator<Notice>
{
    public NoticeRules()
    {
        RuleFor(x => x.Title).NotEmpty();
    }
}

public sealed class Saved
{
    public string Summary { get; set; } = "";
}
