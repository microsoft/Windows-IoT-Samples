using EdgeAIKiosk.Interfaces;
using EdgeAIKiosk.Pipeline;

namespace EdgeAIKiosk.Services;

public static class VerifierFactory
{
    public static string ModelPath => System.IO.Path.Combine(System.AppContext.BaseDirectory, KioskSettings.ModelFileName);

    public static ShoppingVerifier Create(
        ImageCapture imageCapture,
        IModelPreprocessor preprocessor,
        IModelLoader loader) =>
        new(imageCapture, preprocessor, loader, new MajorityFrames());
}
