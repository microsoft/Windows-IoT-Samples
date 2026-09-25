using System.Collections.Generic;

namespace EdgeAIKiosk.Models;

public record VerificationResult(
    bool IsMatch,
    IReadOnlyList<ScannedItem> ScannedItems,
    IReadOnlyList<DetectedItem> DetectedItems,
    IReadOnlyList<string> Mismatches);
