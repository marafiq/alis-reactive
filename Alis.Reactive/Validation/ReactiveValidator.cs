namespace Alis.Reactive.Validation;

/// <summary>
/// The rules of <typeparamref name="T"/>, written once: the server checks a model by them, and the browser checks
/// what a request sends by the same rules before it sends it.
/// </summary>
/// <typeparam name="T">The model the rules are written for.</typeparam>
public abstract class ReactiveValidator<T>
{
    /// <summary>Makes the validator; a derived class writes its rules in its constructor.</summary>
    protected ReactiveValidator()
    {
    }
}
