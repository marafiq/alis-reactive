namespace Alis.Reactive.Framework.App.Requests;

/// <summary>
/// The night desk when its requests fail: a ledger the desk can put away, so a request that reads it or
/// writes into it fails, a keeper slate that stays, and endpoints that drop the connection or answer in a
/// form the page cannot read.
/// </summary>
public sealed class OutagePage;

/// <summary>A note about the ledger, sent with the ledger's title.</summary>
public sealed class LedgerNote
{
    public string? Title { get; set; }
}

/// <summary>The keeper's limit, which fills the path; the keeper has none until the page sets it.</summary>
public sealed class LimitRoute
{
    public int? Limit { get; set; }
}
