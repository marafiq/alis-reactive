using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.WebUtilities;

internal static class RenderProbes
{
    public static void Run(IServiceProvider services, string webRoot)
    {
        Console.WriteLine("core-stj-default-encoder-html: System.Text.Json's default encoder writes \"</script><b>Ada</b> & co\" as "
            + JsonSerializer.Serialize("</script><b>Ada</b> & co"));
        var versioned = services.GetRequiredService<IFileVersionProvider>().AddFileVersionToPath(PathString.Empty, "/probe.js");
        var hash = WebEncoders.Base64UrlEncode(SHA256.HashData(File.ReadAllBytes(Path.Combine(webRoot, "probe.js"))));
        Console.WriteLine("core-append-version-hash: asp-append-version writes " + versioned
            + "; the SHA-256 of the file's bytes in URL-safe base64 with no padding is " + hash
            + "; equal " + (versioned == "/probe.js?v=" + hash));
    }
}
