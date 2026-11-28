#if NET10_0_OR_GREATER
using System.Linq;
using Alis.Reactive.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.RenderingAndDelivery;

public static partial class ValidationServices
{
    static partial void ConfigureHost(IServiceCollection services)
    {
        services.AddOptions<MvcOptions>().PostConfigure<ValidatorCatalog>((options, catalog) =>
        {
            options.ModelMetadataDetailsProviders.Add(new RegisteredValidationMetadata(catalog));
            var providers = options.ModelValidatorProviders.ToArray();
            options.ModelValidatorProviders.Clear();
            options.ModelValidatorProviders.Add(new RegisteredModelValidators(catalog, providers));
        });
    }
}
#endif
