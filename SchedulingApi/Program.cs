using ProjArqsi.Auth.Common;
using ProjArqsi.SchedulingApi.Services;
using ProjArqsi.SchedulingApi.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure CORS for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// Add JWT authentication using shared library
builder.Services.AddJwtAuthentication(builder.Configuration);

// Configure HttpClient for Core API
var coreApiBaseUrl = builder.Configuration["CoreApiSettings:BaseUrl"] 
    ?? throw new InvalidOperationException("CoreApiSettings:BaseUrl is not configured.");

builder.Services.AddHttpClient<ICoreApiClient, CoreApiClientService>(client =>
{
    client.BaseAddress = new Uri(coreApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add scheduling engine service
builder.Services.AddScoped<ISchedulingEngine, SchedulingEngineService>();

// Register logging classes
builder.Services.AddScoped<ISchedulingLogger, SchedulingLogger>();

// Add controllers
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ProjArqsi Scheduling API", Version = "v1" });
    
    // Configure Bearer token authentication in Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Use CORS before authentication
app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("\n✓ Scheduling API is ready\n");

app.Run();
