static class Menu
{
    public static readonly Dictionary<string, decimal> Prices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["burger"] = 5.99m,
        ["fries"] = 2.99m,
        ["drink"] = 1.99m,
        ["coffee"] = 2.49m
    };
}
