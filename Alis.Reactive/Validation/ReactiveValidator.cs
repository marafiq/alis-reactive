using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    internal abstract Type ModelType { get; }

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
    internal sealed override Type ModelType => typeof(T);
    private List<ValidationRule<T>> _rules = [];
    private IReadOnlyList<string> _sets = [];

    /// <summary>Makes the validator; a derived class writes its rules in its constructor.</summary>
    [SuppressMessage("ApiDesign", "RS0022:Constructor make noninheritable base class inheritable", Justification = "An app derives from the typed validator; the untyped base stays closed and holds no protected member.")]
    protected ReactiveValidator()
    {
    }

    /// <summary>Gets or sets the cascade mode of every rule of the validator that names none of its own.</summary>
    protected CascadeMode RuleLevelCascadeMode
    { get; set; } = CascadeMode.Continue;

    /// <summary>Starts the rule of one member of the model.</summary>
    /// <param name="member">The member, as a member path.</param>
    /// <returns>The rule, which takes its checks.</returns>
    public IRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> member)
    {
        var named = new ModelMember<T, TProperty>(member);
        var rule = new MemberRule<T, TProperty>(this, named.Name, model => [named.Read(model)], false, _sets, () => RuleLevelCascadeMode);
        _rules.Add(rule);
        return rule;
    }

    /// <summary>Starts the rule of each item of a list the model holds.</summary>
    /// <param name="collection">The list, as a member path.</param>
    /// <returns>The rule, which takes the checks of an item.</returns>
    public IRuleBuilder<T, TElement> RuleForEach<TElement>(Expression<Func<T, IEnumerable<TElement>>> collection)
    {
        var named = new ModelMember<T, IEnumerable<TElement>>(collection);
        var rule = new MemberRule<T, TElement>(this, named.Name, model => named.Read(model) ?? Enumerable.Empty<TElement>(), true, _sets, () => RuleLevelCascadeMode);
        _rules.Add(rule);
        return rule;
    }

    /// <summary>Adds the rules of another validator of the same model to this validator's.</summary>
    /// <param name="rules">The validator whose rules are added.</param>
    public void Include(IValidator<T> rules) => _rules.Add(new IncludeRule<T>(rules, _sets));

    /// <summary>Declares the rules written inside as members of a named rule set.</summary>
    /// <remarks>
    /// A check runs one validator under one rule set: a request is checked under the default rule set unless it
    /// names another.
    /// </remarks>
    /// <param name="ruleSet">The rule set's name.</param>
    /// <param name="rules">Declares the rules of the set.</param>
    public void RuleSet(string ruleSet, Action rules)
    {
        if (ruleSet.Length == 0) throw new ArgumentException("The value cannot be an empty string", "ruleSetName");
        var outer = _sets;
        _sets = RuleSets.Named(ruleSet);
        try { rules(); }
        finally { _sets = outer; }
    }

    /// <summary>Applies the rules written inside only when the condition holds.</summary>
    /// <remarks>
    /// The server and the browser each decide the condition. One the browser cannot decide is refused where the
    /// view renders: declare it the server's with <see cref="WhenOnServer"/>.
    /// </remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder When(Expression<Func<T, bool>> predicate, Action rules) => Branch(RuleCondition<T>.Of(predicate, true), false, rules);

    /// <summary>Applies the rules written inside only when the condition does not hold.</summary>
    /// <remarks>
    /// The server and the browser each decide the condition. One the browser cannot decide is refused where the
    /// view renders: declare it the server's with <see cref="UnlessOnServer"/>.
    /// </remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder Unless(Expression<Func<T, bool>> predicate, Action rules) => Branch(RuleCondition<T>.Of(predicate, true), true, rules);

    /// <summary>Applies the rules written inside only when the condition holds, decided by the server alone.</summary>
    /// <remarks>The rules under it stay whole on the server.</remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder WhenOnServer(Expression<Func<T, bool>> predicate, Action rules) => Branch(RuleCondition<T>.Of(predicate, true), false, rules);

    /// <summary>Applies the rules written inside only when the condition does not hold, decided by the server alone.</summary>
    /// <remarks>The rules under it stay whole on the server.</remarks>
    /// <param name="predicate">The condition over the model.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder UnlessOnServer(Expression<Func<T, bool>> predicate, Action rules) => Branch(RuleCondition<T>.Of(predicate, true), true, rules);

    /// <summary>Applies the rules written inside only when an awaited condition holds, decided by the server alone.</summary>
    /// <param name="predicate">The condition over the model, answered by a task.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder WhenAsync(Func<T, CancellationToken, Task<bool>> predicate, Action rules) => Branch(RuleCondition<T>.Awaited(predicate), false, rules);

    /// <summary>Applies the rules written inside only when an awaited condition does not hold, decided by the server alone.</summary>
    /// <param name="predicate">The condition over the model, answered by a task.</param>
    /// <param name="rules">Declares the rules.</param>
    /// <returns>The block, whose <c>Otherwise</c> declares the rules of the other case.</returns>
    public IConditionBuilder UnlessAsync(Func<T, CancellationToken, Task<bool>> predicate, Action rules) => Branch(RuleCondition<T>.Awaited(predicate), true, rules);

    /// <inheritdoc />
    public ValidationResult Validate(T instance) => Validate(instance, "default");

    /// <inheritdoc />
    public ValidationResult Validate(T instance, string ruleSet) => CheckBound(instance, ruleSet, [], []);

    internal ValidationResult CheckBound(T instance, string ruleSet, IReadOnlyList<ValidationFailure> binding, IReadOnlyCollection<Type> flags)
    {
        if (instance is null) throw new ArgumentNullException(nameof(instance));
        var run = new ValidationRun(true, CancellationToken.None, ruleSet, binding, flags);
        RunRules(new ValidationContext<T>(instance, "", run)).GetAwaiter().GetResult();
        return new ValidationResult(run.Failures.ToArray());
    }

    /// <inheritdoc />
    public Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default) => ValidateAsync(instance, "default", cancellation);

    /// <inheritdoc />
    public Task<ValidationResult> ValidateAsync(T instance, string ruleSet, CancellationToken cancellation = default)
        => CheckBoundAsync(instance, ruleSet, [], [], cancellation);

    internal async Task<ValidationResult> CheckBoundAsync(T instance, string ruleSet, IReadOnlyList<ValidationFailure> binding, IReadOnlyCollection<Type> flags, CancellationToken cancellation)
    {
        if (instance is null) throw new ArgumentNullException(nameof(instance));
        var run = new ValidationRun(false, cancellation, ruleSet, binding, flags);
        await RunRules(new ValidationContext<T>(instance, "", run)).ConfigureAwait(false);
        return new ValidationResult(run.Failures.ToArray());
    }

    internal async Task RunRules(ValidationContext<T> context)
    {
        foreach (var rule in _rules) await rule.Validate(context).ConfigureAwait(false);
    }

    internal void TakeHolderSets(IReadOnlyList<string> sets) => _sets = sets;

    internal List<ValidationRule<T>> Capture(Action rules, IReadOnlyList<string> sets)
    {
        var outer = _rules;
        var outerSets = _sets;
        _rules = [];
        _sets = sets;
        try { rules(); return _rules; }
        finally { _rules = outer; _sets = outerSets; }
    }

    private IConditionBuilder Branch(RuleCondition<T> condition, bool negated, Action rules)
    {
        DeclareUnder(new RuleGuard<T>(condition, negated), rules);
        return new Otherwise(this, new RuleGuard<T>(condition, !negated), _sets);
    }

    private void DeclareUnder(RuleGuard<T> guard, Action rules)
    {
        var captured = Capture(rules, _sets);
        foreach (var rule in captured) rule.Guards.Insert(0, guard);
        _rules.AddRange(captured);
    }

    private sealed class Otherwise(ReactiveValidator<T> owner, RuleGuard<T> guard, IReadOnlyList<string> sets) : IConditionBuilder
    {
        void IConditionBuilder.Otherwise(Action rules)
        {
            var captured = owner.Capture(rules, sets);
            foreach (var rule in captured) rule.Guards.Insert(0, guard);
            owner._rules.AddRange(captured);
        }
    }
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
