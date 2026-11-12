#if NET48
using System;
using System.Web.Mvc;

namespace Alis.Reactive.RenderingAndDelivery;

/// <summary>
/// Validates the antiforgery token a request carries in the <c>RequestVerificationToken</c> header, else in the form
/// field, on MVC 5, whose own filter reads the form alone.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class ValidateAntiForgeryHeaderAttribute : FilterAttribute, IAuthorizationFilter
{
    /// <summary>Validates the request's token against the antiforgery cookie, and refuses the request when it does not match.</summary>
    /// <param name="filterContext">The request being authorized.</param>
    public void OnAuthorization(AuthorizationContext filterContext) => throw new NotImplementedException();
}
#endif
