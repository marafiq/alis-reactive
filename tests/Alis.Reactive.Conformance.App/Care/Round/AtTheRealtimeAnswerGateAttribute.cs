using Alis.Reactive.Conformance.App.World;
using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Alis.Reactive.Conformance.App.Care;

internal sealed class AtTheRealtimeAnswerGateAttribute : ActionFilterAttribute
{
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;
        await http.RequestServices.GetRequiredService<CurrentWorld>().Data.Reach(GateName.Of(RealtimeGate.Answer), http.Request.Path);
        await next();
    }
}
