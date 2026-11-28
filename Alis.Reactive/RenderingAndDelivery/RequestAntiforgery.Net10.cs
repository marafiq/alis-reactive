#if NET10_0_OR_GREATER
using System;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.TriggersAndPipelines;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.RenderingAndDelivery;

internal static class RequestAntiforgery
{
    internal static PageAntiforgery Of(HttpContext context)
        => new(context.RequestServices.GetService<RequestDefaults>() is { } defaults ? defaults.AntiforgeryChoice : AntiforgeryChoice.NoRequestsCall,
            context.Request.Scheme + "://" + context.Request.Host.ToUriComponent(),
            () => context.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context).RequestToken
                ?? throw new InvalidOperationException("The host's antiforgery wrote no request token for this view; register MVC's antiforgery, as AddControllersWithViews does."));
}
#endif
