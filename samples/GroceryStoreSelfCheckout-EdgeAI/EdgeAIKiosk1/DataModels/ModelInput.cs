namespace EdgeAIKiosk1.Models;

public record ModelInput(float[] Tensor, int Width, int Height, int Channels)
{
    public float PadLeft { get; init; }
    public float PadTop { get; init; }
    public float Scale { get; init; }
}
