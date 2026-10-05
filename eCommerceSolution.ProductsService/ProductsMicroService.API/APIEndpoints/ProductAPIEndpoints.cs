namespace eCommerce.ProductsMicroService.API.APIEndpoints;

public static class ProductAPIEndpoints
{
    public static IEndpointRouteBuilder MapProductAPIEndpoints(this IEndpointRouteBuilder app)
    {
        //GET /api/products
        app.MapGet("/api/products", async (IProductService productService) =>
        {
            List<ProductResponse?> products = await productService.GetProducts();

            return Results.Ok(products);
        });

        //GET /api/products/search/product-id/guid
        app.MapGet("/api/products/search/product-id/{ProductID}", async (IProductService productService, Guid ProductID) =>
        {
            ProductResponse? product = await productService.GetProductByCondition(temp=>temp.ProductID == ProductID);

            return Results.Ok(product);
        });

        //GET /api/products/search/searchstring
        app.MapGet("/api/products/search/{SearchString}", async (IProductService productService, string SearchString) =>
        {
            List<ProductResponse?> products = await productService.GetProductsByCondition(temp => temp.ProductName.Contains(SearchString) || temp.Category.Contains(SearchString));

            return Results.Ok(products);
        });

        //POST /api/products
        app.MapPost("/api/products", async (IProductService productService, ProductAddRequest productAddRequest) =>
        {
            ProductResponse? product = await productService.AddProduct(productAddRequest);

            return Results.Ok(product);
        });

        //PUT /api/products
        app.MapPut("/api/products", async (IProductService productService, ProductUpdateRequest productUpdateRequest) =>
        {
            ProductResponse? product = await productService.UpdateProduct(productUpdateRequest);

            return Results.Ok(product);
        });


        //GET /api/products/guid
        app.MapDelete("/api/products/{ProductID}", async (IProductService productService, Guid ProductID) =>
        {
            var result  = await productService.DeleteProduct(ProductID);

            return Results.Ok(result);
        });

        return app;
    }
}
