using System.Net;
using System.Text;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

static class HttpProbes
{
    public static async Task Run(Uri baseAddress)
    {
        using var client = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false }) { BaseAddress = baseAddress };
        var options = new AntiforgeryOptions();
        Console.WriteLine("core-antiforgery-names: header " + options.HeaderName + ", form field " + options.FormFieldName);
        var token = await client.GetStringAsync("/h/token");
        var jsonWithHeader = await Status(client, HttpMethod.Post, "/h/protected", JsonBody("{}"), token);
        var formWithField = await Status(client, HttpMethod.Post, "/h/protected", FormBody("__RequestVerificationToken=" + Uri.EscapeDataString(token)), null);
        var multipartWithField = await Status(client, HttpMethod.Post, "/h/protected", Multipart(("__RequestVerificationToken", token)), null);
        var jsonWithMember = await Status(client, HttpMethod.Post, "/h/protected", JsonBody("{\"__RequestVerificationToken\":\"" + token + "\"}"), null);
        var noToken = await Status(client, HttpMethod.Post, "/h/protected", JsonBody("{}"), null);
        Console.WriteLine("core-antiforgery-placement: [ValidateAntiForgeryToken] answers a JSON body with the token in the header " + jsonWithHeader + "; a URL-encoded body with it in its field " + formWithField + "; a multipart body with it in its field " + multipartWithField + "; a JSON body with it in a member " + jsonWithMember + "; no token " + noToken);
        var safe = new List<string>();
        foreach (var method in new[] { HttpMethod.Get, HttpMethod.Head, HttpMethod.Options, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete })
        {
            safe.Add(method.Method + " " + await Status(client, method, "/h/auto", null, null));
        }
        Console.WriteLine("core-antiforgery-safe-methods: [AutoValidateAntiforgeryToken] with no token answers " + string.Join(", ", safe));
        Console.WriteLine("core-head-on-get-action: HEAD to an [HttpGet] action answers " + await Status(client, HttpMethod.Head, "/h/get-only", null, null) + "; HEAD to an [HttpHead] action answers " + await Status(client, HttpMethod.Head, "/h/head-only", null, null));
        using (var allowed = await Send(client, HttpMethod.Options, "/h/allow", null, null, baseAddress.GetLeftPart(UriPartial.Authority), false))
        using (var wrongVerb = await Send(client, HttpMethod.Options, "/h/get-only", null, null, baseAddress.GetLeftPart(UriPartial.Authority), false))
        using (var preflight = await Send(client, HttpMethod.Options, "/h/allow", null, null, "http://elsewhere.test", true))
        {
            Console.WriteLine("core-options-reaches-action: OPTIONS with its own Origin to an [HttpOptions] action answers " + (int)allowed.StatusCode + " Allow " + string.Join(",", allowed.Content.Headers.Allow) + "; to an [HttpGet] action answers " + (int)wrongVerb.StatusCode + " Allow " + string.Join(",", wrongVerb.Content.Headers.Allow) + "; a preflight under the CORS default policy answers " + (int)preflight.StatusCode + (preflight.Headers.Contains("Access-Control-Allow-Methods") ? " from the CORS middleware" : " from the action"));
        }
        Console.WriteLine("core-string-result-media-type: a string action answers " + await MediaType(client, "/h/text", null) + "; with Accept application/json " + await MediaType(client, "/h/text", "application/json") + " " + await Body(client, "/h/text", "application/json") + "; with Accept text/html " + await MediaType(client, "/h/text", "text/html") + "; Content(text) answers " + await MediaType(client, "/h/content", null));
        Console.WriteLine("core-accept-weighted: a string action with Accept text/plain, application/json;q=0.5, */*;q=0.1 answers " + await MediaType(client, "/h/text", "text/plain, application/json;q=0.5, */*;q=0.1") + "; with application/json, */*;q=0.1 answers " + await MediaType(client, "/h/text", "application/json, */*;q=0.1") + "; with text/html, */*;q=0.1 answers " + await MediaType(client, "/h/text", "text/html, */*;q=0.1") + "; a 400 ValidationProblem with Accept text/plain, */*;q=0.1 answers " + await PostMediaType(client, "/h/invalid", "text/plain, */*;q=0.1"));
        using (var file = await client.GetAsync("/h/file"))
        {
            Console.WriteLine("core-file-disposition: File(bytes, \"application/pdf\", \"Résumé.pdf\") answers " + file.Content.Headers.ContentType + " with Content-Disposition " + string.Join(",", file.Content.Headers.GetValues("Content-Disposition")));
        }
        using (var documents = new MultipartFormDataContent())
        {
            documents.Add(new StringContent("North"), "Ward");
            documents.Add(new StringContent("Front"), "Documents[0].Caption");
            documents.Add(Pdf(4), "Documents[0].File", "a.pdf");
            documents.Add(new StringContent("Back"), "Documents[1].Caption");
            documents.Add(Pdf(6), "Documents[1].File", "b.pdf");
            documents.Add(Pdf(1), "Photos", "p1.png");
            documents.Add(Pdf(2), "Photos", "p2.png");
            using var bound = await client.PostAsync("/h/documents", documents);
            Console.WriteLine("core-multipart-files-and-fields: " + await bound.Content.ReadAsStringAsync());
        }
        using (var raw = new ByteArrayContent(Encoding.UTF8.GetBytes("--b\r\nContent-Disposition: form-data; name=\"Photos\"; filename=\"Résumé.pdf\"\r\nContent-Type: application/pdf\r\n\r\n%PDF\r\n--b--\r\n")))
        {
            raw.Headers.TryAddWithoutValidation("Content-Type", "multipart/form-data; boundary=b");
            using var bound = await client.PostAsync("/h/documents", raw);
            Console.WriteLine("core-multipart-utf8-file-name: a part whose filename is Résumé.pdf in UTF-8, as a browser writes it, binds " + await bound.Content.ReadAsStringAsync());
        }
        var routeOnly = await PostBody(client, "/h/r/North/intake", Multipart(("Caption", "x")));
        var routeAndBody = await PostBody(client, "/h/r/North/intake", Multipart(("Ward", "South")));
        var routeIntoJson = await PostBody(client, "/h/r/North/intake-json", JsonBody("{}"));
        Console.WriteLine("core-route-value-binds-member: /h/r/North/intake with a multipart body lacking Ward binds " + routeOnly + "; with Ward=South in the body binds " + routeAndBody + "; a [FromBody] model with {} binds " + routeIntoJson);
        Console.WriteLine("core-json-to-form-bound: an action whose model is bound from the form answers a JSON body {\"ward\":\"South\"} " + await PostBody(client, "/h/r/North/intake", JsonBody("{\"ward\":\"South\"}")) + "; the same action at /h/documents answers " + await PostBody(client, "/h/documents", JsonBody("{\"ward\":\"South\"}")));
        Console.WriteLine("core-frombody-multipart: a [FromBody] action answers a multipart body " + await Status(client, HttpMethod.Post, "/h/r/North/intake-json", Multipart(("Ward", "North")), null) + " and a URL-encoded body " + await Status(client, HttpMethod.Post, "/h/r/North/intake-json", FormBody("Ward=North"), null));
        var later = await client.GetStringAsync("/h/token");
        Console.WriteLine("core-antiforgery-later-token: a token issued by a later GET, as a slot's partial gets one, " + (later == token ? "is the same token" : "is a new token") + " and answers " + await Status(client, HttpMethod.Post, "/h/protected", JsonBody("{}"), later) + "; the first token still answers " + await Status(client, HttpMethod.Post, "/h/protected", JsonBody("{}"), token));
        Console.WriteLine("core-query-key-case: ward=North&SHIFT=Night binds " + await client.GetStringAsync("/h/page?ward=North&SHIFT=Night"));
        Console.WriteLine("core-consumes-urlencoded: [Consumes(\"application/x-www-form-urlencoded\")] answers a URL-encoded body " + await Status(client, HttpMethod.Post, "/h/urlencoded", FormBody("Ward=North"), null) + " and a multipart body " + await Status(client, HttpMethod.Post, "/h/urlencoded", Multipart(("Ward", "North")), null));
        using (var large = new MultipartFormDataContent())
        {
            large.Add(new ByteArrayContent(new byte[31_000_000]), "file", "large.bin");
            string answer;
            try
            {
                using var response = await client.PostAsync("/h/upload", large);
                answer = ((int)response.StatusCode).ToString();
            }
            catch (HttpRequestException exception)
            {
                answer = "no response: " + exception.HttpRequestError;
            }
            Console.WriteLine("core-upload-over-limit: a 31,000,000-byte file under Kestrel's default body limit answers " + answer);
        }
    }

    static StringContent JsonBody(string json) => new(json, Encoding.UTF8, "application/json");

    static StringContent FormBody(string form) => new(form, Encoding.UTF8, "application/x-www-form-urlencoded");

    static MultipartFormDataContent Multipart(params (string Name, string Value)[] fields)
    {
        var content = new MultipartFormDataContent();
        foreach (var (name, value) in fields)
        {
            content.Add(new StringContent(value), name);
        }
        return content;
    }

    static ByteArrayContent Pdf(int length)
    {
        var content = new ByteArrayContent(new byte[length]);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        return content;
    }

    static async Task<int> Status(HttpClient client, HttpMethod method, string path, HttpContent? content, string? token)
    {
        using var response = await Send(client, method, path, content, token, null, false);
        return (int)response.StatusCode;
    }

    static Task<HttpResponseMessage> Send(HttpClient client, HttpMethod method, string path, HttpContent? content, string? token, string? origin, bool preflight)
    {
        var request = new HttpRequestMessage(method, path) { Content = content };
        if (token != null) request.Headers.Add("RequestVerificationToken", token);
        if (origin != null) request.Headers.Add("Origin", origin);
        if (preflight) request.Headers.Add("Access-Control-Request-Method", "POST");
        return client.SendAsync(request);
    }

    static async Task<string> PostBody(HttpClient client, string path, HttpContent content)
    {
        using var response = await client.PostAsync(path, content);
        return (int)response.StatusCode + " " + await response.Content.ReadAsStringAsync();
    }

    static async Task<string> MediaType(HttpClient client, string path, string? accept)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (accept != null) request.Headers.Add("Accept", accept);
        using var response = await client.SendAsync(request);
        return response.Content.Headers.ContentType?.ToString() ?? "no media type";
    }

    static async Task<string> PostMediaType(HttpClient client, string path, string accept)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = new StringContent("", Encoding.UTF8, "application/x-www-form-urlencoded") };
        request.Headers.Add("Accept", accept);
        using var response = await client.SendAsync(request);
        return (int)response.StatusCode + " " + (response.Content.Headers.ContentType?.MediaType ?? "no media type");
    }

    static async Task<string> Body(HttpClient client, string path, string accept)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("Accept", accept);
        using var response = await client.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }
}

