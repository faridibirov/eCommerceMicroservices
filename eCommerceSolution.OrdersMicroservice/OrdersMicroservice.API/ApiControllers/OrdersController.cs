using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
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

    [HttpGet("/search/productid/{orderID}")]
    public async Task<OrderResponse?> GetOrderByOrderID(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        var order = await _ordersService.GetOrderByCondition(filter);
        return order;
    }

    [HttpGet("/search/productid/{productID}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrdersByProductID(Guid productID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.ElemMatch(temp => temp.OrderItems, 
            Builders<OrderItem>.Filter.Eq(tempProdcut => tempProdcut.ProductID, productID));
        var orders = await _ordersService.GetOrdersByCondition(filter);
        return orders;
    }

    [HttpGet("/search/orderDate/{orderDate}")]
    public async Task<IEnumerable<OrderResponse?>> GetOrderByOrderDate(DateTime  orderDate)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderDate.ToString("yyyy-MM-dd"), orderDate.ToString("yyyy-MM-dd"));
        var orders = await _ordersService.GetOrdersByCondition(filter);
        return orders;
    }

}
