namespace EdgeAIKiosk1.Models;

public class DetectedItem
{
    public string Label { get; set; } = string.Empty;
    public float Confidence { get; set; }
    public BoundingBox Box { get; set; } = new(0, 0, 0, 0);
    public int ObservationCount { get; set; }
}
