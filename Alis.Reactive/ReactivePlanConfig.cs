using System;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// One-time startup configuration for the reactive framework.
    /// </summary>
    /// <remarks>
    /// Call <see cref="UseClientValidationRuleSource"/> in <c>Program.cs</c> or <c>Startup.cs</c>
    /// to enable client-side validation rules from validators, direct rules, or generated model metadata.
    /// Without this call, views that use <c>Validate&lt;TValidationSource&gt;()</c> will throw at render time.
    /// </remarks>
    public static class ReactivePlanConfig
    {
        private static ClientValidationRuleSourceRegistration _ruleSource =
            ClientValidationRuleSourceRegistration.Missing;

        internal static ClientValidationRuleSourceRegistration ClientValidationRuleSource => _ruleSource;

        /// <summary>
        /// Registers the source that provides deterministic browser validation rules.
        /// </summary>
        /// <remarks>
        /// Must be called exactly once at app startup. Calling it a second time throws
        /// to prevent accidental double-registration that would silently replace the rule source.
        /// </remarks>
        /// <param name="source">The rule source implementation, such as direct model rules or the FluentValidation adapter.</param>
        /// <exception cref="InvalidOperationException">Thrown if a rule source is already registered.</exception>
        public static void UseClientValidationRuleSource(IClientValidationRuleSource source)
        {
            _ruleSource = _ruleSource.Register(source);
        }

        /// <summary>Test-only: resets static state so UseClientValidationRuleSource can be called again.</summary>
        internal static void Reset() => _ruleSource = ClientValidationRuleSourceRegistration.Missing;
    }

    internal abstract class ClientValidationRuleSourceRegistration
    {
        internal static ClientValidationRuleSourceRegistration Missing { get; } =
            new MissingClientValidationRuleSourceRegistration();

        internal abstract ClientValidationRuleSourceRegistration Register(IClientValidationRuleSource source);

        internal abstract IClientValidationRuleSource RequireFor(ValidationJob job);
    }

    internal sealed class MissingClientValidationRuleSourceRegistration : ClientValidationRuleSourceRegistration
    {
        internal override ClientValidationRuleSourceRegistration Register(IClientValidationRuleSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new RegisteredClientValidationRuleSource(source);
        }

        internal override IClientValidationRuleSource RequireFor(ValidationJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            throw new InvalidOperationException(
                $"Request at '{job.RequestUrl}' specifies validation source '{job.ValidationSourceType.Name}' " +
                "but no IClientValidationRuleSource is registered. " +
                "Call ReactivePlanConfig.UseClientValidationRuleSource() at app startup.");
        }
    }

    internal sealed class RegisteredClientValidationRuleSource : ClientValidationRuleSourceRegistration
    {
        private readonly IClientValidationRuleSource _source;

        internal RegisteredClientValidationRuleSource(IClientValidationRuleSource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        internal override ClientValidationRuleSourceRegistration Register(IClientValidationRuleSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            throw new InvalidOperationException(
                "Client validation rule source is already registered. " +
                "UseClientValidationRuleSource must be called exactly once at app startup.");
        }

        internal override IClientValidationRuleSource RequireFor(ValidationJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            return _source;
        }
    }
}
