using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInRules : ReactiveValidator<MoveInPage>
{
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
