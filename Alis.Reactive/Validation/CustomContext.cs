using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Validation;

/// <summary>
/// What a <c>Custom</c> check receives: the model, and the doors that add the check's failures.
/// </summary>
/// <typeparam name="T">The model the validator checks.</typeparam>
public sealed class CustomContext<T>
{
    private readonly List<ValidationFailure> _failures;
    private readonly string _prefix;
    private readonly string _fieldPath;

    internal CustomContext(T model, List<ValidationFailure> failures, string prefix, string fieldPath)
    {
        Model = model;
        _failures = failures;
        _prefix = prefix;
        _fieldPath = fieldPath;
    }

    /// <summary>Gets the model the checked member belongs to.</summary>
    public T Model { get; }

    /// <summary>Adds a failure under the member the rule is on.</summary>
    /// <param name="message">The failure's message.</param>
    public void AddFailure(string message) => _failures.Add(new ValidationFailure(_prefix + _fieldPath, message));

    /// <summary>Adds a failure under another member of the model.</summary>
    /// <param name="member">The member the failure is under, as a member path.</param>
    /// <param name="message">The failure's message.</param>
    public void AddFailure<TMember>(Expression<Func<T, TMember>> member, string message) => _failures.Add(new ValidationFailure(_prefix + MvcText.Of(member), message));
}
