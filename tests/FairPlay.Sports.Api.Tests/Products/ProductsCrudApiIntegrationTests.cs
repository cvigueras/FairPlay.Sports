using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FairPlay.Sports.Application.Products;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

namespace FairPlay.Sports.Api.Tests.Products;

/// <summary>
/// End-to-end integration tests for the dummy Products CRUD: real HTTP pipeline,
/// real DI container, real ASP.NET Core routing/model-binding/validation - the only
/// "fake" piece is the in-memory repository adapter, by design.
/// A fresh WebApplicationFactory per test guarantees each test starts from the same
/// 5-item seeded dummy catalog, regardless of test order.
/// </summary>
[TestFixture]
public class ProductsCrudApiIntegrationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        // "Testing" (not the default "Development") so startup skips the dev-only
        // database auto-migration - this suite only exercises the in-memory Products slice.
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetAll_ReturnsTheFiveSeededDummyProducts()
    {
        var response = await _client.GetAsync("/api/products");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var products = await response.Content.ReadFromJsonAsync<List<ProductDto>>(JsonOptions);
        Assert.That(products, Has.Count.EqualTo(5));
    }

    [Test]
    public async Task GetById_WithSeededId_ReturnsThatProduct()
    {
        var all = await (await _client.GetAsync("/api/products"))
            .Content.ReadFromJsonAsync<List<ProductDto>>(JsonOptions);
        var seeded = all!.First();

        var response = await _client.GetAsync($"/api/products/{seeded.Id}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var found = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        Assert.That(found!.Id, Is.EqualTo(seeded.Id));
    }

    [Test]
    public async Task GetById_WithUnknownId_Returns404()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task FullCrudLifecycle_CreateReadUpdateDelete_WorksEndToEnd()
    {
        var createPayload = new { Name = "Casco de ciclismo", Description = "Casco ligero", Price = 59.99m, Stock = 25 };

        var createResponse = await _client.PostAsJsonAsync("/api/products", createPayload, JsonOptions);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        Assert.That(created!.Name, Is.EqualTo("Casco de ciclismo"));
        Assert.That(createResponse.Headers.Location, Is.Not.Null);

        var getResponse = await _client.GetAsync(createResponse.Headers.Location);
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var updatePayload = new { Name = "Casco de ciclismo Pro", Description = "Casco reforzado", Price = 69.99m, Stock = 20 };
        var updateResponse = await _client.PutAsJsonAsync($"/api/products/{created.Id}", updatePayload, JsonOptions);
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProductDto>(JsonOptions);
        Assert.That(updated!.Name, Is.EqualTo("Casco de ciclismo Pro"));

        var deleteResponse = await _client.DeleteAsync($"/api/products/{created.Id}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var afterDeleteResponse = await _client.GetAsync($"/api/products/{created.Id}");
        Assert.That(afterDeleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Create_WithInvalidData_ReturnsBadRequest()
    {
        var invalidPayload = new { Name = "", Description = "desc", Price = -1m, Stock = -1 };

        var response = await _client.PostAsJsonAsync("/api/products", invalidPayload, JsonOptions);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Update_WithUnknownId_Returns404()
    {
        var payload = new { Name = "X", Description = "desc", Price = 1m, Stock = 1 };

        var response = await _client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}", payload, JsonOptions);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_WithUnknownId_Returns404()
    {
        var response = await _client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
