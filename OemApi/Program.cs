using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ProjArqsi.Auth.Common;
using ProjArqsi.OemApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger/OpenAPI configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OEM API - Operations & Execution Management",
        Version = "v1",
        Description = "API for managing Operation Plans and execution workflows"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure Database
builder.Services.AddDbContext<OemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OemDatabase")));

// Configure JWT Authentication (using shared Auth.Common)
builder.Services.AddJwtAuthentication(builder.Configuration);

// Configure Authorization with Role-based policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LogisticOperator", policy =>
        policy.RequireRole("LogisticOperator"));
    
    options.AddPolicy("PortAuthority", policy =>
        policy.RequireRole("PortAuthority"));
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSPA", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OEM API v1");
    });
}

app.UseCors("AllowSPA");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("✓ OEM API is ready");
Console.WriteLine($"  Swagger: http://localhost:5003/swagger");
Console.WriteLine($"  Health: http://localhost:5003/api/oem/health");

app.Run();
