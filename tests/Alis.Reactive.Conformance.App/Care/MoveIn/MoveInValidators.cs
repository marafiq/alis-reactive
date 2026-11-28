using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

internal static partial class MoveInValidators
{
    internal static ReactiveFluentValidationBuilder AddTo(ReactiveFluentValidationBuilder validators)
    {
        validators.Add<MoveInRules>();
        ForTheResident(validators);
        ForTheContacts(validators);
        ForTheMedications(validators);
        ForTheServices(validators);
        ForTheDiet(validators);
        ForTheAllocationReview(validators);
        ForTheServiceReview(validators);
        ForThePayerVerification(validators);
        ForTheWaitlist(validators);
        ForTheDocumentsUpload(validators);
        ForTheDeposit(validators);
        ForTheReception(validators);
        ForTheCoordinatorLine(validators);
        ForTheOwners(validators);
        return validators;
    }

    static partial void ForTheResident(ReactiveFluentValidationBuilder validators);

    static partial void ForTheContacts(ReactiveFluentValidationBuilder validators);

    static partial void ForTheMedications(ReactiveFluentValidationBuilder validators);

    static partial void ForTheServices(ReactiveFluentValidationBuilder validators);

    static partial void ForTheDiet(ReactiveFluentValidationBuilder validators);

    static partial void ForTheAllocationReview(ReactiveFluentValidationBuilder validators);

    static partial void ForTheServiceReview(ReactiveFluentValidationBuilder validators);

    static partial void ForThePayerVerification(ReactiveFluentValidationBuilder validators);

    static partial void ForTheWaitlist(ReactiveFluentValidationBuilder validators);

    static partial void ForTheDocumentsUpload(ReactiveFluentValidationBuilder validators);

    static partial void ForTheDeposit(ReactiveFluentValidationBuilder validators);

    static partial void ForTheReception(ReactiveFluentValidationBuilder validators);

    static partial void ForTheCoordinatorLine(ReactiveFluentValidationBuilder validators);

    static partial void ForTheOwners(ReactiveFluentValidationBuilder validators);
}
