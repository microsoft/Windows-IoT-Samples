using EdgeAIKiosk1.Models;
using System.Collections.Generic;

namespace EdgeAIKiosk1.Interfaces;

public interface IObjectTrackingStrategy
{
    IReadOnlyList<DetectedItem> Track(IEnumerable<DetectedItem> detections, bool reset = false);
}
