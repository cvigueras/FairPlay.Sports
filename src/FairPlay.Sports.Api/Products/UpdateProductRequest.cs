namespace FairPlay.Sports.Api.Products;

public sealed record UpdateProductRequest(string Name, string Description, decimal Price, int Stock);
