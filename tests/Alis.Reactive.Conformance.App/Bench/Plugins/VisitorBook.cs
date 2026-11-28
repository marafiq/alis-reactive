using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.Bench;

[Mirrors("Bench/Plugins/visitor-desk", "VisitorBook")]
public sealed class VisitorBook : Plugin
{
    public VisitorBook()
        : base("visitorbook")
    {
    }

    public ReadOnly<int> Visits => ReadOnly<int>();

    public Event Signed => Event();
}
