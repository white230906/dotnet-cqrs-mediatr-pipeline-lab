using DotNetCqrsMediatRPipelineLab.Contract.WorkItems;
using FluentValidation;

namespace DotNetCqrsMediatRPipelineLab.Application.WorkItems.Validators;

public class ChangeWorkItemStatus : AbstractValidator<Command.ChangeWorkItemStatus>
{
    public ChangeWorkItemStatus()
    {
        RuleFor(command => command.Id)
            .NotEmpty();

        RuleFor(command => command.Status)
            .IsInEnum();

        RuleFor(command => command.ExpectedVersion)
            .GreaterThan(0u);
    }
}