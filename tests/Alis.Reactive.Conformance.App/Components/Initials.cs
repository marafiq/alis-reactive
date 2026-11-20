using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Components;

[Mirrors("Components/initials", "initials")]
public sealed class Initials : Plugin
{
    public Initials()
        : base("initials")
    {
    }

    public MethodResult<string> Of(Arg<string> name) => CallItself<string>([name]);
}
