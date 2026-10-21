using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

public record OrderResponse(
   Guid OrderID,
   Guid UserID,
   string? UserPersonName,
   string? Email,
   decimal TotalBill,
   DateTime OrderDate,
   List<OrderItemResponse> OrderItems)
{
    public OrderResponse() : this(default, default, default, default, default, default, default)
    {
    }
}
