using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Core-owned source for deterministic browser validation projections.
    /// </summary>
    public sealed class ClientValidationProjectionRegistry : IClientValidationProjectionSource
    {
        private readonly IReadOnlyDictionary<Type, IReadOnlyList<ClientValidationField>> _fieldsByValidationSource;

        private ClientValidationProjectionRegistry(
            IReadOnlyDictionary<Type, IReadOnlyList<ClientValidationField>> fieldsByValidationSource)
        {
            _fieldsByValidationSource = fieldsByValidationSource ?? throw new ArgumentNullException(nameof(fieldsByValidationSource));
        }

        public static ClientValidationProjectionRegistry Create(
            Action<ClientValidationProjectionRegistryBuilder> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var builder = new ClientValidationProjectionRegistryBuilder();
            configure(builder);
            return new ClientValidationProjectionRegistry(builder.Build());
        }

        public ClientValidationProjection Project(ClientValidationProjectionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            if (!_fieldsByValidationSource.TryGetValue(request.ValidationSourceType, out var fields))
            {
                throw new InvalidOperationException(
                    $"No client validation projection is registered for validation source '{request.ValidationSourceType.FullName}'. " +
                    "Register it inside ClientValidationProjectionRegistry.Create(registry => registry.For<TValidationSource, TModel>(...)).");
            }

            return ClientValidationProjection.ForFields(request, fields);
        }
    }
}