[Route("h")]
public sealed class HttpProbeController(IAntiforgery antiforgery) : Controller
{
    [HttpGet("token")]
    public string Token() => antiforgery.GetAndStoreTokens(HttpContext).RequestToken!;

    [HttpPost("protected"), ValidateAntiForgeryToken]
    public string Protected() => "ok";

    [AutoValidateAntiforgeryToken]
    [AcceptVerbs("GET", "HEAD", "OPTIONS", "POST", "PUT", "DELETE", Route = "auto")]
    public IActionResult Auto() => Ok("ok");

    [HttpGet("get-only")]
    public string GetOnly() => "got";

    [HttpHead("head-only")]
    public IActionResult HeadOnly() => Ok();

    [HttpOptions("allow")]
    public IActionResult Allow()
    {
        Response.Headers.Allow = "GET, POST";
        return NoContent();
    }

    [HttpPost("invalid")]
    public IActionResult Invalid()
    {
        ModelState.AddModelError("Ward", "Name the ward.");
        return ValidationProblem(ModelState);
    }

    [HttpGet("text")]
    public string Text() => "Ann";

    [HttpGet("content")]
    public IActionResult ContentText() => Content("Ann");

    [HttpGet("file")]
    public IActionResult Download() => File(new byte[] { 37, 80, 68, 70 }, "application/pdf", "Résumé.pdf");

