# Alis.Reactive

Reactive pages for ASP.NET MVC, written in your views as fluent, typed C#. The C# says what a page
does (react to an event, send a request, check a form on both sides, load a partial, show a branch);
the framework renders it as a plan that its browser runtime runs. Views carry no JavaScript.

It is for teams building server-rendered MVC applications, on ASP.NET Core MVC (.NET 10) or ASP.NET
MVC 5 (.NET Framework 4.8), who want dynamic pages without hand-written scripts, and who want to bring
any JavaScript component (Syncfusion, native inputs, their own) through one typed declaration.

## ASP.NET Core MVC on .NET 10

### How do you add Alis.Reactive to an app?

Add the package to your ASP.NET Core MVC app:

```bash
dotnet add package AlisReactive
```

### What does a reactive page look like?

A view opens its plan, says what happens and when, and writes the plan as its last line. The home page below shows
"Not booted" as the server renders it, and "Booted" once the page has loaded in the browser.

### How does a view open its plan?

```cshtml
@model Welcome
@{
    ViewData["Title"] = "Home";
    var plan = Html.ReactivePlan<Welcome>();
}
```

`Html.ReactivePlan<TModel>()` opens the plan for the view's model, a class of your own that the action hands the
view. Every door after it is typed by that class, so the compiler catches a wrong member before the browser does.

```csharp
public sealed class Welcome
{
    public string Message { get; set; } = "Booted";
}
```

### How does the page react once it loads?

```cshtml
<p id="consumer-status">Not booted</p>
@{
    Html.On(plan, trigger => trigger.DomReady(pipeline => pipeline.Element("consumer-status").Text.Set(Model.Message)));
}
```

`Html.On` handles a trigger: `DomReady` runs its pipeline once the page has loaded, and `pipeline.Element(id)` reaches
an element by the id you gave it. `Model.Message` is the view's own model, read where the view renders. The view
holds no JavaScript.

### How does the plan reach the page?

```cshtml
@Html.RenderPlan(plan)
```

`Html.RenderPlan(plan)` writes the plan into the page, as the view's last line.

### Which namespaces does the view name?

```cshtml
@using Alis.Reactive.Consumer.Net10.Models
@using Alis.Reactive.Consumer.Net10.Components
@using Alis.Reactive.RenderingAndDelivery
@using Alis.Reactive.TriggersAndPipelines
```

The first two `@using` lines are the app's own: where the view's model lives, and where the app's own components live
("How do you bring a component of your own?" below). Each of the others names the namespace of the fluent builders a
view calls: `Alis.Reactive.RenderingAndDelivery` for the plan, `Alis.Reactive.TriggersAndPipelines` for its triggers
and what a pipeline does. Write them at the top of the view, or once in `Views/_ViewImports.cshtml` for every view.

### How does the browser get the framework's script?

Your layout links it after the page's content, as the last script of the page:

```cshtml
    <script src="~/_content/AlisReactive/js/alis-reactive.js" asp-append-version="true"></script>
```

The package carries the file as a static web asset, served under `_content/AlisReactive/`. `asp-append-version` adds
the hash of the file's content to its address, so a new release's file gets a new address. It is a tag helper, which
`Views/_ViewImports.cshtml` turns on:

```cshtml
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

### What does the app's startup need?

MVC, as a new MVC app registers it:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
```

and its static assets, which serve the package's script, before its routes:

```csharp
var app = builder.Build();
app.MapStaticAssets();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}").WithStaticAssets();
app.Run();
```

A page that only reacts in the browser needs no startup call of the framework's.

### How do you bring a component of your own?

A component is a JavaScript object an element holds, and your app brings one as the framework brings its own: a class
that declares the object's members by their JavaScript names, and a render helper that writes its element. The home
page's button is the app's own. The browser's `button` element is the object, so it needs no script:

```csharp
using Alis.Reactive.Declarations;
using Alis.Reactive.Vendors;

namespace Alis.Reactive.Consumer.Net10.Components;

public sealed class AppButton : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public Property<string> TextContent => Property<string>();

    public Event<Clicked> Click => Event<Clicked>();

    public sealed class Clicked : JsObject
    {
    }
}
```

`ComponentVendor.Native` says the element itself is the object and raises the DOM's events. `TextContent` is the
button's `textContent`, which a pipeline reads and sets, and `Click` is its `click` event, whose payload is a class
nested in the declaration, so no other event shares it.

The render helper builds the element under the id the view gives it, and `plan.Render` declares the component on it:

