using Alis.Reactive;
using Alis.Reactive.FluentValidator;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// SF EJ2 uses Newtonsoft internally — camelCase for DataSource rendering
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver()
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Register FluentValidation extraction for client-side validation
ReactivePlanConfig.UseValidationExtractor(
    new FluentValidationAdapter(type =>
        (FluentValidation.IValidator?)Activator.CreateInstance(type)));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Intake}/{action=Index}/{id?}");

app.Run();
