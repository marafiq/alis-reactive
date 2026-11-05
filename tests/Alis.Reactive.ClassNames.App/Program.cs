using Alis.Reactive;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddRazorOptions(razor =>
{
    razor.ViewLocationFormats.Clear();
    razor.ViewLocationFormats.Add("/{1}/{0}.cshtml");
    razor.ViewLocationFormats.Add("/Shared/{0}.cshtml");
});
builder.Services.AddReactiveFluentValidation(validation => validation.AddFromAssemblyContaining<Program>());
builder.Services.AddReactiveClassNames(names =>
{
    names.Field = "mb-3";
    names.FieldLabel = "form-label";
    names.FieldRequired = "text-danger";
    names.FieldMessage = "invalid-feedback";
    names.InvalidInput = "is-invalid";
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapControllerRoute("slices", "{controller=Notices}/{action=Index}");
app.Run();

public partial class Program;
