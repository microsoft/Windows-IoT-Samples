using System.Collections.Generic;

namespace EdgeAIKiosk1.Models;

public record ModelOutput(IReadOnlyList<DetectedItem> Detections, double InferenceTimeMs);
