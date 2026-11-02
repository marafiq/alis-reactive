using Alis.Reactive.Validation;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Where a condition written over members reads a member's value once it is bound to a container: the
    /// container's input for the field, as MVC binds it. A form supplies it when it binds its rules.
    /// </summary>
    internal abstract class FieldValues
    {
        internal abstract ValueExpression Value(FieldReference field);
    }
}
