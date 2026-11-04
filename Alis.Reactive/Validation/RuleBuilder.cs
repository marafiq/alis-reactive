using System;
using System.Linq.Expressions;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// The rules of one member, from <c>RuleFor(x =&gt; x.Member)</c>: its checks follow as
    /// <c>.NotEmpty()</c>, <c>.MaximumLength(40)</c>, <c>.Matches(..)</c>, written in FluentValidation's grammar.
    /// An app adds a check of its own as an extension method that composes these, so the browser runs it too:
    /// <c>public static IRuleBuilderOptions&lt;T, string?&gt; WristbandCode&lt;T&gt;(this IRuleBuilder&lt;T, string?&gt; rule) =&gt; rule.Matches(..).WithMessage(..);</c>
    /// </summary>
    /// <typeparam name="T">The model the validator validates.</typeparam>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    public interface IRuleBuilder<T, out TProperty>
    {
        /// <summary>Validates the member's value with its own validator; its errors are named under the member.</summary>
        /// <param name="validator">The validator of the member's type.</param>
        IRuleBuilderOptions<T, TProperty> SetValidator(IValidator<TProperty> validator);

        /// <summary>
        /// What the server does after a check of this rule fails: <see cref="CascadeMode.Continue"/> runs the rest
        /// and answers with every failing check, <see cref="CascadeMode.Stop"/> answers with the first alone, as
        /// the browser shows one message per input. It overrides the validator's
        /// <c>RuleLevelCascadeMode</c> for this rule.
        /// </summary>
        /// <param name="mode">The mode.</param>
        IRuleBuilder<T, TProperty> Cascade(CascadeMode mode);
    }

    /// <summary>What the server does after a check of a rule fails.</summary>
    public enum CascadeMode
    {
        /// <summary>Runs the rule's remaining checks and answers with every failing one, FluentValidation's default.</summary>
        Continue,

        /// <summary>Answers with the first failing check alone, as the browser shows one message per input.</summary>
        Stop,
    }

    /// <summary>A member's rules after a check: the check's message, its conditions, and where it runs.</summary>
    /// <typeparam name="T">The model the validator validates.</typeparam>
    /// <typeparam name="TProperty">The member's type.</typeparam>
    public interface IRuleBuilderOptions<T, out TProperty> : IRuleBuilder<T, TProperty>
    {
        /// <summary>The message of the check just written, in place of its default.</summary>
        /// <param name="message">The text; it may name <c>{PropertyName}</c> and the check's own values.</param>
        IRuleBuilderOptions<T, TProperty> WithMessage(string message);

        /// <summary>
        /// A message the server builds from the model. Only the server can say it, so the check it names runs
        /// on the server alone.
        /// </summary>
        /// <param name="message">Builds the text from the model.</param>
        IRuleBuilderOptions<T, TProperty> WithMessage(Func<T, string> message);

        /// <summary>
        /// The name the rule's messages say for the member, in place of the one its spelling gives
        /// (<c>AdmissionDate</c> reads "Admission Date"): every check of the rule, in the browser and on the
        /// server. Errors stay under the member's path.
        /// </summary>
        /// <param name="displayName">The name people read.</param>
        IRuleBuilderOptions<T, TProperty> WithName(string displayName);

        /// <summary>
        /// Rules that apply only while every check of this rule passes, on every item of a list, or does not apply,
        /// written inside as under <c>When</c>. The browser skips them while this rule fails, when every check of it
        /// has a browser form; otherwise they are the server's. A rule that waits (<c>MustAsync</c>) takes none.
        /// </summary>
        /// <param name="rules">Declares the dependent rules.</param>
        IRuleBuilderOptions<T, TProperty> DependentRules(Action rules);

        /// <summary>
        /// Runs the checks written so far only when the condition holds. A condition that tests model members
        /// against fixed values runs in the browser too; one the browser cannot read is refused when a page
        /// describes the rules: declare it with <see cref="WhenOnServer"/>.
        /// </summary>
        /// <param name="predicate">The condition.</param>
        IRuleBuilderOptions<T, TProperty> When(Expression<Func<T, bool>> predicate);

        /// <summary>Runs the checks written so far only when the condition does not hold.</summary>
        /// <param name="predicate">The condition.</param>
        IRuleBuilderOptions<T, TProperty> Unless(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Runs the checks written so far only when the condition holds, decided by the server alone: for a test
        /// the browser cannot make (a method, a service, a query over a collection). The checks stay with the
        /// server too.
        /// </summary>
        /// <param name="predicate">The condition.</param>
        IRuleBuilderOptions<T, TProperty> WhenOnServer(Expression<Func<T, bool>> predicate);

        /// <summary>Runs the checks written so far only when the condition does not hold, decided by the server alone.</summary>
        /// <param name="predicate">The condition.</param>
        IRuleBuilderOptions<T, TProperty> UnlessOnServer(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Runs the member's rules on the server alone: for a member the page has no input for, or a check the
        /// page must not run.
        /// </summary>
        IRuleBuilderOptions<T, TProperty> ServerOnly();
    }

    /// <summary>The rules for when a block's condition does not hold.</summary>
    public interface IConditionBuilder
    {
        /// <summary>Declares the rules that apply when the condition does not hold.</summary>
        /// <param name="rules">Declares the rules.</param>
        void Otherwise(Action rules);
    }

    internal interface IRuleTarget<T, TValue>
    {
        IRuleBuilderOptions<T, TValue> Add(Check<T, TValue> check);

        IRuleBuilderOptions<T, TValue> Add(RulePart<T, TValue> part);

        RuleSets Sets { get; }
    }

    internal static class RuleTarget
    {
        internal static IRuleTarget<T, TValue> Of<T, TValue>(IRuleBuilder<T, TValue> rule)
            => rule as IRuleTarget<T, TValue>
            ?? throw new ArgumentException("Write a check on a rule a ReactiveValidator declares: RuleFor(x => x.Member).", nameof(rule));
    }
}
