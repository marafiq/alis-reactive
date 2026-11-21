#if NET48
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Alis.Reactive.OwnershipAndState;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.RenderingAndDelivery;

internal static class RequestAntiforgery
{
    internal static PageAntiforgery Of(HttpContextBase context)
        => new(DependencyResolver.Current.GetService<RequestDefaults>() is { } defaults ? defaults.AntiforgeryChoice : AntiforgeryChoice.NoRequestsCall,
            context.Request.Url.GetLeftPart(System.UriPartial.Authority),
            () => Token(context));

    private static string Token(HttpContextBase context)
    {
        AntiForgery.GetTokens(context.Request.Cookies[AntiForgeryConfig.CookieName]?.Value, out var newCookie, out var formToken);
        if (newCookie is not null)
            context.Response.Cookies.Set(new HttpCookie(AntiForgeryConfig.CookieName, newCookie) { HttpOnly = true, Secure = AntiForgeryConfig.RequireSsl });
        return formToken;
    }
}
#endif