    [HttpPost("documents")]
    public string Documents(ProbeIntake intake) =>
        "Ward " + intake.Ward + "; " + string.Join("; ", intake.Documents.Select(document => document.Caption + " " + (document.File?.FileName ?? "no file") + " " + (document.File?.Length ?? 0))) + "; Photos " + string.Join(",", intake.Photos.Select(photo => photo.FileName + " " + photo.Length));

    [HttpPost("r/{ward}/intake")]
    public string RouteIntake(ProbeIntake intake) => "Ward " + (intake.Ward ?? "null");

    [HttpPost("r/{ward}/intake-json")]
    public string RouteIntakeJson([FromBody] ProbeIntake intake) => "Ward " + (intake.Ward ?? "null");

    [HttpGet("page")]
    public string Page(ProbePage page) => "Ward=" + page.Ward + " Shift=" + page.Shift;

    [HttpPost("urlencoded"), Consumes("application/x-www-form-urlencoded")]
    public string UrlEncoded(ProbeIntake intake) => "Ward " + intake.Ward;

    [HttpPost("upload")]
    public string Upload(IFormFile file) => file.Length.ToString();
}

public sealed class ProbeIntake
{
    public string? Ward { get; set; }
    public List<ProbeDocument> Documents { get; set; } = [];
    public List<IFormFile> Photos { get; set; } = [];
}

public sealed class ProbeDocument
{
    public IFormFile? File { get; set; }
    public string? Caption { get; set; }
}

public sealed class ProbePage
{
    public string? Ward { get; set; }
    public string? Shift { get; set; }
}
