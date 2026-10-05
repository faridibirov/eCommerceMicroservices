using eCommerce.BusinessLogicLayer.DTO;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using FluentValidation;

namespace eCommerce.ProductsMicroService.API.APIEndpoints;

public static class ProductAPIEndpoints
{
    public static IEndpointRouteBuilder MapProductAPIEndpoints(this IEndpointRouteBuilder app)
    {
        //GET /api/products
        app.MapGet("/api/products", async (IProductsService productService) =>
        {
            List<ProductResponse?> products = await productService.GetProducts();

            return Results.Ok(products);
        });

        //GET /api/products/search/product-id/guid
        app.MapGet("/api/products/search/product-id/{ProductID:guid}", async (IProductsService productService, Guid ProductID) =>
        {
            ProductResponse? product = await productService.GetProductByCondition(temp => temp.ProductID == ProductID);

            return Results.Ok(product);
        });

        //GET /api/products/search/searchstring
        app.MapGet("/api/products/search/{SearchString}", async (IProductsService productService, string SearchString) =>
        {
            List<ProductResponse?> productsByProductName = await productService.GetProductsByCondition(temp =>
            temp.ProductName != null && temp.ProductName.Contains(SearchString, StringComparison.OrdinalIgnoreCase));

            List<ProductResponse?> productsByCategory = await productService.GetProductsByCondition(temp =>
            temp.Category != null && temp.Category.Contains(SearchString, StringComparison.OrdinalIgnoreCase));

            var products = productsByProductName.Union(productsByCategory);

            return Results.Ok(products);
        });

        //POST /api/products
        app.MapPost("/api/products", async (IProductsService productService, ProductAddRequest productAddRequest, IValidator<ProductAddRequest> productAddRequestValidator) =>
        {
            var validationResult = await productAddRequestValidator.ValidateAsync(productAddRequest);

            if (!validationResult.IsValid)
            {
                Dictionary<string, string[]> errors = validationResult.Errors.GroupBy(temp =>
                 temp.PropertyName).ToDictionary(grp => grp.Key, grp => grp.Select(temp => temp.ErrorMessage).ToArray());
                return Results.ValidationProblem(errors);
            }

            ProductResponse? addedProduct = await productService.AddProduct(productAddRequest);
            if (addedProduct != null)
            {
                return Results.Created($"/api/products/serach/product-id/{addedProduct.ProductID}", addedProduct);
            }
            else
            {
                return Results.Problem("Error while adding the product. Please try again later.");
            }
        });

        //PUT /api/products
        app.MapPut("/api/products", async (IProductsService productService, ProductUpdateRequest productUpdateRequest, IValidator<ProductUpdateRequest> productUpdateRequestValidator) =>
        {
            var validationResult = await productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

            if (!validationResult.IsValid)
            {
                Dictionary<string, string[]> errors = validationResult.Errors.GroupBy(temp =>
                 temp.PropertyName).ToDictionary(grp => grp.Key, grp => grp.Select(temp => temp.ErrorMessage).ToArray());
                return Results.ValidationProblem(errors);
            }

            ProductResponse? updatedProduct = await productService.UpdateProduct(productUpdateRequest);
            if (updatedProduct != null)
            {
                return Results.Ok(updatedProduct);
            }
            else
            {
                return Results.Problem("Error while updating the product. Please try again later.");
            }
        });


        //GET /api/products/guid
        app.MapDelete("/api/products/{ProductID}", async (IProductsService productService, Guid ProductID) =>
        {
            var result = await productService.DeleteProduct(ProductID);

            if (result)
                return Results.Ok(true);
            else
                return Results.Problem("Error while deleting the product. Please try again later.");
        
        });

        return app;
    }
}
