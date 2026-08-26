using FairPlay.Sports.Application;
using FairPlay.Sports.Infrastructure;

const string FrontendCorsPolicy = "FrontendCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Hexagonal wiring: the Api (driving adapter) composes Application (use cases)
// and Infrastructure (driven adapters), never the other way around.
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed so WebApplicationFactory<Program> can be used from integration tests.
public partial class Program
{
}