```csharp
using Alis.Reactive.RenderingAndDelivery;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.Consumer.Net10.Components;

public static class AppButtonHtml
{
    public static Rendered<AppButton, TModel> AppButton<TModel>(this ReactivePlan<TModel> plan, string id, string text)
    {
        var button = new TagBuilder("button");
        button.MergeAttribute("id", id);
        button.MergeAttribute("type", "button");
        button.InnerHtml.Append(text);
        return plan.Render<AppButton>(id, button);
    }
}
```

The view renders the button and says what its click does:

```cshtml
@(plan.AppButton("acknowledge", "Acknowledge").Reactive(button => button.Click, (clicked, pipeline) => pipeline.Component<AppButton>("acknowledge").TextContent.Set("Acknowledged")))
```

`Reactive` runs its pipeline each time the button raises `Click`. `pipeline.Component<AppButton>(id)` reaches the
component by the id you gave it, typed by its declaration, so a member the declaration does not hold does not compile.

Your app's slices are vertical, as the framework's are. A component is one folder, `Components/AppButton/`, holding
its declaration and its render helper, shared with no other component, as an action that needs a model has a model of
its own, shared with no other view. Your app shares a base class only for the abstract members it enforces.

## ASP.NET MVC 5 on .NET Framework 4.8

### How do you add Alis.Reactive to an MVC 5 app?

The package reaches an MVC 5 app through PackageReference, which brings its build files and the rules the compiler
checks your code by. Visual Studio's MVC 5 template restores by `packages.config`, and NuGet's migrator refuses an
ASP.NET project, so you move the project by hand. In its `.csproj`, name the restore style in the first
`PropertyGroup`:

```xml
    <RestoreProjectStyle>PackageReference</RestoreProjectStyle>
```

Write a `PackageReference` for each package you installed yourself, since the packages they depend on come with them,
then remove each `Reference` whose `HintPath` points into `packages\` and delete `packages.config`:

```xml
    <PackageReference Include="Microsoft.AspNet.Mvc" Version="5.3.0" />
    <PackageReference Include="Microsoft.CodeDom.Providers.DotNetCompilerPlatform" Version="4.1.0" />
