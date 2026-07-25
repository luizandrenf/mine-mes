using Microsoft.EntityFrameworkCore;
using MiniMes.Api.Application.Abstractions;
using MiniMes.Api.Application.ProductionOrders;
using MiniMes.Api.Data;
using MiniMes.Api.Infrastructure.Persistence;
using MiniMes.Api.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

string? connectionString =
    builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("A connection string 'Postgres' não foi configurada");

builder.Services.AddDbContext<MiniMesDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// Scoped: uma instância por request HTTP. Repository e UnitOfWork recebem o MESMO
// MiniMesDbContext (também Scoped), então compartilham o change tracker na mesma request.
builder.Services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IProductionOrderService, ProductionOrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
