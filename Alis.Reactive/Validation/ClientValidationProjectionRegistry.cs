using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Core-owned source for deterministic browser validation projections.
    /// </summary>
    public sealed class ClientValidationProjectionRegistry : IClientValidationProjectionSource
    {
        private readonly IReadOnlyDictionary<Type, ClientValidationProjectionDefinition> _definitions;

        private ClientValidationProjectionRegistry(
            IReadOnlyDictionary<Type, ClientValidationProjectionDefinition> definitions)
        {
            _definitions = definitions ?? throw new ArgumentNullException(nameof(definitions));
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

            if (!_definitions.TryGetValue(request.ValidatorType, out var definition))
            {
                throw new InvalidOperationException(
                    $"No client validation projection is registered for '{request.ValidatorType.FullName}'. " +
                    "Register it inside ClientValidationProjectionRegistry.Create(registry => registry.For<TValidationSource, TModel>(...)).");
            }

            return definition.Project(request);
        }
    }
}
