using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EdgeAIKiosk.Interfaces;
using EdgeAIKiosk.Models;
using EdgeAIKiosk.Pipeline;

namespace EdgeAIKiosk.Services;

public sealed class ShoppingVerifier(
    ImageCapture imageCapture,
    IModelPreprocessor preprocessor,
    IModelLoader modelLoader,
    IObjectTrackingStrategy tracker) : System.IDisposable
{
    // Keeping capture, preprocessing, inference, and tracking separate lets the sample show
    // each edge-AI stage without hiding hardware/model assumptions in UI code.
    private readonly ImageCapture _imageCapture = imageCapture;
    private readonly IModelPreprocessor _preprocessor = preprocessor;
    private readonly IModelLoader _modelLoader = modelLoader;
    private readonly IObjectTrackingStrategy _tracker = tracker;

    // A short burst balances checkout latency with enough observations to smooth camera noise.
    private const int FrameCount = 5;

    #region Main entrypoint

    /// <summary>Captures several frames, compares detected items against scanned items, and returns checkout status.</summary>
    /// <param name="scannedItems">The cart items collected from barcode scans before checkout.</param>
    public async Task<VerificationResult> Verify(List<ScannedItem> scannedItems)
    {
        this.ResetTracker();
        await this.RunInferenceFrames();
        return BuildVerificationResult(scannedItems, this._tracker.Track([]));
    }

    #endregion

    #region Steps

    private void ResetTracker() =>
        this._tracker.Track([], reset: true);

    /// <summary>Samples camera frames and feeds each successful model output into the tracker.</summary>
    private async Task RunInferenceFrames()
    {
        // Sample multiple frames so the tracker can smooth detections.
        for (int i = 0; i < FrameCount; i++)
        {
            var frame = await this._imageCapture.CaptureFrame();
            if (frame is null) continue;

            // Preprocess, infer, then accumulate detections.
            var output = await this._modelLoader.RunInference(this._preprocessor.Preprocess(frame));
            this._tracker.Track(output.Detections);
        }
    }

    /// <summary>Builds the final mismatch result from scanned cart items and tracked camera detections.</summary>
    /// <param name="scanned">The barcode cart items that the customer claims are present.</param>
    /// <param name="detected">The camera detections that passed the tracker threshold.</param>
    internal static VerificationResult BuildVerificationResult(
        List<ScannedItem> scanned,
        IReadOnlyList<DetectedItem> detected)
    {
        // The demo only compares labels that the selected model/classes can actually detect;
        // other scanned products need barcode/POS data, not camera verification.
        var scannedLabels = new HashSet<string>(scanned.Select(item => item.Name).Where(VerificationLabels.LabelsToVerify.Contains));
        var detectedLabels = new HashSet<string>(detected.Select(item => item.Label).Where(VerificationLabels.LabelsToVerify.Contains));

        // A mismatch is anything present in only one side.
        var scannedOnly = scannedLabels.Except(detectedLabels);
        var detectedOnly = detectedLabels.Except(scannedLabels);
        var mismatches = scannedOnly.Concat(detectedOnly).Distinct().ToList();
        return new VerificationResult(mismatches.Count == 0, scanned, detected, mismatches);
    }

    #endregion

    public void Dispose() => this._modelLoader.Dispose();
}
