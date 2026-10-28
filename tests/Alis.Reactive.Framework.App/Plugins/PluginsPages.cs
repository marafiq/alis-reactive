namespace Alis.Reactive.Framework.App.Plugins;

/// <summary>A page that counts on the tally plugin.</summary>
public sealed class TallyPage;

/// <summary>A page that writes in a ledger no script on the page registers.</summary>
public sealed class LedgerPage;

/// <summary>A page whose scripts register a plugin the runtime refuses.</summary>
public sealed class RefusedPluginPage;

/// <summary>A ledger the page registers as a plugin; no script on the page registers its object.</summary>
public sealed class Ledger : Plugin
{
    public Ledger()
        : base("ledger")
    {
    }

    public void Add(Arg<int> amount) => Method(amount);
}
