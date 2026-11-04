using Alis.Reactive.Inputs;
using Alis.Reactive.PlanModel.Values;

namespace Alis.Reactive.PlanModel.Conditions
{
    internal abstract class FieldValues
    {
        internal abstract ValueExpression Value(FieldReference field);
    }
}
