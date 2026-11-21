using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.Plugins;
using Alis.Reactive.RenderingAndDelivery;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Native.NativeAppLevelObjects;

internal sealed class NativeRowActionsPlugin : Plugin
{
    public NativeRowActionsPlugin()
        : base("native-row-actions")
    {
    }

    public RowControls<TRow> Controls<TRow>(Arg<IReadOnlyList<RowEntry<TRow>>> entries) => ReturnedObject<RowControls<TRow>>([entries]);
}

internal sealed class RowControls<TRow> : EventedObject
{
    protected override EventListening Listening => EventListening.EventTarget;

    public Event<RowActivated<TRow>> Activated => Event<RowActivated<TRow>>();
}
