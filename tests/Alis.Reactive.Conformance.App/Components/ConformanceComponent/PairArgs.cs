using Alis.Reactive.Declarations;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class PairArgs : JsObject
{
    public ReadOnly<string> Key => Argument<string>(0);

    public ReadOnly<Record> Record => Argument<Record>(1);
}
