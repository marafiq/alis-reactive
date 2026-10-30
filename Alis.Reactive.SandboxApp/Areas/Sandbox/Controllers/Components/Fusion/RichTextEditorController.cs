using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.RichTextEditor;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// The family newsletter's Garden Club paragraph: last month's paragraph to start from, and the save that
    /// counts the words families will read.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/RichTextEditor")]
    public class RichTextEditorController : Controller
    {
        private const string LastMonthParagraph =
            "<p>The <strong>Garden Club</strong> potted herbs for the dining room windows.</p>";

        [HttpGet("")]
        public IActionResult Index()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/RichTextEditor/Index.cshtml",
                new RichTextEditorModel { LastMonthParagraph = LastMonthParagraph });
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] NewsletterParagraphSave save)
        {
            var words = WordsIn(save.Paragraph);
            var noun = words == 1 ? "word" : "words";
            return Ok(new NewsletterParagraphSaved { Message = $"Saved the Garden Club paragraph: {words} {noun}." });
        }

        // The words a family reads: the paragraph's text without its markup.
        private static int WordsIn(string paragraph)
        {
            var text = Regex.Replace(paragraph, "<[^>]+>", " ").Replace("&nbsp;", " ");
            return text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}
