using DotNetCqrsMediatRPipelineLab.Contract.WorkItems.Commands;
using FluentValidation;

namespace DotNetCqrsMediatRPipelineLab.Application.WorkItems.Validators;

public class ChangeWorkItemStatusCommandValidator : AbstractValidator<ChangeWorkItemStatusCommand>
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