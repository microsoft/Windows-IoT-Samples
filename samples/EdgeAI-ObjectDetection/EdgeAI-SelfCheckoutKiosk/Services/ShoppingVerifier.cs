using EdgeAI.ObjectDetection.Yolo;
using EdgeAI.SelfCheckoutKiosk.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EdgeAI.SelfCheckoutKiosk.Services;

public static class ShoppingVerifier
{
    public static VerificationResult Verify(IReadOnlyList<ScannedItem> scanned,
        IReadOnlyList<PreviewDetection> detected, IReadOnlySet<string> includedLabels)
    {
        HashSet<string> labels = new(includedLabels, StringComparer.OrdinalIgnoreCase);
        HashSet<string> scannedLabels = new(
            scanned.Select(item => item.Name).Where(labels.Contains), StringComparer.OrdinalIgnoreCase);
        PreviewDetection[] relevantDetections = detected.Where(item => labels.Contains(item.Label)).ToArray();
        HashSet<string> detectedLabels = new(
            relevantDetections.Select(item => item.Label), StringComparer.OrdinalIgnoreCase);
        string[] mismatches = scannedLabels.Except(detectedLabels, StringComparer.OrdinalIgnoreCase)
            .Concat(detectedLabels.Except(scannedLabels, StringComparer.OrdinalIgnoreCase)).ToArray();
        return new VerificationResult(mismatches.Length == 0, scanned.ToArray(),
            relevantDetections, mismatches);
    }
}
