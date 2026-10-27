using DotNetCqrsMediatRPipelineLab.Contract.WorkItems;
using FluentValidation;

namespace DotNetCqrsMediatRPipelineLab.Application.WorkItems.Validators;

public class ChangeWorkItemStatusCommandValidator : AbstractValidator<Command.ChangeWorkItemStatus>
{
    public ChangeWorkItemStatusCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Status)
            .IsInEnum();

        RuleFor(command => command.ExpectedVersion)
            .GreaterThan(0u);
    }
}