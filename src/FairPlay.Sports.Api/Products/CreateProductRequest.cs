namespace FairPlay.Sports.Api.Products;

public sealed record CreateProductRequest(string Name, string Description, decimal Price, int Stock);
