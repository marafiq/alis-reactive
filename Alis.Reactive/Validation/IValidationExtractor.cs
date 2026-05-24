using System;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Extracts validation ownership from a server validator type.
    /// Implemented by FluentValidationAdapter.
    /// </summary>
    public interface IValidationExtractor
    {
        ValidationExtractionReport Extract(ValidationExtractionRequest request);
    }
}
