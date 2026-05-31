using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class TodoValidator : ReactiveValidator<TodoModel>
    {
        public TodoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            ClientRule(x => x.Title)
                .Required("'Title' is required.")
                .MaxLength(200, "'Title' must be at most 200 characters.");

            WhenField(x => x.IsUrgent, () =>
            {
                RuleFor(x => x.DueDate).NotEmpty().WithMessage("Urgent todos need a due date");
                ClientRule(x => x.DueDate).Required("Urgent todos need a due date");
            });
        }
    }
}
