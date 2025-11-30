using ProjArqsi.Application.Services;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Infrastructure.Repositories;
using ProjArqsi.Infrastructure.Shared;
using ProjArqsi.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add EF Core DbContext for SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories
builder.Services.AddScoped<IVesselTypeRepository, VesselTypeRepository>();
builder.Services.AddSingleton<IUnitOfWork, UnitOfWork>();

// Register application services
builder.Services.AddScoped<VesselTypeService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
