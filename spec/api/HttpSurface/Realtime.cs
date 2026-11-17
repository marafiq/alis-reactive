using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Alis.Reactive.Components;
using Alis.Reactive.Values;

namespace Alis.Reactive.Realtime;

public abstract class SignalRHub : JsObject
{
    protected MethodResult<TResult> Invoke<TResult>(IReadOnlyList<Arg> arguments, [CallerMemberName] string declaredBy = "") => throw new NotSupportedException();

    protected void Send(IReadOnlyList<Arg> arguments, [CallerMemberName] string declaredBy = "") => throw new NotSupportedException();
}
