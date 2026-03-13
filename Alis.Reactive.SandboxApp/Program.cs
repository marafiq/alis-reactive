using Alis.Reactive;
using Alis.Reactive.FluentValidator;

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
