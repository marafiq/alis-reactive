using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.NestedBugs;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    [Area("Sandbox")]
    [Route("Sandbox/Validation/NestedBugs")]
    public class NestedBugsController : Controller
    {
        [HttpGet("NestedCondition")]
        public IActionResult NestedCondition() =>
            View("~/Areas/Sandbox/Views/Validation/NestedBugs/NestedCondition.cshtml",
                new NestedAddressModel());

        [HttpGet("ParentChild")]
        public IActionResult ParentChild() =>
            View("~/Areas/Sandbox/Views/Validation/NestedBugs/ParentChild.cshtml",
                new ParentChildModel());

        [HttpGet("IncludeConditional")]
        public IActionResult IncludeConditional() =>
            View("~/Areas/Sandbox/Views/Validation/NestedBugs/IncludeConditional.cshtml",
                new IncludeModel());

        [HttpPost("SubmitNestedAddress")]
        public IActionResult SubmitNestedAddress([FromBody] NestedAddressModel? model)
        {
            if (model == null) return BadRequest(new { errors = new { Name = new[] { "Request body required." } } });
            var result = new NestedAddressParentValidator().Validate(model);
            if (!result.IsValid)
                return BadRequest(new { errors = result.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()) });
            return Ok(new { message = "Saved" });
        }

        [HttpPost("SubmitParentChild")]
        public IActionResult SubmitParentChild([FromBody] ParentChildModel? model)
        {
            if (model == null) return BadRequest(new { errors = new { ParentFlag = new[] { "Request body required." } } });
            var result = new ParentChildBugValidator().Validate(model);
            if (!result.IsValid)
                return BadRequest(new { errors = result.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()) });
            return Ok(new { message = "Saved" });
        }

        [HttpPost("SubmitInclude")]
        public IActionResult SubmitInclude([FromBody] IncludeModel? model)
        {
            if (model == null) return BadRequest(new { errors = new { IsEmployed = new[] { "Request body required." } } });
            var result = new IncludeBugValidator().Validate(model);
            if (!result.IsValid)
                return BadRequest(new { errors = result.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()) });
            return Ok(new { message = "Saved" });
        }
    }
}
