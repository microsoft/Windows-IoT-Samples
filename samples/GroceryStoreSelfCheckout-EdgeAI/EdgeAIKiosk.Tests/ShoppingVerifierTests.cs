using System.Collections.Generic;
using EdgeAIKiosk.Models;
using EdgeAIKiosk.Services;

namespace EdgeAIKiosk.Tests;

public class ShoppingVerifierTests
{
    [Fact]
    public void AllItemsMatch_IsMatchTrue_NoMismatches()
    {
        var scanned  = new List<ScannedItem> { new("b1", "apple", 1) };
        var detected = new List<DetectedItem> { new() { Label = "apple", Confidence = 0.9f } };

        var result = ShoppingVerifier.BuildVerificationResult(scanned, detected);

        Assert.True(result.IsMatch);
        Assert.Empty(result.Mismatches);
    }

    [Fact]
    public void DetectedItemNotInScanned_IsMatchFalse_MismatchListed()
    {
        var scanned  = new List<ScannedItem> { new("b1", "apple", 1) };
        var detected = new List<DetectedItem> { new() { Label = "banana", Confidence = 0.9f } };

        var result = ShoppingVerifier.BuildVerificationResult(scanned, detected);

        Assert.False(result.IsMatch);
        Assert.Contains("banana", result.Mismatches);
    }

    [Fact]
    public void ExtraDetectedItemWithEmptyCart_IsMatchFalse()
    {
        var scanned  = new List<ScannedItem>();
        var detected = new List<DetectedItem> { new() { Label = "apple", Confidence = 0.9f } };

        var result = ShoppingVerifier.BuildVerificationResult(scanned, detected);

        Assert.False(result.IsMatch);
        Assert.Contains("apple", result.Mismatches);
    }

    [Fact]
    public void ScannedItemNotDetected_IsMatchFalse_MismatchListed()
    {
        var scanned  = new List<ScannedItem> { new("b1", "apple", 1) };
        var detected = new List<DetectedItem>();

        var result = ShoppingVerifier.BuildVerificationResult(scanned, detected);

        Assert.False(result.IsMatch);
        Assert.Contains("apple", result.Mismatches);
    }

    [Fact]
    public void OneScannedItemMissing_IsMatchFalse_OnlyMissingItemListed()
    {
        var scanned  = new List<ScannedItem> { new("b1", "apple", 1), new("b2", "banana", 1) };
        var detected = new List<DetectedItem> { new() { Label = "apple", Confidence = 0.9f } };

        var result = ShoppingVerifier.BuildVerificationResult(scanned, detected);

        Assert.False(result.IsMatch);
        Assert.Contains("banana", result.Mismatches);
        Assert.DoesNotContain("apple", result.Mismatches);
    }

    [Fact]
    public void DetectedIgnoredLabel_IsMatchTrue_NoMismatches()
    {
        var scanned  = new List<ScannedItem>();
        var detected = new List<DetectedItem> { new() { Label = "person", Confidence = 0.9f } };

        var result = ShoppingVerifier.BuildVerificationResult(scanned, detected);

        Assert.True(result.IsMatch);
        Assert.Empty(result.Mismatches);
    }

    [Fact]
    public void ScannedIgnoredLabel_IsMatchTrue_NoMismatches()
    {
        var scanned  = new List<ScannedItem> { new("b1", "person", 1) };
        var detected = new List<DetectedItem>();

        var result = ShoppingVerifier.BuildVerificationResult(scanned, detected);

        Assert.True(result.IsMatch);
        Assert.Empty(result.Mismatches);
    }
}
