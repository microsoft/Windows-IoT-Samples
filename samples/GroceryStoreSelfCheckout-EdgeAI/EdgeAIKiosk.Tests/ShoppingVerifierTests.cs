using EdgeAI_ObjectDetection.Controls;
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
    public void Verify_ReportsExactMismatches(
        string[] scannedLabels, string[] detectedLabels, string[] expectedMismatches)
    {
        List<ScannedItem> scanned = scannedLabels.Select(label => new ScannedItem(label, label, 1)).ToList();
        List<PreviewDetection> detected = detectedLabels.Select(label => new PreviewDetection(0, label, 0.9f, default)).ToList();

        VerificationResult result = ShoppingVerifier.Verify(scanned, detected, new HashSet<string> { "apple", "banana" });

        Assert.Equal(expectedMismatches.Length == 0, result.IsMatch);
        Assert.Equal(
            expectedMismatches.OrderBy(label => label, StringComparer.Ordinal),
            result.Mismatches.OrderBy(label => label, StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("APPLE", "apple", "Apple")]
    [InlineData("banana", "BANANA", "Banana")]
    public void Verify_UsesExplicitSelectionCaseInsensitively(string scanned, string detected, string selected)
    {
        VerificationResult result = ShoppingVerifier.Verify([new(scanned, scanned, 1)],
            [new(0, detected, 0.9f, default), new(1, "person", 0.9f, default)],
            new HashSet<string> { selected });
        Assert.True(result.IsMatch);
        Assert.Single(result.DetectedItems);
    }

    [Fact]
    public void Verify_EmptySelectionIgnoresAllLabels()
    {
        VerificationResult result = ShoppingVerifier.Verify([new("apple", "apple", 1)],
            [new(0, "banana", 0.9f, default)], new HashSet<string>());
        Assert.True(result.IsMatch);
        Assert.Empty(result.DetectedItems);
    }

    [Fact]
    public void Verify_UsesLabelSetsNotQuantitiesAndCopiesCart()
    {
        List<ScannedItem> scanned = new() { new("apple", "apple", 3) };
        VerificationResult result = ShoppingVerifier.Verify(scanned,
            [new(0, "apple", 0.9f, default), new(0, "apple", 0.8f, default)],
            new HashSet<string> { "apple" });
        scanned.Clear();
        Assert.True(result.IsMatch);
        Assert.Single(result.ScannedItems);
        Assert.Equal(2, result.DetectedItems.Count);
    }

    [Fact]
    public void Verify_LaterStreamUpdatesDoNotChangeCheckoutResult()
    {
        List<PreviewDetection> detected = new() { new(0, "apple", 0.9f, default) };
        VerificationResult result = ShoppingVerifier.Verify([new("apple", "apple", 1)],
            detected, new HashSet<string> { "apple", "banana" });

        detected.Clear();
        detected.Add(new(1, "banana", 0.9f, default));

        Assert.True(result.IsMatch);
        Assert.Empty(result.Mismatches);
        Assert.Equal("apple", Assert.Single(result.DetectedItems).Label);
    }
}
