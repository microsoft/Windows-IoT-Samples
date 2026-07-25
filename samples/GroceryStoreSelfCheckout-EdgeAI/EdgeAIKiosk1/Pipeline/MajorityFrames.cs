using System.Collections.Generic;
using System.Linq;
using EdgeAIKiosk1.Interfaces;
using EdgeAIKiosk1.Models;

namespace EdgeAIKiosk1.Pipeline;

public sealed class MajorityFrames : IObjectTrackingStrategy
{
    // Several quick frames are more reliable than one snapshot in a kiosk: hands,
    // glare, and motion blur can hide an item for a single camera read.
    private readonly Dictionary<string, int> _observationCounts = new();
    private readonly Dictionary<string, DetectedItem> _latestDetections = new();

    // The tracker keeps model-specific confidence noise out of checkout decisions.
    public int RequiredCount { get; set; } = 1;
    public float MinConfidence { get; set; } = 0.5f;
    public int TotalObservations { get; private set; }

    #region Main entrypoint

    /// <summary>Accumulates detections across frames and returns labels that passed the confidence/count gate.</summary>
    /// <param name="detections">The model detections from the current frame or an empty list when only reading current results.</param>
    /// <param name="reset">Clears prior frame history before processing the supplied detections.</param>
    public IReadOnlyList<DetectedItem> Track(IEnumerable<DetectedItem> detections, bool reset = false)
    {
        this.ResetIfRequested(reset);
        this.AccumulateCounts(detections);
        return this.BuildValidatedItems().ToList();
    }

    #endregion

    #region Steps

    private void ResetIfRequested(bool reset)
    {
        if (!reset) return;
        this._observationCounts.Clear();
        this._latestDetections.Clear();
        this.TotalObservations = 0;
    }

    /// <summary>Adds confident detections into the running label counts.</summary>
    /// <param name="detections">The current frame's detected objects, including labels, confidence scores, and boxes.</param>
    private void AccumulateCounts(IEnumerable<DetectedItem> detections)
    {
        // Count only detections that are confident enough to participate.
        foreach (var item in detections.Where(d => d.Confidence >= MinConfidence))
        {
            this._observationCounts.TryGetValue(item.Label, out int count);
            this._observationCounts[item.Label] = count + 1;
            this._latestDetections[item.Label] = item;
            this.TotalObservations++;
        }
    }

    private IEnumerable<DetectedItem> BuildValidatedItems()
    {
        // Only labels seen enough times pass the majority gate.
        foreach (var countByLabel in this._observationCounts.Where(item => item.Value >= this.RequiredCount))
        {
            yield return this.BuildValidatedItem(countByLabel.Key, countByLabel.Value);
        }
    }

    #endregion

    #region Helpers

    /// <summary>Copies the latest detection for a label and attaches its accumulated observation count.</summary>
    private DetectedItem BuildValidatedItem(string label, int count)
    {
        var latestDetection = this._latestDetections[label];
        var validatedItem = new DetectedItem();
        validatedItem.Label = latestDetection.Label;
        validatedItem.Confidence = latestDetection.Confidence;
        validatedItem.Box = latestDetection.Box;
        validatedItem.ObservationCount = count;
        return validatedItem;
    }

    #endregion
}
