using ProjArqsi.Application.Services;
using ProjArqsi.Domain.VesselTypeAggregate;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Infrastructure.VesselType;
using ProjArqsi.Infrastructure.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register repositories
builder.Services.AddSingleton<IVesselTypeRepository, VesselTypeRepository>();
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
