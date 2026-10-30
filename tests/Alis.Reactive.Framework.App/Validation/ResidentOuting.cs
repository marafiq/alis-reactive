namespace Alis.Reactive.Framework.App.Validation;

/// <summary>
/// A resident signed out at the front desk for an outing. The desk's screens render who goes out and with
/// whom; when the resident will be back is the server's to ask for. No validator checks the outing in the
/// browser: the server decides it all.
/// </summary>
public sealed class ResidentOuting
{
    public string? Resident { get; set; }

    public string? Companion { get; set; }

    public string? BackBy { get; set; }
}
