using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;

namespace Alis.Reactive.Conformance.App.World;

[Mirrors("World/care-session", "CareSession")]
public sealed class CareSession : Plugin
{
    public CareSession()
        : base("session")
    {
    }

    public void Renew() => MethodAsync();

    public void Flush() => MethodAsync();
}
