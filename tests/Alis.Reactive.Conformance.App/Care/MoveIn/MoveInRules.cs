using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInRules : ReactiveValidator<MoveInPage>
{
    internal const string Draft = "Draft";
    internal const string Submit = "Submit";

    public MoveInRules()
    {
        TheLocation();
        TheResident();
        TheCare();
        TheContacts();
        ThePayer();
        TheMedications();
        TheServices();
        TheDiet();
        TheDocuments();
        RuleSet(Submit, () =>
        {
            RuleFor(m => m.Location.BedId).NotEmpty().WithMessage("Choose a bed.");
            RuleFor(m => m.Services.Codes).NotEmpty().WithMessage("Choose at least one service.");
            RuleFor(m => m.AcceptedQuoteRevision).NotNull().WithMessage("Accept the current quote.");
        });
    }

    partial void TheLocation();

    partial void TheResident();

    partial void TheCare();

    partial void TheContacts();

    partial void ThePayer();

    partial void TheMedications();

    partial void TheServices();

    partial void TheDiet();

    partial void TheDocuments();
}
