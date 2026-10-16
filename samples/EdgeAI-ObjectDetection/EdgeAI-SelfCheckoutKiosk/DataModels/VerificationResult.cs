using EdgeAI.ObjectDetection.Yolo;
using System.Collections.Generic;

namespace EdgeAI.SelfCheckoutKiosk.Models;

public record VerificationResult(
    bool IsMatch,
    IReadOnlyList<ScannedItem> ScannedItems,
    IReadOnlyList<PreviewDetection> DetectedItems,
    IReadOnlyList<string> Mismatches);
