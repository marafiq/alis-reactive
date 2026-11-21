using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Alis.Reactive.Conformance.App.World;

internal sealed class AtTheRespondGateAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        await http.RequestServices.GetRequiredService<CurrentWorld>().Data.Reach(GateName.Of(TracingGate.Respond), http.Request.Path);
        await next();
    }
}
