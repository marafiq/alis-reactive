using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class TodoValidator : ReactiveValidator<TodoModel>
    {
        public TodoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("'Title' is required.")
                .MaximumLength(200).WithMessage("'Title' must be at most 200 characters.");

            When(x => x.IsUrgent, () =>
            {
                RuleFor(x => x.DueDate).NotEmpty().WithMessage("Urgent todos need a due date");
            });
        }
    }
}
