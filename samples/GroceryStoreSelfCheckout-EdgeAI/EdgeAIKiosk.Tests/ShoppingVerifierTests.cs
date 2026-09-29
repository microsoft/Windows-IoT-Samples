using EdgeAIKiosk.Models;
using EdgeAIKiosk.Services;

namespace EdgeAIKiosk.Tests;

public class ShoppingVerifierTests
{
    public static TheoryData<string[], string[], string[]> VerificationCases => new()
    {
        { ["apple"], ["apple"], [] },
        { ["apple"], ["banana"], ["apple", "banana"] },
        { [], ["apple"], ["apple"] },
        { ["apple"], [], ["apple"] },
        { ["apple", "banana"], ["apple"], ["banana"] },
        { [], ["person"], [] },
        { ["person"], [], [] }
    };

    [Theory]
    [MemberData(nameof(VerificationCases))]
    public void BuildVerificationResult_ReportsExactMismatches(
        string[] scannedLabels, string[] detectedLabels, string[] expectedMismatches)
    {
        var scanned = scannedLabels.Select(label => new ScannedItem(label, label, 1)).ToList();
        var detected = detectedLabels.Select(label => new DetectedItem { Label = label, Confidence = 0.9f }).ToList();

        var result = ShoppingVerifier.BuildVerificationResult(scanned, detected);

        Assert.Equal(expectedMismatches.Length == 0, result.IsMatch);
        Assert.Equal(
            expectedMismatches.OrderBy(label => label, StringComparer.Ordinal),
            result.Mismatches.OrderBy(label => label, StringComparer.Ordinal));
    }
}
