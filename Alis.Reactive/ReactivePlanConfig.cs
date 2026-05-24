using System;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    /// <summary>
    /// One-time startup configuration for the reactive framework.
    /// </summary>
    /// <remarks>
    /// Call <see cref="UseClientValidationProjectionSource"/> in <c>Program.cs</c> or <c>Startup.cs</c>
    /// to enable client-side validation projection from validators or generated model metadata.
    /// Without this call, views that use <c>Validate&lt;TValidator&gt;()</c> will throw at render time.
    /// </remarks>
    public static class ReactivePlanConfig
    {
        private static ClientValidationProjectionSourceRegistration _projectionSource =
            ClientValidationProjectionSourceRegistration.Missing;

        internal static ClientValidationProjectionSourceRegistration ClientValidationProjectionSource => _projectionSource;

        /// <summary>
        /// Registers the source that projects deterministic browser validation rules.
        /// </summary>
        /// <remarks>
        /// Must be called exactly once at app startup. Calling it a second time throws
        /// to prevent accidental double-registration that would silently replace the projection source.
        /// </remarks>
        /// <param name="source">The projection source implementation (typically from <c>Alis.Reactive.FluentValidator</c>).</param>
        /// <exception cref="InvalidOperationException">Thrown if a projection source is already registered.</exception>
        public static void UseClientValidationProjectionSource(IClientValidationProjectionSource source)
        {
            _projectionSource = _projectionSource.Register(source);
        }

        /// <summary>Test-only: resets static state so UseClientValidationProjectionSource can be called again.</summary>
        internal static void Reset() => _projectionSource = ClientValidationProjectionSourceRegistration.Missing;
    }

    internal abstract class ClientValidationProjectionSourceRegistration
    {
        internal static ClientValidationProjectionSourceRegistration Missing { get; } =
            new MissingClientValidationProjectionSourceRegistration();

        internal abstract ClientValidationProjectionSourceRegistration Register(IClientValidationProjectionSource source);

        internal abstract IClientValidationProjectionSource RequireFor(ValidationJob job);
    }

    internal sealed class MissingClientValidationProjectionSourceRegistration : ClientValidationProjectionSourceRegistration
    {
        internal override ClientValidationProjectionSourceRegistration Register(IClientValidationProjectionSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new RegisteredClientValidationProjectionSource(source);
        }

        internal override IClientValidationProjectionSource RequireFor(ValidationJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            throw new InvalidOperationException(
                $"Request at '{job.RequestUrl}' specifies validator '{job.ValidatorType.Name}' " +
                "but no IClientValidationProjectionSource is registered. " +
                "Call ReactivePlanConfig.UseClientValidationProjectionSource() at app startup.");
        }
    }

    internal sealed class RegisteredClientValidationProjectionSource : ClientValidationProjectionSourceRegistration
    {
        private readonly IClientValidationProjectionSource _source;

        internal RegisteredClientValidationProjectionSource(IClientValidationProjectionSource source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        internal override ClientValidationProjectionSourceRegistration Register(IClientValidationProjectionSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            throw new InvalidOperationException(
                "Client validation projection source is already registered. " +
                "UseClientValidationProjectionSource must be called exactly once at app startup.");
        }

        internal override IClientValidationProjectionSource RequireFor(ValidationJob job)
        {
            if (job == null) throw new ArgumentNullException(nameof(job));
            return _source;
        }
    }
}