```

Then add the package, and the container your startup builds, in the Package Manager Console:

```text
Install-Package AlisReactive
Install-Package Microsoft.Extensions.DependencyInjection -Version 10.0.15
```

Visual Studio builds the project, and so does its MSBuild with `msbuild /restore`. The .NET SDK's `dotnet build`
restores a classic MVC 5 project's packages and does not compile against them.

### What does the app's startup need?

A view reads the app's JSON options through MVC's resolver, on a page that only reacts too. `Application_Start` in
`Global.asax.cs` registers them with `AddReactiveJson` and sets MVC's resolver over the container:

```csharp
        protected void Application_Start()
        {
            var services = new ServiceCollection();
            services.AddReactiveJson(json => { });
            DependencyResolver.SetResolver(new ContainerResolver(DependencyResolver.Current, services.BuildServiceProvider()));
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
```

`AddReactiveJson` sets the options your app writes its data with, starting from camel-case names as ASP.NET Core MVC
does, and `json => { }` keeps them. `Global.asax.cs` names `Alis.Reactive.RenderingAndDelivery` and
`Microsoft.Extensions.DependencyInjection`. The resolver is your app's own class: it asks the container first and MVC's
own resolver for everything else.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Alis.Reactive.Consumer.Net48
{
    public class ContainerResolver : IDependencyResolver
    {
        private readonly IDependencyResolver _mvc;
        private readonly IServiceProvider _container;

        public ContainerResolver(IDependencyResolver mvc, IServiceProvider container)
        {
            _mvc = mvc;
            _container = container;
        }

        public object GetService(Type serviceType) => _container.GetService(serviceType) ?? _mvc.GetService(serviceType);

        public IEnumerable<object> GetServices(Type serviceType) => _container.GetServices(serviceType).Concat(_mvc.GetServices(serviceType));
    }
}
```

### Which namespaces do the views name?

`Views/Web.config` names them once for every view, in its `namespaces`:

```xml
        <add namespace="Alis.Reactive.RenderingAndDelivery" />
        <add namespace="Alis.Reactive.TriggersAndPipelines" />
```

The home page above then needs no `@using`: its plan, its trigger, its button and `Html.RenderPlan(plan)` are written
the same way.

### How does the browser get the framework's script?

The package's build copies its browser files into your app's `Content\AlisReactive\`, beside a `.gitignore` holding
`*`, so git lists none of them. Your layout links the script after the page's content, as the last script of the
page:

```cshtml
    <script src="@Url.AppendVersion("~/Content/AlisReactive/js/alis-reactive.js")"></script>
```

`Url.AppendVersion` adds the hash of the file's content to its address, as `asp-append-version` does on .NET 10, so a
new release's file gets a new address.

### How do you bring a component of your own?

The button's declaration is the class above, written in a namespace block, in `Components/AppButton/AppButton.cs`. Its
render helper hands MVC 5's markup to `plan.Render` as an `MvcHtmlString`:

```csharp
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Consumer.Net48.Components
{
    public static class AppButtonHtml
    {
        public static Rendered<AppButton, TModel> AppButton<TModel>(this ReactivePlan<TModel> plan, string id, string text)
        {
            var button = new TagBuilder("button");
            button.MergeAttribute("id", id);
            button.MergeAttribute("type", "button");
            button.SetInnerText(text);
            return plan.Render<AppButton>(id, new MvcHtmlString(button.ToString(TagRenderMode.Normal)));
        }
    }
}
```

A classic project compiles the files its `Compile` items list, so both files join them:

```xml
    <Compile Include="Components\AppButton\AppButton.cs" />
    <Compile Include="Components\AppButton\AppButtonHtml.cs" />
```

and `Views/Web.config` names their namespace for every view, beside the framework's:

```xml
        <add namespace="Alis.Reactive.Consumer.Net48.Components" />
```

The view renders the button with the same line as on .NET 10.

### When does a mistake in a view show?

MVC 5 compiles a view the first time it renders, not when the app builds. A view that opens its plan and never writes
it with `Html.RenderPlan(plan)` shows a page whose triggers do nothing, the first time you open it.

## This branch

This branch rewrites the framework from its specification. The design is in `spec/`, and `spec/REWRITE.md` lists
the units it is built in; each unit adds the documentation of the doors it lands to this file. The released
packages are built from `main`.

## Working on the framework

`scripts/doctor.sh` checks the tools, `scripts/build.sh` builds, and `scripts/playwright.sh` runs the browser
tests one selector names (`scripts/playwright.sh --help` lists them). `CLAUDE.md` holds the rules every contributor and coding agent follows.

## Unicode CLDR data

On .NET Framework 4.8, AlisReactive names a view's time zone to the browser by its IANA id, which it reads from a
table generated from Unicode CLDR's `common/supplemental/windowsZones.xml` at release-49
(`spec/probes/cldr/windowsZones.xml.txt`, pinned by `spec/probes/cldr/windowsZones.lock`). CLDR releases that file
under the Unicode License V3, whose notice follows.

```text
COPYRIGHT AND PERMISSION NOTICE

Copyright © 2001–2026 Unicode, Inc.

NOTICE TO USER: Carefully read the following legal agreement. BY
DOWNLOADING, INSTALLING, COPYING OR OTHERWISE USING DATA FILES, AND/OR
SOFTWARE, YOU UNEQUIVOCALLY ACCEPT, AND AGREE TO BE BOUND BY, ALL OF THE
TERMS AND CONDITIONS OF THIS AGREEMENT. IF YOU DO NOT AGREE, DO NOT
DOWNLOAD, INSTALL, COPY, DISTRIBUTE OR USE THE DATA FILES OR SOFTWARE.

Permission is hereby granted, free of charge, to any person obtaining a
copy of data files and any associated documentation (the "Data Files") or
software and any associated documentation (the "Software") to deal in the
Data Files or Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, and/or sell
copies of the Data Files or Software, and to permit persons to whom the
Data Files or Software are furnished to do so, provided that either (a)
this copyright and permission notice appear with all copies of the Data
Files or Software, or (b) this copyright and permission notice appear in
associated Documentation.

THE DATA FILES AND SOFTWARE ARE PROVIDED "AS IS", WITHOUT WARRANTY OF ANY
KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT OF
THIRD PARTY RIGHTS.

IN NO EVENT SHALL THE COPYRIGHT HOLDER OR HOLDERS INCLUDED IN THIS NOTICE
BE LIABLE FOR ANY CLAIM, OR ANY SPECIAL INDIRECT OR CONSEQUENTIAL DAMAGES,
OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS,
WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION,
ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THE DATA
FILES OR SOFTWARE.

Except as contained in this notice, the name of a copyright holder shall
not be used in advertising or otherwise to promote the sale, use or other
dealings in these Data Files or Software without prior written
authorization of the copyright holder.
```
