using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Care;

[Mirrors("Care/MoveIn/coordinator-line-format", "CoordinatorLineFormat")]
public sealed class CoordinatorLineFormat : Plugin
{
    public CoordinatorLineFormat()
        : base("coordinator-line-format")
    {
    }

    public MethodResult<string> Members(Arg<IReadOnlyList<string>> names) => Method<string>([names]);

    public MethodResult<string> Transcript(Arg<List<CoordinatorMessage>> messages) => Method<string>([messages]);
}
