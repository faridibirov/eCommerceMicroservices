using eCommerce.Infrastructure;
using eCommerce.Core;
using eCommerce.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

//Add Infrastructure services
builder.Services.AddInfrastructure();
builder.Services.AddCore();

//Add controllers to the service collection
builder.Services.AddControllers();

var app = builder.Build();
app.UseExceptionHandlingMiddleware();
//Routing
app.UseRouting();

//Auth
app.UseAuthentication();
app.UseAuthorization();

//Controller routes
app.MapControllers();
app.Run();
