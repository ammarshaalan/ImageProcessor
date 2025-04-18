using ImageProcessor.Services;
using ImageProcessor.Middleware;
using ImageProcessor.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddResponseCompression();

// Add health checks
builder.Services.AddHealthChecks()
    .AddDiskStorageHealthCheck(options => options.AddDrive("C:\\", 1024)) // Check disk space
    .AddCheck<ImageServiceHealthCheck>("image_service");

// Add memory cache
builder.Services.AddMemoryCache();

// Add rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Register our services
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IExifService, ExifService>();

// Configure request size limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

// Add request logging
app.UseMiddleware<RequestLoggingMiddleware>();

// Add compression
app.UseResponseCompression();

// Add rate limiting
app.UseRateLimiter();

// Add CORS
app.UseCors();

app.UseHttpsRedirection();
app.UseAuthorization();

// Add health check endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.MapControllers();

// Create storage directory if it doesn't exist
var storageDir = Path.Combine(app.Environment.ContentRootPath, "storage");
Directory.CreateDirectory(storageDir);

app.Run();