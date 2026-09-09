namespace EdgeAIKiosk.Models;

public record BoundingBox(float X, float Y, float Width, float Height)
{
    public float Area => Width * Height;
}
