using System;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.ModuleTests;

internal static class Views
{
    internal static IHtmlHelper<TModel> Of<TModel>(TModel model) => Of(model, _ => { });

    internal static IHtmlHelper<TModel> Of<TModel>(TModel model, Action<JsonSerializerOptions> appJson) => Of(model, appJson, _ => { });

    internal static IHtmlHelper<TModel> Of<TModel>(TModel model, Action<JsonSerializerOptions> appJson, Action<IServiceCollection> app)
    {
        var services = MvcOfAnApp(appJson, app).CreateScope().ServiceProvider;
        var html = services.GetRequiredService<IHtmlHelper<TModel>>();
        var context = new DefaultHttpContext { RequestServices = services };
        var view = new ViewContext
        {
            HttpContext = context,
            RouteData = new RouteData(),
            FormContext = new FormContext(),
            ActionDescriptor = new ActionDescriptor(),
            TempData = services.GetRequiredService<ITempDataDictionaryFactory>().GetTempData(context),
            ViewData = new ViewDataDictionary<TModel>(services.GetRequiredService<IModelMetadataProvider>(), new ModelStateDictionary()) { Model = model },
            Writer = new StringWriter(),
        };
        ((IViewContextAware)html).Contextualize(view);
        return html;
    }

    internal static string TextOf(IHtmlContent rendered)
    {
        using var written = new StringWriter();
        rendered.WriteTo(written, HtmlEncoder.Default);
        return written.ToString();
    }

    private static ServiceProvider MvcOfAnApp(Action<JsonSerializerOptions> appJson, Action<IServiceCollection> app)
    {
        var services = WebApplication.CreateEmptyBuilder(new WebApplicationOptions { ApplicationName = typeof(Views).Assembly.GetName().Name }).Services;
        var diagnostics = new DiagnosticListener("Alis.Reactive.ModuleTests");
        services.AddLogging();
        services.AddSingleton(diagnostics);
        services.AddSingleton<DiagnosticSource>(diagnostics);
        services.AddControllersWithViews().AddApplicationPart(typeof(Views).Assembly).AddJsonOptions(json => appJson(json.JsonSerializerOptions));
        services.Configure<RazorViewEngineOptions>(options => options.ViewLocationFormats.Insert(0, "/InputsAndIds/{0}.cshtml"));
        app(services);
        return services.BuildServiceProvider();
    }
}
