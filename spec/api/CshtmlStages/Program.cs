using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Alis.Reactive.Elements;
using Alis.Reactive.Hosting;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Requests;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddReactiveRequests(requests => requests
    .WhileLoading(pipeline =>
    {
        pipeline.Element("app-status").Text.Set("Working");
        pipeline.Element("app").Classes.Add("busy");
    })
    .Finally(pipeline =>
    {
        pipeline.Element("app").Classes.Remove("busy");
        pipeline.Element("app-status").Text.Set("Ready");
    })
    .OnError(401, Content.None, (expired, pipeline) =>
    {
        pipeline.Notice().Warning(expired.Headers.Get("WWW-Authenticate"));
        pipeline.Element("sign-in").Classes.Add("shown");
    }));
var app = builder.Build();
app.MapDefaultControllerRoute();
app.Run();
