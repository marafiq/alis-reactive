using Alis.Reactive;
using Alis.Reactive.FluentValidator;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

// Global: Newtonsoft serializes PascalCase C# → camelCase JSON.
// SF EJ2 uses Newtonsoft internally for DataSource rendering —
// JsonConvert.DefaultSettings merges into SF's explicit settings.
JsonConvert.DefaultSettings = () => new JsonSerializerSettings
{
    ContractResolver = new CamelCasePropertyNamesContractResolver()
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

ReactivePlanConfig.UseValidationExtractor(
    new FluentValidationAdapter(type => (FluentValidation.IValidator?)Activator.CreateInstance(type)));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
