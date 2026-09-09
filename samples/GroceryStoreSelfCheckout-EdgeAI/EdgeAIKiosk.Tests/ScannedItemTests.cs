using EdgeAIKiosk.Models;

namespace EdgeAIKiosk.Tests;

public class ScannedItemTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var item = new ScannedItem("barcode42", "Milk", 2);
        Assert.Equal("barcode42", item.Barcode);
        Assert.Equal("Milk",      item.Name);
        Assert.Equal(2,           item.Quantity);
    }

    [Fact]
    public void Properties_AreSettable_AfterConstruction()
    {
        var item = new ScannedItem("b1", "apple", 1);
        item.Barcode  = "b2";
        item.Name     = "orange";
        item.Quantity = 5;
        Assert.Equal("b2",     item.Barcode);
        Assert.Equal("orange", item.Name);
        Assert.Equal(5,        item.Quantity);
    }

    [Fact]
    public void DefaultStringProperties_AreNotNull()
    {
        var item = new ScannedItem(string.Empty, string.Empty, 0);
        Assert.NotNull(item.Barcode);
        Assert.NotNull(item.Name);
    }
}
