using AutoMapper;
using BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;

public class OrdersService : IOrdersService
{
    private readonly IValidator<OrderAddRequest> _orderAddRequestValidator;
    private readonly IValidator<OrderUpdateRequest> _orderUpdateRequestValidator;
    private readonly IValidator<OrderItemAddRequest> _orderItemAddRequestValidator;
    private readonly IValidator<OrderItemUpdateRequest> _orderItemUpdateRequestValidator;
    private readonly IMapper _mapper;
    private readonly IOrdersRepository _orderRepository;
    private UsersMicroserviceClient _usersMicroserviceClient;
    private ProductsMicroserviceClient _productsMicroserviceClient;

    public OrdersService(IValidator<OrderAddRequest> orderAddRequestValidator, IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
        IValidator<OrderItemAddRequest> orderItemAddRequestValidator, IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator,
        IMapper mapper, IOrdersRepository orderRepository, UsersMicroserviceClient usersMicroserviceClient, ProductsMicroserviceClient productsMicroserviceClient)
    {
        _orderAddRequestValidator = orderAddRequestValidator;
        _orderUpdateRequestValidator = orderUpdateRequestValidator;
        _orderItemAddRequestValidator = orderItemAddRequestValidator;
        _orderItemUpdateRequestValidator = orderItemUpdateRequestValidator;
        _mapper = mapper;
        _orderRepository = orderRepository;
        _usersMicroserviceClient = usersMicroserviceClient;
        _productsMicroserviceClient = productsMicroserviceClient;
    }

    public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
    {
        if (orderAddRequest == null)
        {
            throw new ArgumentNullException(nameof(orderAddRequest));
        }
        ValidationResult orderAddRequestValidationResult = await _orderAddRequestValidator.ValidateAsync(orderAddRequest);

        if (!orderAddRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        List<ProductDTO> products = new List<ProductDTO>();

        foreach (OrderItemAddRequest orderItemAddRequest in orderAddRequest.OrderItems)
        {
            ValidationResult orderItemAddRequestValidationResult = await _orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);

            if (!orderItemAddRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }
            ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemAddRequest.ProductID);
            if (product == null)
            {
                throw new ArgumentException("Invalid Product ID");
            }

            products.Add(product);
        }

        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderAddRequest.UserID);
        if (user == null)
        {
            throw new ArgumentException("Invalid User ID");
        }

        Order orderInput = _mapper.Map<Order>(orderAddRequest);

        foreach(var orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
        }

        orderInput.TotalBill = orderInput.OrderItems.Sum(x => x.TotalPrice);

        Order? addedOrder = await _orderRepository.AddOrder(orderInput);

        if (addedOrder == null)
        {
            return null;
        }

        OrderResponse addedOrderResponse = _mapper.Map<OrderResponse>(addedOrder);

        if (addedOrderResponse != null)
        {
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, addedOrderResponse);
            }

            foreach (OrderItemResponse orderItemResponse in addedOrderResponse.OrderItems)
            {
                ProductDTO? product = products.Where(x => x.ProductID == orderItemResponse.ProductID).FirstOrDefault();

                if (product == null)
                {
                    continue;
                }
                _mapper.Map<ProductDTO, OrderItemResponse>(product, orderItemResponse);
            }
        }

        return addedOrderResponse;
    }
    public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
    {
        if (orderUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(orderUpdateRequest));
        }
        ValidationResult orderUpdateRequestValidationResult = await _orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);

        if (!orderUpdateRequestValidationResult.IsValid)
        {
            string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        List<ProductDTO> products = new List<ProductDTO>();

        foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
        {
            ValidationResult orderItemUpdateRequestValidationResult = await _orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);

            if (!orderItemUpdateRequestValidationResult.IsValid)
            {
                string errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
                throw new ArgumentException(errors);
            }
            ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemUpdateRequest.ProductID);
            if (product == null)
            {
                throw new ArgumentException("Invalid Product ID");
            }
            products.Add(product);
        }


        UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderUpdateRequest.UserID);
        if (user == null)
        {
            throw new ArgumentException("Invalid User ID");
        }

        Order orderInput = _mapper.Map<Order>(orderUpdateRequest);

        foreach (var orderItem in orderInput.OrderItems)
        {
            orderItem.TotalPrice = orderItem.UnitPrice * orderItem.Quantity;
        }

        orderInput.TotalBill = orderInput.OrderItems.Sum(x => x.TotalPrice);

        Order? updatedOrder = await _orderRepository.UpdateOrder(orderInput);

        if (updatedOrder == null)
        {
            return null;
        }

        OrderResponse updatedOrderResponse = _mapper.Map<OrderResponse>(updatedOrder);

        if (updatedOrderResponse != null)
        {
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, updatedOrderResponse);
            }

            foreach (OrderItemResponse orderItemResponse in updatedOrderResponse.OrderItems)
            {
                ProductDTO? product = products.Where(x => x.ProductID == orderItemResponse.ProductID).FirstOrDefault();

                if (product == null)
                {
                    continue;
                }
                _mapper.Map<ProductDTO, OrderItemResponse>(product, orderItemResponse);
            }
        }

        return updatedOrderResponse;
    }


    public async Task<List<OrderResponse?>> GetOrders()
    {
        IEnumerable<Order?> orders = await _orderRepository.GetOrders();

        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);

        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);

                if (product == null)
                {
                    continue;
                }
                _mapper.Map<ProductDTO, OrderItemResponse>(product, orderItemResponse);
            }

            UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }

        }
        return orderResponses.ToList();
    }
    public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        IEnumerable<Order?> orders = await _orderRepository.GetOrdersByCondition(filter);

        IEnumerable<OrderResponse?> orderResponses = _mapper.Map<IEnumerable<OrderResponse>>(orders);

        foreach (OrderResponse? orderResponse in orderResponses)
        {
            if (orderResponse == null)
            {
                continue;
            }

            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);

                if (product == null)
                {
                    continue;
                }
                _mapper.Map<ProductDTO, OrderItemResponse>(product, orderItemResponse);
            }
            UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }
        }

        return orderResponses.ToList();
    }
    public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        Order order = await _orderRepository.GetOrderByCondition(filter);

        if (order == null)
        {
            return null;
        }

        OrderResponse orderResponse = _mapper.Map<OrderResponse>(order);

        if (orderResponse != null)
        {
            foreach (OrderItemResponse orderItemResponse in orderResponse.OrderItems)
            {
                ProductDTO? product = await _productsMicroserviceClient.GetProductByProductID(orderItemResponse.ProductID);

                if (product == null)
                {
                    continue;
                }
                _mapper.Map<ProductDTO, OrderItemResponse>(product, orderItemResponse);
            }
            UserDTO? user = await _usersMicroserviceClient.GetUserByUserID(orderResponse.UserID);
            if (user != null)
            {
                _mapper.Map<UserDTO, OrderResponse>(user, orderResponse);
            }
        }

        return orderResponse;
    }


    public async Task<bool> DeleteOrder(Guid orderId)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderId);

        Order? existingOrder = await _orderRepository.GetOrderByCondition(filter);

        if (existingOrder == null)
        {
            return false;
        }

        var result = await _orderRepository.DeleteOrder(orderId);
        return result;
    }
}
