using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Bench;

[Mirrors("Bench/Sources/page-log", "PageLog")]
public sealed class PageLog : Plugin
{
    public PageLog()
        : base("pageLog")
    {
    }

    public void Log(Arg<string?> text) => Method([text]);
}
