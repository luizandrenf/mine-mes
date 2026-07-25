using Microsoft.EntityFrameworkCore;
using MiniMes.Api.Data;

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
