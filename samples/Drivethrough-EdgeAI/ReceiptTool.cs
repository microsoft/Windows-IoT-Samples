using OpenAI.Chat;

sealed record ReceiptParams(string[] items);

static class ReceiptTool
{
public static string MakeReceipt(IEnumerable<string> items, Dictionary<string, decimal> prices){
    decimal total = 0;
    foreach (string item in items){
        total += prices[item];
    }
    return $"Total: {total}";
}

public static readonly ChatTool Definition = ChatTool.CreateFunctionTool(
    functionName: "make_receipt",
    functionDescription: "Add up items in an order to produce the final bill",
    functionParameters: BinaryData.FromString("""
    {
      "type": "object",
      "properties": {
        "items": {
          "type": "array",
          "items": { "type": "string" }
        }
      },
      "required": ["items"]
    }
    """));
}
