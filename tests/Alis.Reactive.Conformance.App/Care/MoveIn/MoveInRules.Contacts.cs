using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInRules
{
    partial void TheContacts() => RuleForEach(moveIn => moveIn.Contacts.Rows).SetValidator(new ContactRules(reference => false));
}

public sealed partial class ContactRules : ReactiveValidator<ContactInput>
{
    private readonly Func<string, bool> _isRevoked;

    public ContactRules(Func<string, bool> isRevoked)
    {
        _isRevoked = isRevoked;
        RuleFor(contact => contact.Name).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Enter the contact's name.")
            .Length(2, 100).WithMessage("A contact's name has 2 to 100 characters.");
        RuleFor(contact => contact.Email).EmailAddress().WithMessage("Enter a valid contact email.");
        RuleFor(contact => contact.Phone).Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Enter the contact's phone.")
            .Matches("^[0-9]{10}$").WithMessage("A phone number has ten digits.");
        RuleFor(contact => contact.AlternatePhone).NotEqual(contact => contact.Phone).WithMessage("The alternate phone differs from the phone.").When(contact => contact.AlternatePhone != "");
        TheAuthority();
    }

    partial void TheAuthority();
}

internal sealed class ContactEditRules : ReactiveValidator<ContactEdit>
{
    internal const string Removal = "Remove";

    public ContactEditRules(MoveInReads reads)
    {
        var aContact = new ContactRules(reads.HoldsRevoked);
        RuleForEach(edit => edit.Contacts.Rows).SetValidator(aContact);
        RuleFor(edit => edit.NewContact!).SetValidator(aContact);
        RuleSet(Removal, () => RuleFor(edit => edit.RemoveId).NotEmpty().WithMessage("Choose a contact."));
    }
}
