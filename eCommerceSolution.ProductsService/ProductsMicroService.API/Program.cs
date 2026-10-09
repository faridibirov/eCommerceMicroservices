using eCommerce.ProductsService.BusinessLogicLayer;
using eCommerce.ProductsService.DataAccessLayer;
using eCommerceSolution.ProductsMicroService.API.Middleware;
using FluentValidation.AspNetCore;
using eCommerce.ProductsMicroService.API.APIEndpoints;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//Add DAL and BLL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer();


builder.Services.AddControllers();


//FluentValidations
builder.Services.AddFluentValidationAutoValidation();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Cors
builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

//Cors
app.UseCors();

//Swagger
app.UseSwagger();
app.UseSwaggerUI();

//Routing
app.UseRouting();

//Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//Controller routes
app.MapControllers();
app.MapProductAPIEndpoints();
app.Run();

