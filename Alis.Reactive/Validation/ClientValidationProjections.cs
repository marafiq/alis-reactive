using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Client-side validation projections keyed by their validation source type.
    /// </summary>
    public sealed class ClientValidationProjections : IClientValidationProjectionSource
    {
        private readonly IReadOnlyDictionary<Type, IReadOnlyList<ClientValidationField>> _fieldsBySource;

        private ClientValidationProjections(
            IReadOnlyDictionary<Type, IReadOnlyList<ClientValidationField>> fieldsBySource)
        {
            _fieldsBySource = fieldsBySource ?? throw new ArgumentNullException(nameof(fieldsBySource));
        }

        public static ClientValidationProjections Create(
            params ClientValidationProjectionDefinition[] projections)
        {
            if (projections == null) throw new ArgumentNullException(nameof(projections));

            var fieldsBySource = new Dictionary<Type, IReadOnlyList<ClientValidationField>>();
            foreach (var projection in projections)
            {
                if (projection == null)
                    throw new ArgumentException("Client validation projection must not be null.", nameof(projections));

                projection.AddTo(fieldsBySource);
            }

            return new ClientValidationProjections(fieldsBySource);
        }

        public static ClientValidationProjectionDefinition For<TValidationSource, TModel>(
            Action<ClientValidationProjectionBuilder<TModel>> define)
            where TModel : class =>
            ClientValidationProjectionDefinition.For<TValidationSource, TModel>(define);

        public IReadOnlyList<ClientValidationField> ProjectClientRules(Type validationSourceType)
        {
            if (validationSourceType == null) throw new ArgumentNullException(nameof(validationSourceType));

            if (_fieldsBySource.TryGetValue(validationSourceType, out var fields))
                return fields;

            throw new InvalidOperationException(
                $"No client validation projection is defined for validation source '{validationSourceType.FullName}'. " +
                "Add one with ClientValidationProjections.For<TValidationSource, TModel>(projection => ...).");
        }
    }

    public sealed class ClientValidationProjectionDefinition
    {
        private readonly Type _sourceType;
        private readonly IReadOnlyList<ClientValidationField> _fields;

        private ClientValidationProjectionDefinition(
            Type sourceType,
            IReadOnlyList<ClientValidationField> fields)
        {
            _sourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        internal static ClientValidationProjectionDefinition For<TValidationSource, TModel>(
            Action<ClientValidationProjectionBuilder<TModel>> define)
            where TModel : class
        {
            if (define == null) throw new ArgumentNullException(nameof(define));

            var projection = new ClientValidationProjectionBuilder<TModel>();
            define(projection);
            return new ClientValidationProjectionDefinition(
                typeof(TValidationSource),
                projection.ToFields());
        }

        internal void AddTo(Dictionary<Type, IReadOnlyList<ClientValidationField>> fieldsBySource)
        {
            if (fieldsBySource == null) throw new ArgumentNullException(nameof(fieldsBySource));

            if (fieldsBySource.ContainsKey(_sourceType))
            {
                throw new InvalidOperationException(
                    $"Client validation projection for '{_sourceType.FullName}' is already defined.");
            }

            fieldsBySource.Add(_sourceType, _fields);
        }
    }
}
