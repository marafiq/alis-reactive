namespace Alis.Reactive.Framework.App.Plugins;

public sealed class TallyPage;

public sealed class LedgerPage;

public sealed class RefusedPluginPage;

public sealed class Ledger : Plugin
{
    public Ledger()
        : base("ledger")
    {
    }

    public void Add(Arg<int> amount) => Method(amount);
}
