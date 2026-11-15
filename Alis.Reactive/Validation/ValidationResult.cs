using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.Validation;

/// <summary>
/// One failure of a check: the member it is under and its message.
/// </summary>
public sealed class ValidationFailure
{
    internal ValidationFailure(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }

    /// <summary>Gets the name of the member the failure is under.</summary>
    public string PropertyName { get; }

    /// <summary>Gets the failure's message.</summary>
    public string ErrorMessage { get; }
}

/// <summary>
/// What a check of a model found.
/// </summary>
public sealed class ValidationResult
{
    internal ValidationResult(IReadOnlyList<ValidationFailure> errors)
    {
        Errors = errors;
    }

    /// <summary>Gets whether the model broke no rule.</summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>Gets every failure the check found.</summary>
    public IReadOnlyList<ValidationFailure> Errors { get; }

    /// <summary>Gets the messages of the failures, by the member each is under.</summary>
    public IReadOnlyDictionary<string, string[]> ErrorsByField => Errors
        .GroupBy(error => error.PropertyName, System.StringComparer.Ordinal)
        .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray(), System.StringComparer.Ordinal);
}
