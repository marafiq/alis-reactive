using Alis.Reactive.Plugins;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class PageLog : Plugin
{
    public PageLog()
        : base("page-log")
    {
    }

    public void Log(Arg<string?> text) => Method([text]);
}
