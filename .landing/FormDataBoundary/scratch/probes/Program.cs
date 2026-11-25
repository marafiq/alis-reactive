using System.Text;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Logging.ClearProviders();
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
await app.StartAsync();
using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };

async Task<string> Send(HttpMethod method, string url, HttpContent? body)
{
    using var request = new HttpRequestMessage(method, url) { Content = body };
    using var response = await client.SendAsync(request);
    return (int)response.StatusCode + " " + await response.Content.ReadAsStringAsync();
}
HttpContent Text(string s) => new StringContent(s, Encoding.UTF8, "text/plain");
HttpContent File() { var c = new ByteArrayContent(new byte[] { 37, 80, 68, 70 }); c.Headers.ContentType = new("application/pdf"); return c; }
HttpContent Multipart(params (string Key, string Value)[] fields) { var m = new MultipartFormDataContent(); foreach (var f in fields) m.Add(new StringContent(f.Value), f.Key); return m; }
HttpContent Json(string s) => new StringContent(s, Encoding.UTF8, "application/json");

Console.WriteLine("core-plain-no-body: DELETE /plain/North?Shift=Night binds " + await Send(HttpMethod.Delete, "/plain/North?Shift=Night", null));
Console.WriteLine("core-plain-get: GET /plain/North?Shift=Night binds " + await Send(HttpMethod.Get, "/plain/North?Shift=Night", null));
Console.WriteLine("core-plain-text-body: POST /plain/North?Shift=Night with a text body binds " + await Send(HttpMethod.Post, "/plain/North?Shift=Night", Text("Bed by the window")));
Console.WriteLine("core-plain-file-body: POST /plain/North?Shift=Night with a file body binds " + await Send(HttpMethod.Post, "/plain/North?Shift=Night", File()));
Console.WriteLine("core-fromform-no-body: DELETE /form/North?Shift=Night binds " + await Send(HttpMethod.Delete, "/form/North?Shift=Night", null));
Console.WriteLine("core-fromform-text-body: POST /form/North?Shift=Night with a text body binds " + await Send(HttpMethod.Post, "/form/North?Shift=Night", Text("Bed by the window")));
Console.WriteLine("core-fromform-multipart-query: POST /form/North?Shift=Evening with a multipart body Text=x binds " + await Send(HttpMethod.Post, "/form/North?Shift=Evening", Multipart(("Text", "x"))));
Console.WriteLine("core-plain-multipart-query: POST /plain/North?Shift=Evening with a multipart body Text=x binds " + await Send(HttpMethod.Post, "/plain/North?Shift=Evening", Multipart(("Text", "x"))));
Console.WriteLine("core-plain-multipart-query-and-body: POST /plain/North?Shift=Evening with a multipart body Shift=Night binds " + await Send(HttpMethod.Post, "/plain/North?Shift=Evening", Multipart(("Shift", "Night"))));
Console.WriteLine("core-frombody-json-query: POST /json/North?Shift=Evening with {} binds " + await Send(HttpMethod.Post, "/json/North?Shift=Evening", Json("{}")));
Console.WriteLine("core-empty-query-text: DELETE /plain/North?Shift= binds " + await Send(HttpMethod.Delete, "/plain/North?Shift=", null));
Console.WriteLine("core-start-kept: DELETE /started/North binds " + await Send(HttpMethod.Delete, "/started/North", null) + "; with ?Shift= binds " + await Send(HttpMethod.Delete, "/started/North?Shift=", null));
await app.StopAsync();

public class Note
{
    public string? Ward { get; set; }
    public string? Shift { get; set; }
    public string? Text { get; set; }
}

public class StartedNote
{
    public string? Ward { get; set; }
    public string? Shift { get; set; } = "Evening";
}

public class NotesController : Controller
{
    static string Show(string? ward, string? shift) => "Ward=" + (ward ?? "null") + " Shift=" + (shift ?? "null");

    [AcceptVerbs("GET", "POST", "DELETE", Route = "/plain/{ward}")]
    public string Plain(Note note) => Show(note.Ward, note.Shift);

    [AcceptVerbs("POST", "DELETE", Route = "/form/{ward}")]
    public string Form([FromForm] Note note) => Show(note.Ward, note.Shift);

    [HttpPost("/json/{ward}")]
    public string Json([FromBody] Note note) => Show(note.Ward, note.Shift);

    [HttpDelete("/started/{ward}")]
    public string Started(StartedNote note) => Show(note.Ward, note.Shift);
}
