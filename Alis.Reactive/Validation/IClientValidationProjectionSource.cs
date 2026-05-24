using System;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Projects the deterministic browser validation rules for a validation source type.
    /// </summary>
    public interface IClientValidationProjectionSource
    {
        ClientValidationProjection Project(ClientValidationProjectionRequest request);
    }
}
