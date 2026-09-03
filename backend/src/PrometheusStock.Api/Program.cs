using PrometheusStock.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Market-data slice: pure intraday aggregator + Yahoo-backed IStockDataProvider (typed HttpClient).
builder.Services.AddMarketData(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Liveness probe. Deliberately dependency-free so it stays cheap and always available.
app.MapGet("/health", () => TypedResults.Ok(new { status = "healthy" }))
   .WithName("Health");

app.Run();

// Exposed so the test project can bootstrap the app with WebApplicationFactory<Program>.
public partial class Program;
