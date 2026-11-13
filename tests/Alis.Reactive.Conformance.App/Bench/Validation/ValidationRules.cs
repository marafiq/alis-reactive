using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Bench;

public sealed class CodeRules : ReactiveValidator<ConformanceForm>
{
    public CodeRules() => RuleFor(form => form.Code).NotEmpty().WithMessage("Pick a code.");
}

public sealed class StrictCodeRules : ReactiveValidator<ConformanceForm>
{
    public StrictCodeRules() => RuleFor(form => form.Code).NotEmpty().WithMessage("Pick a code.").When(form => form.IsStrict);
}

public sealed class EntryRules : ReactiveValidator<ConformanceEntry>
{
    public EntryRules() => RuleFor(entry => entry.Code).NotEmpty().WithMessage("Pick a code.");
}

public sealed class EntriesRules : ReactiveValidator<ConformanceEntries>
{
    public EntriesRules() => RuleForEach(form => form.Entries).SetValidator(new EntryRules());
}

public sealed class FirstRules : ReactiveValidator<ConformanceFirst>
{
    public FirstRules() => RuleFor(form => form.First).SetValidator(new EntryRules());
}
