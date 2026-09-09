using EdgeAIKiosk.Models;
using System.Collections.Generic;

namespace EdgeAIKiosk.Interfaces;

public interface IObjectTrackingStrategy
{
    IReadOnlyList<DetectedItem> Track(IEnumerable<DetectedItem> detections, bool reset = false);
}
