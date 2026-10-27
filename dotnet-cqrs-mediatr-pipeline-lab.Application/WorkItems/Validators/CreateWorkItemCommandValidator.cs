using DotNetCqrsMediatRPipelineLab.Contract.WorkItems.Commands;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace DotNetCqrsMediatRPipelineLab.Application.WorkItems.Validators;

public class CreateWorkItemCommandValidator: AbstractValidator<CreateWorkItemCommand>
{
    public CreateWorkItemCommandValidator(IOptions<WorkItemOptions> options)
    {
        var maxTitleLength = options.Value.MaxTitleLength;

        RuleFor(command => command.Title)
            .Cascade(CascadeMode.Stop)//stop là dừng lại, tức một cái thất bại thì stop luôn
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Work item title is required.")
            .MinimumLength(3)
            .MaximumLength(maxTitleLength);
    }
}