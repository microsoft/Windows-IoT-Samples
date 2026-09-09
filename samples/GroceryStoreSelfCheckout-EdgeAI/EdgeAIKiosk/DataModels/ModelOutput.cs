using System.Collections.Generic;

namespace EdgeAIKiosk.Models;

public record ModelOutput(IReadOnlyList<DetectedItem> Detections, double InferenceTimeMs);
