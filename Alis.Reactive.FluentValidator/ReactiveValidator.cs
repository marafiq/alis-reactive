using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FluentValidation;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// A FluentValidation validator whose rules also run in the browser. Rules are written once, in
    /// FluentValidation's own grammar; every rule the browser can evaluate reaches it, and the rest stay
    /// on the server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The browser evaluates <c>NotEmpty</c>, <c>Empty</c>, <c>MinimumLength</c>, <c>MaximumLength</c>,
    /// <c>Length</c>, <c>EmailAddress</c>, <c>Matches</c>, <c>CreditCard</c>, the comparisons against a
    /// value or a sibling member, <c>InclusiveBetween</c>, <c>ExclusiveBetween</c> and the framework's
    /// <see cref="ReactiveRuleBuilderExtensions.Url{T}"/>; <c>SetValidator</c>, <c>RuleForEach</c> with a
    /// child validator and <c>Include</c> bring the child's rules along. <c>Must</c>, <c>Custom</c>,
    /// asynchronous rules, rule sets and dependent rules stay on the server.
    /// </para>
    /// <para>
    /// A <c>When</c> or <c>Unless</c> written as a lambda over the model is recorded, so the browser applies
    /// the same condition: comparisons of members with values, <c>string.IsNullOrEmpty</c>,
    /// <c>Contains</c>, <c>StartsWith</c>, <c>EndsWith</c>, <c>Regex.IsMatch</c>, membership in a fixed set,
    /// joined by <c>&amp;&amp;</c>, <c>||</c> and <c>!</c>. Rules under any other condition, or under a
    /// condition written as a method group, stay on the server.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// public sealed class AdmissionValidator : ReactiveValidator&lt;Admission&gt;
    /// {
    ///     public AdmissionValidator()
    ///     {
    ///         RuleFor(x =&gt; x.ResidentName).NotEmpty().MaximumLength(100);
    ///         RuleFor(x =&gt; x.DischargeDate).GreaterThan(x =&gt; x.AdmissionDate);
    ///         When(x =&gt; x.IsVeteran, () =&gt; RuleFor(x =&gt; x.ServiceNumber).NotEmpty());
    ///         RuleForEach(x =&gt; x.Contacts).SetValidator(new ContactValidator());
    ///     }
    /// }
    /// </code>
    /// </example>
    public abstract class ReactiveValidator<T> : RecordingValidator<T>, IClientValidationMetadataSource
        where T : class
    {
        IReadOnlyList<ClientValidationField> IClientValidationMetadataSource.GetClientRules() =>
            ClientRuleProjection.Project(this, ScopeOf);

        /// <summary>
        /// Applies the rules <paramref name="action"/> declares only when <paramref name="predicate"/>
        /// holds, on the server and, when the condition has a browser form, in the browser.
        /// </summary>
        /// <param name="predicate">The condition over the model, written as a lambda.</param>
        /// <param name="action">Declares the conditional rules.</param>
        /// <returns>A builder whose <c>Otherwise</c> declares the rules for when the condition fails.</returns>
        public IConditionBuilder When(Expression<Func<T, bool>> predicate, Action action)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var condition = ClientCondition.From(predicate);
            return RecordedWhen(condition, predicate.Compile(), action);
        }

        /// <summary>
        /// Applies the rules <paramref name="action"/> declares only when <paramref name="predicate"/>
        /// fails, on the server and, when the condition has a browser form, in the browser.
        /// </summary>
        /// <param name="predicate">The condition over the model, written as a lambda.</param>
        /// <param name="action">Declares the conditional rules.</param>
        /// <returns>A builder whose <c>Otherwise</c> declares the rules for when the condition holds.</returns>
        public IConditionBuilder Unless(Expression<Func<T, bool>> predicate, Action action)
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            var condition = ClientCondition.From(predicate)?.Negated();
            return RecordedUnless(condition, predicate.Compile(), action);
        }
    }
}
