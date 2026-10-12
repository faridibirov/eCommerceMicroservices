using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;

public class OrderUpdateRequestValidator : AbstractValidator<OrderUpdateRequest>
{
    public OrderUpdateRequestValidator()
    {
        RuleFor(x => x.OrderID)
            .NotEmpty().WithMessage("Order ID can't be blank");

        RuleFor(x => x.UserID)
            .NotEmpty().WithMessage("User ID can't be blank");

        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage("Order Date can't be blank");

        RuleFor(x => x.OrderItems)
            .NotEmpty().WithMessage("Order Items can't be blank");
    }
}
