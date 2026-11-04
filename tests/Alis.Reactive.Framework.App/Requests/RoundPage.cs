namespace Alis.Reactive.Framework.App.Requests;

public sealed class RoundPage
{
    public Dictionary<int, RoundDose> Doses { get; set; } = new();
}

public sealed class RoundDose
{
    public string? Amount { get; set; }
}
