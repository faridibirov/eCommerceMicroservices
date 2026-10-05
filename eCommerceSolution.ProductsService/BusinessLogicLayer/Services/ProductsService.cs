
using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;

namespace BusinessLogicLayer.Services;

public class ProductsService : IProductsService
{
    private readonly IValidator<ProductAddRequest> _productAddRequestValidator;
    private readonly IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
    private readonly IMapper _mapper;
    private readonly IProductsRepository _productsRepository;

    public ProductsService(IValidator<ProductAddRequest> productAddRequestValidator, IValidator<ProductUpdateRequest> productUpdateRequestValidator,
         IMapper mapper, IProductsRepository productsRepository)
    {
        _productAddRequestValidator = productAddRequestValidator;
        _productUpdateRequestValidator = productUpdateRequestValidator;
        _mapper = mapper;
        _productsRepository = productsRepository;

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
        return productResponses;
    }
    public async Task<List<ProductResponse?>> GetProductsByCondition(Expression<Func<Product, bool>> conditionExpression)
    {
        IEnumerable<Product?> products = await _productRepository.GetProductsByCondition(conditionExpression);

        if (!products.Any())
        {
            return null;
        }

        IEnumerable<ProductResponse?> productResponse = _mapper.Map<IEnumerable<ProductResponse>>(products);
        return productResponse;
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


    public Task<ProductResponse?> UpdateProduct(ProductUpdateRequest productUpdateRequest)
    {
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

        Product? updatedProduct = await _productRepository.UpdateProduct(productInput);

        if (updatedProduct == null)
        {
            return null;
        }

        ProductResponse updatedProductResponse = _mapper.Map<ProductResponse>(updatedProduct);

        return updatedProductResponse;
    }

    public async Task<bool> DeleteProduct(Guid productId)
    {
        if (productId == Guid.Empty)
        {
            return false;
        }

        var result = await _productRepository.DeleteProduct(productId);
        return result;
    }
}
