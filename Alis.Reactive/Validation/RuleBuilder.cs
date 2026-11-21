using System;
using System.Linq.Expressions;

namespace Alis.Reactive.Validation;

/// <summary>
/// The rule of one member, which takes its checks.
/// </summary>
/// <typeparam name="T">The model the validator checks.</typeparam>
/// <typeparam name="TProperty">The member's type.</typeparam>
public interface IRuleBuilder<T, out TProperty>
{
    /// <summary>Checks the member's value by a validator of its own type.</summary>
    /// <remarks>
    /// A member that holds null is skipped. A validator that is no <see cref="ReactiveValidator"/> checks on the
    /// server alone.
    /// </remarks>
    /// <param name="validator">The validator of the member's type.</param>
    /// <returns>The rule, which takes its options.</returns>
    IRuleBuilderOptions<T, TProperty> SetValidator(IValidator<TProperty> validator);

    /// <summary>Sets what the rule does after one of its checks fails.</summary>
    /// <param name="mode">What the rule does.</param>
    /// <returns>The rule, which takes its checks.</returns>
    IRuleBuilder<T, TProperty> Cascade(CascadeMode mode);
}

/// <summary>
/// What a rule does after one of its checks fails.
/// </summary>
public enum CascadeMode
{
    /// <summary>Runs the rule's remaining checks.</summary>
    Continue,

    /// <summary>Stops the rule at its first failing check.</summary>
    Stop,
}

/// <summary>
/// The rule of one member after a check: the check's message, the member's name, the rule's conditions and where
/// it is checked.
/// </summary>
/// <typeparam name="T">The model the validator checks.</typeparam>
/// <typeparam name="TProperty">The member's type.</typeparam>
public interface IRuleBuilderOptions<T, out TProperty> : IRuleBuilder<T, TProperty>
{
    /// <summary>Sets the message of the check just written, in place of its default.</summary>
    /// <remarks>
    /// The text may name <c>{PropertyName}</c>, which reads the name <c>WithName</c> gives the member, else the
    /// member's path split into words. The browser shows the same text.
    /// </remarks>
    /// <param name="message">The message's text.</param>
    /// <returns>The rule, which takes its options.</returns>
    IRuleBuilderOptions<T, TProperty> WithMessage(string message);

    /// <summary>Sets a message built from the model.</summary>
    /// <remarks>Only the server builds it, so the check it names stays on the server.</remarks>
    /// <param name="message">Builds the message's text from the model.</param>
    /// <returns>The rule, which takes its options.</returns>
    IRuleBuilderOptions<T, TProperty> WithMessage(Func<T, string> message);

    /// <summary>Sets the name the rule's messages say for the member.</summary>
    /// <param name="displayName">The name people read.</param>
    /// <returns>The rule, which takes its options.</returns>
    IRuleBuilderOptions<T, TProperty> WithName(string displayName);

    /// <summary>Declares the rules that depend on this rule.</summary>
    /// <param name="rules">Declares the dependent rules.</param>
    /// <returns>The rule, which takes its options.</returns>
    IRuleBuilderOptions<T, TProperty> DependentRules(Action rules);

    /// <summary>Checks the rule only when the condition holds.</summary>
    /// <remarks>
    /// The server and the browser each decide the condition. One the browser cannot decide is refused where the
    /// view renders: declare it the server's with <see cref="WhenOnServer"/>.
    /// </remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <returns>The rule, which takes its options.</returns>
    IRuleBuilderOptions<T, TProperty> When(Expression<Func<T, bool>> predicate);

    /// <summary>Checks the rule only when the condition does not hold.</summary>
    /// <remarks>
    /// The server and the browser each decide the condition. One the browser cannot decide is refused where the
    /// view renders: declare it the server's with <see cref="UnlessOnServer"/>.
    /// </remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <returns>The rule, which takes its options.</returns>
    IRuleBuilderOptions<T, TProperty> Unless(Expression<Func<T, bool>> predicate);

    /// <summary>Checks the rule only when the condition holds, decided by the server alone.</summary>
    /// <remarks>The rule stays whole on the server.</remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <returns>The rule, which takes its options.</returns>
    IRuleBuilderOptions<T, TProperty> WhenOnServer(Expression<Func<T, bool>> predicate);

    /// <summary>Checks the rule only when the condition does not hold, decided by the server alone.</summary>
    /// <remarks>The rule stays whole on the server.</remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <returns>The rule, which takes its options.</returns>
    IRuleBuilderOptions<T, TProperty> UnlessOnServer(Expression<Func<T, bool>> predicate);

    /// <summary>Keeps the rule on the server: the browser checks nothing of it.</summary>
    /// <returns>The rule, which takes its options.</returns>
    IRuleBuilderOptions<T, TProperty> ServerOnly();
}

/// <summary>
/// What follows a block of conditional rules: the rules of the other case.
/// </summary>
public interface IConditionBuilder
{
    /// <summary>Declares the rules of the other case: the ones that apply where the block's own do not.</summary>
    /// <param name="rules">Declares the rules.</param>
    void Otherwise(Action rules);
}
