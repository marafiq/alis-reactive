using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

internal static partial class MoveInValidators
{
    static partial void ForTheContacts(ReactiveFluentValidationBuilder validators) => validators.Add<ContactEditRules>();
}
