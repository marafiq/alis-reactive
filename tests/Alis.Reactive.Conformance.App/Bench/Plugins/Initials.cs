using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Bench;

[Mirrors("Bench/Plugins/shift-log-desk", "initials")]
public sealed class Initials : Plugin
{
    public Initials()
        : base("initials")
    {
    }

    public MethodResult<string> Of(Arg<string> name) => CallItself<string>([name]);
}
