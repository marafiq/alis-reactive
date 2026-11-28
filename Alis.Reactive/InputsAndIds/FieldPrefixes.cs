using System;

namespace Alis.Reactive.InputsAndIds;

internal static class FieldPrefixes
{
    internal static void RefuseAMoveAfterThePlanStarted(string started, string now, Func<string> view, string name)
    {
        if (started == now) return;
        throw new InvalidOperationException(
            $"The field prefix of {view()} changed from {Described(started)} to {Described(now)} after its plan started, so input '{name}' would post under a name its plan does not send or check.");
    }

    internal static void RefuseAMoveInsideThePlan(string started, string now, Func<string> view)
    {
        if (started == now) return;
        throw new InvalidOperationException(
            $"The field prefix of {view()} changed from {Described(started)} to {Described(now)} inside its plan, so what the view renders after the change posts under names its plan does not send or check: render a member's view with Html.PartialFor, which leaves the view's prefix as it is.");
    }

    private static string Described(string prefix) => prefix.Length == 0 ? "none" : $"'{prefix}'";
}
