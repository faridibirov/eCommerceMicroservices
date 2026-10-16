using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace OrdersMicroservice.API.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _ordersService;


    public OrdersController(IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    [HttpGet]
    public async Task<IEnumerable<OrderResponse?>> GetOrders()
    {
        var orders = await _ordersService.GetOrders();
        return orders;
    }

    [HttpGet("search/orderid/{orderID}")]
    public async Task<OrderResponse?> GetOrderByOrderID(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        var order = await _ordersService.GetOrderByCondition(filter);
        return order;
    }

    [HttpGet("search/productid/{productID}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByProductID(Guid productID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.ElemMatch(temp => temp.OrderItems,
            Builders<OrderItem>.Filter.Eq(tempProdcut => tempProdcut.ProductID, productID));
        var orders = await _ordersService.GetOrdersByCondition(filter);
        return orders;
    }

    [HttpGet("search/orderDate/{orderDate}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByOrderDate(DateTime orderDate)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderDate.ToString("yyyy-MM-dd"), orderDate.ToString("yyyy-MM-dd"));
        var orders = await _ordersService.GetOrdersByCondition(filter);
        return orders;
    }

    [HttpGet("search/userid/{userID}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByUserID(Guid userID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.UserID, userID);
        var orders = await _ordersService.GetOrdersByCondition(filter);
        return orders;
    }


    [HttpPost("/api/Orders")]
    public async Task<IActionResult> AddOrder(OrderAddRequest orderAddRequest)
    {

        if (orderAddRequest == null)
        {
            return BadRequest("Invalid order data");
        }

        OrderResponse? addedOrder = await _ordersService.AddOrder(orderAddRequest);
        if (addedOrder == null)
        {
            return Problem("Error while adding the order. Please try again later.");
        }

        return Created($"api/Orders/search/orderid/{addedOrder.OrderID}", addedOrder);

    }

    [HttpPut("/api/Orders/{orderID}")]
    public async Task<IActionResult> UpdateOrder(Guid orderID, OrderUpdateRequest orderUpdateRequest)
    {
        if (orderUpdateRequest == null)
        {
            return BadRequest("Invalid order data");
        }

        if (orderID != orderUpdateRequest.OrderID)
        {
            return BadRequest("OrderID in the URL does not match the OrderID in the request body.");
        }

        OrderResponse? updatedOrder = await _ordersService.UpdateOrder(orderUpdateRequest);
        if (updatedOrder == null)
        {
            return BadRequest("Error while updating the order. Please try again later.");
        }

        return Ok(updatedOrder);

    }

    [HttpDelete("/api/Orders/{orderID}")]
    public async Task<IActionResult> DeleteOrder(Guid OrderID)
    {
        if (OrderID == Guid.Empty)
        {
            return BadRequest("OrderID is required.");
        }

        bool result = await _ordersService.DeleteOrder(OrderID);
        if (!result)
        {
            return Problem("Error while deleting the order. Please try again later.");
        }
        return Ok(result);
    }
}
