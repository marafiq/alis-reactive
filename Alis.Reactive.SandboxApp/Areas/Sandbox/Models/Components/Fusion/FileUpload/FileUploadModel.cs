using Microsoft.AspNetCore.Http;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class FileUploadModel
    {
        public string? ResidentName { get; set; }
        public IFormFile[]? Documents { get; set; }
    }

    public class FileUploadEchoResponse
    {
        public string? ResidentName { get; set; }
        public int FileCount { get; set; }
        public string[]? FileNames { get; set; }
        public long[]? FileSizes { get; set; }
    }
}
