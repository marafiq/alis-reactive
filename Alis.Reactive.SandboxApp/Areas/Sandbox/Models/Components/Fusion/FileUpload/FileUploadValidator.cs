using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class FileUploadValidator : ReactiveValidator<FileUploadModel>
    {
        public FileUploadValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("'Resident Name' is required.");
            ClientRule(x => x.ResidentName).Required("'Resident Name' is required.");
            RuleFor(x => x.Documents).NotEmpty().WithMessage("At least one document is required.");
            ClientRule(x => x.Documents).AtLeastOne("At least one document is required.");
        }
    }
}
