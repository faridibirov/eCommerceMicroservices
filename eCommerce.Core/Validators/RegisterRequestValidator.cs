using eCommerce.Core.DTO;
using FluentValidation;

namespace eCommerce.Core.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        //Email
        RuleFor(temp => temp.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address format");

        //Password
        RuleFor(temp => temp.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password should be more than 6 characters");

        //PersonName
        RuleFor(temp => temp.PersonName)
            .NotEmpty().WithMessage("Person name is required")
            .Length(1, 50).WithMessage("Person name should be more than 1 and less than 50 characters");


        //Gender
        RuleFor(temp => temp.Gender)
            .IsInEnum().WithMessage("Invalid gender option");

    }
}
