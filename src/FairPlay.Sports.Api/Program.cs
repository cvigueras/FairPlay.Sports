using System.Text;
using System.Text.Json.Serialization;
using FairPlay.Sports.Api.OpenApi;
using FairPlay.Sports.Application;
using FairPlay.Sports.Infrastructure;
using FairPlay.Sports.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

const string FrontendCorsPolicy = "FrontendCorsPolicy";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

// Hexagonal wiring: the Api (driving adapter) composes Application (use cases)
// and Infrastructure (driven adapters), never the other way around.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection["SigningKey"]
    ?? throw new InvalidOperationException("Jwt:SigningKey is not configured. Set it via user-secrets or an environment variable.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "unique_name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();

// Origins allowed to call the API with credentials. Defaults cover the Vite dev
// server on both hostnames (localhost and 127.0.0.1 are distinct origins to the
// browser); override via `Cors:AllowedOrigins` in config for other setups.
string[] corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    is { Length: > 0 } configured
    ? configured
    : ["http://localhost:5173", "http://127.0.0.1:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            // Required so the browser sends and stores the refresh-token cookie.
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Local convenience: bring the database up to the latest migration on startup.
    // Production applies migrations as an explicit deploy step, never here.
    await app.Services.MigrateAsync();

    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "FairPlay.Sports API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposed so WebApplicationFactory<Program> can be used from integration tests.
public partial class Program
{
}
