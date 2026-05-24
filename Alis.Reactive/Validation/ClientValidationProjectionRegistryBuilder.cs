using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Registers validation source types with their deterministic browser projection.
    /// </summary>
    public sealed class ClientValidationProjectionRegistryBuilder
    {
        private readonly Dictionary<Type, ClientValidationProjectionDefinition> _definitions =
            new Dictionary<Type, ClientValidationProjectionDefinition>();

        internal ClientValidationProjectionRegistryBuilder() { }

        public ClientValidationProjectionRegistryBuilder For<TValidationSource, TModel>(
            Action<ClientValidationProjectionBuilder<TModel>> define)
            where TModel : class
        {
            if (define == null) throw new ArgumentNullException(nameof(define));

            var sourceType = typeof(TValidationSource);
            if (_definitions.ContainsKey(sourceType))
            {
                throw new InvalidOperationException(
                    $"Client validation projection for '{sourceType.FullName}' is already registered.");
            }

            var projection = new ClientValidationProjectionBuilder<TModel>();
            define(projection);
            _definitions.Add(sourceType, projection.ToDefinition());
            return this;
        }

        internal IReadOnlyDictionary<Type, ClientValidationProjectionDefinition> Build() =>
            new Dictionary<Type, ClientValidationProjectionDefinition>(_definitions);
    }
}
