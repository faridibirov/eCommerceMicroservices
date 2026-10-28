
using System.Linq.Expressions;
using AutoMapper;
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.DataAccessLayer.Entities;
using eCommerce.DataAccessLayer.RepositoryContracts;
using eCommerce.ProductsService.BusinessLogicLayer.RabbitMQ;
using FluentValidation;
using FluentValidation.Results;

namespace eCommerce.BusinessLogicLayer.Services;

public class ProductsService : IProductsService
{
    private readonly IValidator<ProductAddRequest> _productAddRequestValidator;
    private readonly IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
    private readonly IMapper _mapper;
    private readonly IProductsRepository _productRepository;
    private readonly IRabbitMQPublisher _rabbitMQPublisher;

    public ProductsService(IValidator<ProductAddRequest> productAddRequestValidator, IValidator<ProductUpdateRequest> productUpdateRequestValidator,
         IMapper mapper, IProductsRepository productRepository, IRabbitMQPublisher rabbitMQPublisher)
    {
        _productAddRequestValidator = productAddRequestValidator;
        _productUpdateRequestValidator = productUpdateRequestValidator;
        _mapper = mapper;
        _productRepository = productRepository;
        _rabbitMQPublisher = rabbitMQPublisher;
    }

    public async Task<ProductResponse?> AddProduct(ProductAddRequest productAddRequest)
    {
        if (productAddRequest == null)
        {
            throw new ArgumentNullException(nameof(productAddRequest));
        }
        ValidationResult validationResult = await _productAddRequestValidator.ValidateAsync(productAddRequest);

        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        Product productInput = _mapper.Map<Product>(productAddRequest);

        Product? addedProduct = await _productRepository.AddProduct(productInput);

        if (addedProduct == null)
        {
            return null;
        }

        ProductResponse addedProductResponse = _mapper.Map<ProductResponse>(addedProduct);

        return addedProductResponse;
    }

    public async Task<List<ProductResponse?>> GetProducts()
    {
        IEnumerable<Product?> products = await _productRepository.GetProducts();

        if (!products.Any())
        {
            return null;
        }

        IEnumerable<ProductResponse?> productResponses = _mapper.Map<IEnumerable<ProductResponse>>(products);
        return productResponses.ToList();
    }
    public async Task<List<ProductResponse?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        IEnumerable<Product?> products = await _productRepository.GetProductsByCondition(conditionExpression);

        IEnumerable<ProductResponse?> productResponse = _mapper.Map<IEnumerable<ProductResponse>>(products);
        return productResponse.ToList();
    }
    public async Task<ProductResponse?> GetProductByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        Product product = await _productRepository.GetProductByCondition(conditionExpression);

        if (product == null)
        {
            return null;
        }

        ProductResponse productResponse = _mapper.Map<ProductResponse>(product);
        return productResponse;
    }


    public async Task<ProductResponse?> UpdateProduct(ProductUpdateRequest productUpdateRequest)
    {
        Product? existingProduct = await _productRepository.GetProductByCondition(temp => temp.ProductID == productUpdateRequest.ProductID);

        if (existingProduct == null)
        {
            throw new ArgumentException("Invalid Product ID");
        }


        if (productUpdateRequest == null)
        {
            throw new ArgumentNullException(nameof(productUpdateRequest));
        }
        ValidationResult validationResult = await _productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

        if (!validationResult.IsValid)
        {
            string errors = string.Join(", ", validationResult.Errors.Select(temp => temp.ErrorMessage));
            throw new ArgumentException(errors);
        }

        Product productInput = _mapper.Map<Product>(productUpdateRequest);

        //check if prodcut name is changed
       // bool isProductNameChanged = productUpdateRequest.ProductName != existingProduct.ProductName;

        Product? updatedProduct = await _productRepository.UpdateProduct(productInput);

        // string routingKey = "product.update";
        var headers = new Dictionary<string, object>()
            {
                {"event", "product.update" },
                {"RowCount", 1 }
            };

        _rabbitMQPublisher.Publish<Product>(headers, productInput);

        ProductResponse updatedProductResponse = _mapper.Map<ProductResponse>(updatedProduct);

        return updatedProductResponse;
    }

    public async Task<bool> DeleteProduct(Guid productId)
    {
        Product? existingProduct = await _productRepository.GetProductByCondition(temp => temp.ProductID == productId);

        if (existingProduct == null)
        {
            return false;
        }

        var result = await _productRepository.DeleteProduct(productId);
        if (result)
        {
            //string routingKey = "product.delete";

            var headers = new Dictionary<string, object>()
            {
                {"event", "product.delete" },
                {"RowCount", 1 }
            };

            _rabbitMQPublisher.Publish<Product>(headers, existingProduct);
        }

        return result;
    }
}
