using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    internal abstract class ClientValidationProjectionDefinition
    {
        internal abstract ClientValidationProjection Project(ClientValidationProjectionRequest request);
    }

    internal sealed class ClientValidationProjectionDefinition<TModel> : ClientValidationProjectionDefinition
        where TModel : class
    {
        private readonly IReadOnlyList<ClientValidationField> _fields;

        internal ClientValidationProjectionDefinition(IReadOnlyList<ClientValidationField> fields)
        {
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        internal override ClientValidationProjection Project(ClientValidationProjectionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return new ClientValidationProjection(
                request.ValidationContainer,
                _fields,
                Array.Empty<SkippedClientRuleProjection>());
        }
    }
}
