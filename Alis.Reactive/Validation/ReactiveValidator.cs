using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Alis.Reactive.Validation;

/// <summary>
/// Checks a model of <typeparamref name="T"/> by its rules.
/// </summary>
/// <typeparam name="T">The model the validator checks.</typeparam>
public interface IValidator<in T>
{
    /// <summary>Checks a model by the validator's rules.</summary>
    /// <param name="instance">The model.</param>
    /// <returns>What the check found.</returns>
    ValidationResult Validate(T instance);

    /// <summary>Checks a model by the rules of one rule set.</summary>
    /// <param name="instance">The model.</param>
    /// <param name="ruleSet">The rule set's name, as <c>RuleSet</c> declares it.</param>
    /// <returns>What the check found.</returns>
    ValidationResult Validate(T instance, string ruleSet);

    /// <summary>Checks a model by the validator's rules, awaiting the rules that wait.</summary>
    /// <param name="instance">The model.</param>
    /// <param name="cancellation">Cancels the rules that wait.</param>
    /// <returns>What the check found.</returns>
    Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default);

    /// <summary>Checks a model by the rules of one rule set, awaiting the rules that wait.</summary>
    /// <param name="instance">The model.</param>
    /// <param name="ruleSet">The rule set's name, as <c>RuleSet</c> declares it.</param>
    /// <param name="cancellation">Cancels the rules that wait.</param>
    /// <returns>What the check found.</returns>
    Task<ValidationResult> ValidateAsync(T instance, string ruleSet, CancellationToken cancellation = default);
}

/// <summary>
/// A validator of the framework, whatever model it checks: what <c>AddReactiveFluentValidation</c> registers.
/// </summary>
/// <remarks>Derive from <see cref="ReactiveValidator{T}"/>.</remarks>
public abstract class ReactiveValidator
{
    private protected ReactiveValidator()
    {
    }
}

/// <summary>
/// The rules of <typeparamref name="T"/>, written once: the server checks a model by them, and the browser checks
/// what a request sends by the same rules before it sends it.
/// </summary>
/// <remarks>
/// Some rules only the server decides, by what they are: <c>Must</c>, <c>MustAsync</c>, <c>Custom</c> and
/// <c>CustomAsync</c>, a rule under <c>WhenAsync</c>, <c>UnlessAsync</c>, <c>WhenOnServer</c> or
/// <c>UnlessOnServer</c>, a message built from the model, a rule marked <c>ServerOnly()</c> and a child validator
/// that is no <see cref="ReactiveValidator"/>.
/// </remarks>
/// <typeparam name="T">The model the rules are written for.</typeparam>
public abstract class ReactiveValidator<T> : ReactiveValidator, IValidator<T>
{
    /// <summary>Makes the validator; a derived class writes its rules in its constructor.</summary>
    [SuppressMessage("ApiDesign", "RS0022:Constructor make noninheritable base class inheritable", Justification = "An app derives from the typed validator; the untyped base stays closed and holds no protected member.")]
    protected ReactiveValidator()
    {
    }

    /// <summary>Gets or sets the cascade mode of every rule of the validator that names none of its own.</summary>
    protected CascadeMode RuleLevelCascadeMode
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    /// <summary>Starts the rule of one member of the model.</summary>
    /// <param name="member">The member, as a member path.</param>
    /// <returns>The rule, which takes its checks.</returns>
    public IRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> member)
        => throw new NotImplementedException();

    /// <summary>Starts the rule of each item of a list the model holds.</summary>
    /// <param name="collection">The list, as a member path.</param>
    /// <returns>The rule, which takes the checks of an item.</returns>
    public IRuleBuilder<T, TElement> RuleForEach<TElement>(Expression<Func<T, IEnumerable<TElement>>> collection)
        => throw new NotImplementedException();

    /// <summary>Adds the rules of another validator of the same model to this validator's.</summary>
    /// <param name="rules">The validator whose rules are added.</param>
    public void Include(IValidator<T> rules) => throw new NotImplementedException();

    /// <summary>Declares the rules written inside as members of a named rule set.</summary>
    /// <remarks>
    /// A check runs one validator under one rule set: a request is checked under the default rule set unless it
    /// names another.
    /// </remarks>
    /// <param name="ruleSet">The rule set's name.</param>
    /// <param name="rules">Declares the rules of the set.</param>
    public void RuleSet(string ruleSet, Action rules) => throw new NotImplementedException();

    /// <summary>Applies the rules written inside only when the condition holds.</summary>
    /// <remarks>
    /// The server and the browser each decide the condition. One the browser cannot decide is refused where the
    /// view renders: declare it the server's with <see cref="WhenOnServer"/>.
    /// </remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder When(Expression<Func<T, bool>> predicate, Action rules) => throw new NotImplementedException();

    /// <summary>Applies the rules written inside only when the condition does not hold.</summary>
    /// <remarks>
    /// The server and the browser each decide the condition. One the browser cannot decide is refused where the
    /// view renders: declare it the server's with <see cref="UnlessOnServer"/>.
    /// </remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder Unless(Expression<Func<T, bool>> predicate, Action rules) => throw new NotImplementedException();

    /// <summary>Applies the rules written inside only when the condition holds, decided by the server alone.</summary>
    /// <remarks>The rules under it stay whole on the server.</remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder WhenOnServer(Expression<Func<T, bool>> predicate, Action rules) => throw new NotImplementedException();

    /// <summary>Applies the rules written inside only when the condition does not hold, decided by the server alone.</summary>
    /// <remarks>The rules under it stay whole on the server.</remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder UnlessOnServer(Expression<Func<T, bool>> predicate, Action rules) => throw new NotImplementedException();

    /// <summary>Applies the rules written inside only when an awaited condition holds, decided by the server alone.</summary>
    /// <param name="predicate">The condition over the model, answered by a task.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder WhenAsync(Func<T, CancellationToken, Task<bool>> predicate, Action rules) => throw new NotImplementedException();

    /// <summary>Applies the rules written inside only when an awaited condition does not hold, decided by the server alone.</summary>
    /// <param name="predicate">The condition over the model, answered by a task.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder UnlessAsync(Func<T, CancellationToken, Task<bool>> predicate, Action rules) => throw new NotImplementedException();

    /// <inheritdoc />
    public ValidationResult Validate(T instance) => throw new NotImplementedException();

    /// <inheritdoc />
    public ValidationResult Validate(T instance, string ruleSet) => throw new NotImplementedException();

    /// <inheritdoc />
    public Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default) => throw new NotImplementedException();

    /// <inheritdoc />
    public Task<ValidationResult> ValidateAsync(T instance, string ruleSet, CancellationToken cancellation = default) => throw new NotImplementedException();
}

/// <summary>
/// A validator whose rules are written where it is made: the validator <c>ChildRules</c> hands its lambda.
/// </summary>
/// <typeparam name="T">The model the rules are written for.</typeparam>
public sealed class InlineValidator<T> : ReactiveValidator<T>
{
    /// <summary>Makes a validator with no rule.</summary>
    public InlineValidator()
    {
    }
}
