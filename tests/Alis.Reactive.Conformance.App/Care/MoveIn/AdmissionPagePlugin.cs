using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Care;

[Mirrors(module: "Care/MoveIn/admission-page", name: "AdmissionPage")]
public sealed class AdmissionPagePlugin : Plugin
{
    public AdmissionPagePlugin()
        : base("admission-page")
    {
    }

    public ReadOnly<string> Url => ReadOnly<string>();

    public void Prepare(TypedSource<AdmissionPrintPacket> packet) => Method([Fixed("admission"), Pass<IAdmissionPrintPacket, AdmissionPrintPacket>(packet)]);

    public void Print() => MethodAsync();
}
