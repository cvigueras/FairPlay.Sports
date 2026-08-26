namespace FairPlay.Sports.Domain.Products;

public sealed class Product
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    public Product(Guid id, string name, string description, decimal price, int stock)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Product id cannot be empty.", nameof(id));

        Id = id;
        Name = ValidateName(name);
        Description = description ?? string.Empty;
        Price = ValidatePrice(price);
        Stock = ValidateStock(stock);
    }

    public void UpdateDetails(string name, string description, decimal price, int stock)
    {
        Name = ValidateName(name);
        Description = description ?? string.Empty;
        Price = ValidatePrice(price);
        Stock = ValidateStock(stock);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));

        return name.Trim();
    }

    private static decimal ValidatePrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), price, "Product price cannot be negative.");

        return price;
    }

    private static int ValidateStock(int stock)
    {
        if (stock < 0)
            throw new ArgumentOutOfRangeException(nameof(stock), stock, "Product stock cannot be negative.");

        return stock;
    }
}
