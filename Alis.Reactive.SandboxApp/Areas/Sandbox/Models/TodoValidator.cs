using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class TodoValidator : ReactiveValidator<TodoModel>
    {
        public TodoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);

            WhenField(x => x.IsUrgent, () =>
            {
                RuleFor(x => x.DueDate).NotEmpty().WithMessage("Urgent todos need a due date");
            });
        }
    }
}
