using Alis.Reactive.FluentValidator;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/Grid")]
    public partial class GridController : Controller
    {
        private static readonly List<ResidentGridItem> AllResidents = GenerateResidents();

        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Areas/Sandbox/Views/Components/Fusion/Grid/Index.cshtml");
        }

        [HttpGet("Editing")]
        public IActionResult Editing()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/Editing.cshtml",
                new ResidentGridEditingModel());
        }

        [HttpGet("ActionCompleteSaveEdit")]
        public IActionResult ActionCompleteSaveEdit()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/ActionCompleteSaveEdit.cshtml",
                new ResidentGridEditingModel());
        }

        [HttpGet("BeginEditNormal")]
        public IActionResult BeginEditNormal()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/BeginEditNormal.cshtml",
                new ResidentGridEditingModel());
        }

        [HttpGet("BuilderRoster")]
        public IActionResult BuilderRoster()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/BuilderRoster.cshtml",
                new ResidentGridEditingModel());
        }

        [HttpGet("RemoteAdaptorRoster")]
        public IActionResult RemoteAdaptorRoster()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/RemoteAdaptorRoster.cshtml",
                new ResidentGridEditingModel());
        }

        [HttpGet("PrintableRoster")]
        public IActionResult PrintableRoster()
        {
            return View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/PrintableRoster.cshtml",
                new ResidentGridEditingModel());
        }

        /// <summary>
        /// The remote adaptor roster's data endpoint: the page the grid's DataManager asks for, as
        /// <c>{result, count}</c>.
        /// </summary>
        [HttpPost("Data")]
        public IActionResult Data([FromBody] GridDataRequest? request)
        {
            request ??= new GridDataRequest();
            var paged = AllResidents.Skip(request.Skip).Take(request.Take > 0 ? request.Take : 10).ToList();

            return Ok(new ResidentGridResponse { Result = paged, Count = AllResidents.Count });
        }

        private static List<ResidentGridItem> GenerateResidents()
        {
            var firstNames = new[]
            {
                "Alice", "Bob", "Carol", "David", "Eve", "Frank", "Grace", "Henry",
                "Irene", "Jack", "Karen", "Leo", "Maria", "Nathan", "Olivia", "Paul",
                "Quinn", "Ruth", "Samuel", "Tina", "Uma", "Victor", "Wendy", "Xavier",
                "Yvonne", "Zachary"
            };
            var lastNames = new[]
            {
                "Johnson", "Smith", "Davis", "Wilson", "Martinez", "Brown", "Lee", "Taylor",
                "Anderson", "Thomas", "White", "Harris", "Clark", "Lewis", "Walker", "Young",
                "King", "Wright", "Hill", "Scott", "Green", "Adams", "Baker", "Nelson",
                "Carter", "Mitchell"
            };
            var careLevels = new[] { "Independent", "Assisted", "Memory Care", "Skilled Nursing" };
            var wings = new[] { "East", "West", "North", "South" };

            var residents = new List<ResidentGridItem>();
            var rng = new Random(42);

            for (var i = 0; i < 200; i++)
            {
                residents.Add(new ResidentGridItem
                {
                    Name = $"{firstNames[rng.Next(firstNames.Length)]} {lastNames[rng.Next(lastNames.Length)]}",
                    Age = rng.Next(65, 99),
                    CareLevel = careLevels[rng.Next(careLevels.Length)],
                    Wing = wings[rng.Next(wings.Length)]
                });
            }

            return residents;
        }

        private static IActionResult ValidationError(IReadOnlyDictionary<string, string[]> errors) =>
            new BadRequestObjectResult(new { errors });

        private static bool TryValidate<T>(IValidator<T> validator, T model, out IActionResult error)
        {
            var result = validator.Validate(model);
            error = result.IsValid ? null! : ValidationError(result.ErrorsByField);
            return result.IsValid;
        }
    }
}
