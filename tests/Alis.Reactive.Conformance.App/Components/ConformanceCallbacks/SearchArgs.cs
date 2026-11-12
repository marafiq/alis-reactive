using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class SearchArgs : JsObject
{
    public ReadOnly<string> Query => Argument<string>(0);

    public void Show(Arg<IReadOnlyList<string>> names) => MethodAt(1, [names]);
}
