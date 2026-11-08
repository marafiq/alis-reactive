using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Alis.Reactive.Elements;
using Alis.Reactive.Hosting;
using Alis.Reactive.Pipelines;
using Alis.Reactive.Requests;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddReactiveRequests(requests => requests
    .WhileLoading(loading =>
    {
        loading.Element("app-status").Text.Set("Working");
        loading.Element("app").Classes.Add("busy");
    })
    .Finally(done =>
    {
        done.Element("app").Classes.Remove("busy");
        done.Element("app-status").Text.Set("Ready");
    })
    .OnError(401, Content.None, (expired, shown) =>
    {
        shown.Notice().Warning(expired.Headers.Get("WWW-Authenticate"));
        shown.Element("sign-in").Classes.Add("shown");
    }));
var app = builder.Build();
app.MapDefaultControllerRoute();
app.Run();
