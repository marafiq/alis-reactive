#if NET10_0_OR_GREATER
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Alis.Reactive.Validation;

internal sealed class RegisteredValidationMetadata(ValidatorCatalog catalog) : IValidationMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        if (!catalog.Checks(context.Key.ModelType)) return;
        context.ValidationMetadata.ValidateChildren = false;
        context.ValidationMetadata.IsRequired = false;
        context.ValidationMetadata.HasValidators = false;
        context.ValidationMetadata.ValidatorMetadata.Clear();
    }
}
internal sealed class RegisteredModelValidators(ValidatorCatalog catalog, IReadOnlyList<IModelValidatorProvider> providers) : IModelValidatorProvider
{
    public void CreateValidators(ModelValidatorProviderContext context)
    {
        if (catalog.Checks(context.ModelMetadata.ModelType))
        {
            context.Results.Clear();
            return;
        }
        foreach (var provider in providers) provider.CreateValidators(context);
    }
}
#endif
