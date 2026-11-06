using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel.Writing
{
    internal static class FormBody
    {
        internal static Expr Live(Scope scope, string formId) => Filled(scope, formId, G.Record());

        internal static Expr Filled(Scope scope, string containerId, Expr beside)
            => G.Call(G.Obj(scope.Objects.Inputs(containerId)), PathMembers.Required, "fill", beside);
    }
}
