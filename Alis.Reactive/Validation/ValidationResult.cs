using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation;

/// <summary>
/// One failure of a check: the member it is under and its message.
/// </summary>
public sealed class ValidationFailure
{
    internal ValidationFailure()
    {
    }

    /// <summary>Gets the name of the member the failure is under.</summary>
    public string PropertyName => throw new NotImplementedException();

    /// <summary>Gets the failure's message.</summary>
    public string ErrorMessage => throw new NotImplementedException();
}

/// <summary>
/// What a check of a model found.
/// </summary>
public sealed class ValidationResult
{
    internal ValidationResult()
    {
    }

    /// <summary>Gets whether the model broke no rule.</summary>
    public bool IsValid => throw new NotImplementedException();

    /// <summary>Gets every failure the check found.</summary>
    public IReadOnlyList<ValidationFailure> Errors => throw new NotImplementedException();

    /// <summary>Gets the messages of the failures, by the member each is under.</summary>
    public IReadOnlyDictionary<string, string[]> ErrorsByField => throw new NotImplementedException();
}
