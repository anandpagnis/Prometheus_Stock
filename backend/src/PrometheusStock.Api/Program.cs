using PrometheusStock.Api.Common;
using PrometheusStock.Api.Extensions;
using PrometheusStock.Api.Features.Intraday;

const string FrontendCorsPolicy = "frontend";

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Domain exceptions -> problem+json (see ProblemDetailsExceptionHandler).
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

builder.Services.AddCors(options => options.AddPolicy(
    FrontendCorsPolicy,
    policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()));

// Market-data slice: pure intraday aggregator + Yahoo-backed IStockDataProvider (typed HttpClient).
builder.Services.AddMarketData(builder.Configuration);

var app = builder.Build();

// First in the pipeline so every downstream failure becomes a problem+json response.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(FrontendCorsPolicy);

app.MapIntradayEndpoints();

// Liveness probe. Deliberately dependency-free so it stays cheap and always available.
app.MapGet("/health", () => TypedResults.Ok(new { status = "healthy" }))
   .WithName("Health");

app.Run();

// Exposed so the test project can bootstrap the app with WebApplicationFactory<Program>.
public partial class Program;
