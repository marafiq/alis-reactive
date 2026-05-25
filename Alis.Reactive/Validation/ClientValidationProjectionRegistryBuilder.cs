using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Registers validation source types with their deterministic browser projection.
    /// </summary>
    public sealed class ClientValidationProjectionRegistryBuilder
    {
        private readonly Dictionary<Type, IReadOnlyList<ClientValidationField>> _fieldsByValidationSource =
            new Dictionary<Type, IReadOnlyList<ClientValidationField>>();

        internal ClientValidationProjectionRegistryBuilder() { }

        public ClientValidationProjectionRegistryBuilder For<TValidationSource, TModel>(
            Action<ClientValidationProjectionBuilder<TModel>> define)
            where TModel : class
        {
            if (define == null) throw new ArgumentNullException(nameof(define));

            var sourceType = typeof(TValidationSource);
            if (_fieldsByValidationSource.ContainsKey(sourceType))
            {
                throw new InvalidOperationException(
                    $"Client validation projection for '{sourceType.FullName}' is already registered.");
            }

            var projection = new ClientValidationProjectionBuilder<TModel>();
            define(projection);
            _fieldsByValidationSource.Add(sourceType, projection.ToFields());
            return this;
        }

        internal IReadOnlyDictionary<Type, IReadOnlyList<ClientValidationField>> Build() =>
            new Dictionary<Type, IReadOnlyList<ClientValidationField>>(_fieldsByValidationSource);
    }
}
