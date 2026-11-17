using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.World;

internal static class WorldLines
{
    private const string Negotiate = "/negotiate";

    internal static void UseWorldLines(this WebApplication app)
        => app.Use(async (context, next) =>
        {
            if (LineOf(context.Request.Path) is not { } line)
            {
                await next(context);
                return;
            }

            var stream = context.RequestServices.GetRequiredService<CurrentWorld>().Data.Streams.Of(line);
            stream.Records(context.Request.Headers.TraceParent.ToString());
            var negotiates = context.Request.Path.Value is { } path && path.EndsWith(Negotiate, StringComparison.Ordinal);
            if (negotiates && await stream.Admits(context.RequestAborted) is null)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                return;
            }

            await next(context);
        });

    private static string? LineOf(PathString path)
    {
        string[] lines = [CareEndpoints.WardLine, CareEndpoints.RoundFeed, CareEndpoints.BedLine, CareEndpoints.Line, CareEndpoints.NurseLine];
        return lines.FirstOrDefault(line => path.StartsWithSegments(line, StringComparison.Ordinal));
    }
}
