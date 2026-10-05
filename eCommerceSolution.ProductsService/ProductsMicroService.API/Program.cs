using eCommerce.ProductsService.BusinessLogicLayer;
using eCommerce.ProductsService.DataAccessLayer;
using eCommerceSolution.ProductsMicroService.API.Middleware;
using FluentValidation.AspNetCore;
using eCommerce.ProductsMicroService.API.APIEndpoints;

var builder = WebApplication.CreateBuilder(args);

//Add DAL and BLL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer();


builder.Services.AddControllers();


//FluentValidations
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

//Routing
app.UseRouting();

//Auth
app.UseAuthentication();
app.UseAuthorization();

//Controller routes
app.MapControllers();
app.MapProductAPIEndpoints();
app.Run();

