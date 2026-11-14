using System;
using System.Diagnostics;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.ModuleTests;

internal static class Views
{
    internal static IHtmlHelper<TModel> Of<TModel>(TModel model) => Of(model, _ => { });

    internal static IHtmlHelper<TModel> Of<TModel>(TModel model, Action<JsonSerializerOptions> appJson)
    {
        var services = MvcOfAnApp(appJson).CreateScope().ServiceProvider;
        var html = services.GetRequiredService<IHtmlHelper<TModel>>();
        var view = new ViewContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = services },
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

    private static ServiceProvider MvcOfAnApp(Action<JsonSerializerOptions> appJson)
    {
        var services = new ServiceCollection();
        var diagnostics = new DiagnosticListener("Alis.Reactive.ModuleTests");
        services.AddLogging();
        services.AddSingleton(diagnostics);
        services.AddSingleton<DiagnosticSource>(diagnostics);
        services.AddControllersWithViews().AddJsonOptions(json => appJson(json.JsonSerializerOptions));
        return services.BuildServiceProvider();
    }
}
