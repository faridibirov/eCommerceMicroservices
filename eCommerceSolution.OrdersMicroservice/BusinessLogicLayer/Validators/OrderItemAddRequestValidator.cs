using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using FluentValidation;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;

public class OrderItemAddRequestValidator:     AbstractValidator<OrderItemAddRequest>
{
    public OrderItemAddRequestValidator()
    {
        RuleFor(x => x.ProductID)
            .NotEmpty().WithMessage("Product ID can't be blank");

        RuleFor(x => x.UnitPrice)
            .NotEmpty().WithMessage("Unit Price can't be blank")
            .GreaterThan(0).WithMessage("Unit Price can't less than or equal to zero");

        RuleFor(x => x.Quantity)
            .NotEmpty().WithMessage("Quantity can't be blank")
            .GreaterThan(0).WithMessage("Quantity can't less than or equal to zero");
    }
}
