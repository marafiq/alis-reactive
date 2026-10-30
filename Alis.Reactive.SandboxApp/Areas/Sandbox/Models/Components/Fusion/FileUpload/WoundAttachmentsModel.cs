using System.Globalization;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.FileUpload
{
    /// <summary>The files a wound-care nurse attaches to Margaret Hale's record after a wound review.</summary>
    public class WoundAttachmentsModel
    {
        public string? WoundSite { get; set; }

        /// <summary>The signed consent form and the wound photo.</summary>
        public IFormFile[]? Attachments { get; set; }
    }

    // The record keeps PDFs and JPG or PNG photos of at most 5 MB. The browser checks that something is attached;
    // only the server reads the files, so it alone refuses one too large or of the wrong kind, and names each file it received.
    public class WoundAttachmentsValidator : ReactiveValidator<WoundAttachmentsModel>
    {
        private const long MaxBytes = 5 * 1024 * 1024;
        private static readonly string[] Kinds = [".pdf", ".jpg", ".jpeg", ".png"];

        public WoundAttachmentsValidator()
        {
            RuleFor(x => x.WoundSite).NotEmpty().WithMessage("Name the wound site.");
            RuleFor(x => x.Attachments)
                .NotEmpty().WithMessage("Attach the signed consent and the wound photo.")
                .Must(files => files == null || files.All(Accepted))
                .WithMessage(m => "Nothing was attached: " + string.Join(" ", m.Attachments!.Where(f => !Accepted(f)).Select(Problem))
                    + " Received: " + string.Join(", ", m.Attachments!.Select(Described)) + ".");
        }

        internal static string Described(IFormFile file) => $"{file.FileName} ({Size(file.Length)})";

        private static bool Accepted(IFormFile file) => IsKept(file) && file.Length <= MaxBytes;

        private static bool IsKept(IFormFile file) =>
            Kinds.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase);

        private static string Problem(IFormFile file) => IsKept(file)
            ? $"{file.FileName} is larger than 5 MB."
            : $"{file.FileName} is not a PDF, JPG or PNG.";

        private static string Size(long bytes) => bytes switch
        {
            < 1024 => bytes == 1 ? "1 byte" : $"{bytes} bytes",
            < 1024 * 1024 => $"{bytes / 1024} KB",
            _ => (bytes / (1024d * 1024d)).ToString("0.0", CultureInfo.InvariantCulture) + " MB"
        };
    }

    /// <summary>What the record answers when the files are attached.</summary>
    public sealed class WoundAttachmentsAnswer
    {
        public string Message { get; set; } = "";
    }
}
