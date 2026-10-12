using System.Linq.Expressions;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;

namespace eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;

public interface IOrdersRepository
{
    Task<IEnumerable<Order>> GetOrderss();
    Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefiniton<Order> filter);
    Task<Order?> GetOrderByCondition(FilterDefiniton<Order> filter);
    Task<Order?> AddOrder(Order order);
    Task<Order?> UpdateOrder(Order Order);
    Task<bool> DeleteOrder(Guid orderID);
}
