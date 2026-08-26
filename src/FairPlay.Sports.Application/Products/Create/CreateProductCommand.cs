namespace FairPlay.Sports.Application.Products.Create;

public sealed record CreateProductCommand(string Name, string Description, decimal Price, int Stock);
