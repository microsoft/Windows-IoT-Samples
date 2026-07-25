using System.Collections.Generic;

namespace EdgeAIKiosk1.Models;

public record VerificationResult(
    bool IsMatch,
    IReadOnlyList<ScannedItem> ScannedItems,
    IReadOnlyList<DetectedItem> DetectedItems,
    IReadOnlyList<string> Mismatches);
