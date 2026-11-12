using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Validation;

/// <summary>
/// What a <c>Custom</c> check receives: the model, and the doors that add the check's failures.
/// </summary>
/// <typeparam name="T">The model the validator checks.</typeparam>
public sealed class CustomContext<T>
{
    internal CustomContext()
    {
    }

    /// <summary>Gets the model the checked member belongs to.</summary>
    public T Model => throw new NotImplementedException();

    /// <summary>Adds a failure under the member the rule is on.</summary>
    /// <param name="message">The failure's message.</param>
    public void AddFailure(string message) => throw new NotImplementedException();

    /// <summary>Adds a failure under another member of the model.</summary>
    /// <param name="member">The member the failure is under, as a member path.</param>
    /// <param name="message">The failure's message.</param>
    public void AddFailure<TMember>(Expression<Func<T, TMember>> member, string message) => throw new NotImplementedException();
}
