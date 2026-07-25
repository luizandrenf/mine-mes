using Microsoft.EntityFrameworkCore;
using MiniMes.Production.Application.Abstractions;
using MiniMes.Production.Application.ProductionOrders;
using MiniMes.Production.Application.Products;
using MiniMes.Production.Data;
using MiniMes.Production.Infrastructure.Persistence;
using MiniMes.Production.Infrastructure.Repositories;
using MiniMes.Production.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();

string? connectionString =
    builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

builder.Services.AddDbContext<MiniMesDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductionOrderService, ProductionOrderService>();

builder.Services.AddHealthChecks().AddDbContextCheck<MiniMesDbContext>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();
