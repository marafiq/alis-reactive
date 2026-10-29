namespace Alis.Reactive.Framework.App.Requests;

/// <summary>An evening medication round: each resident's dose, kept by resident number, recorded on a card of its own.</summary>
public sealed class RoundPage
{
    public Dictionary<int, RoundDose> Doses { get; set; } = new();
}

/// <summary>One resident's dose: a card rendered for the round's entry under that resident's number.</summary>
public sealed class RoundDose
{
    public string? Amount { get; set; }
}
