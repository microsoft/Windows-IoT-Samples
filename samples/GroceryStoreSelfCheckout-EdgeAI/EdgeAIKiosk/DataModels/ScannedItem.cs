namespace EdgeAIKiosk.Models;

public class ScannedItem
{
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }

    public ScannedItem(string barcode, string name, int quantity) =>
        (Barcode, Name, Quantity) = (barcode, name, quantity);

    public override string ToString() => Name;
}
