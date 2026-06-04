using Alis.Reactive.FluentValidator;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// Syncfusion EJ2 DataSource rendering reads Newtonsoft defaults, so example API payloads stay camelCase.
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver()
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddReactiveFluentValidation(validation =>
    validation.AddFromAssemblyContaining<Program>());

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Intake}/{action=Index}/{id?}");

app.Run();
