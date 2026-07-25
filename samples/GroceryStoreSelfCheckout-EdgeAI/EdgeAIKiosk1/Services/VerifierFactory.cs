using EdgeAIKiosk1.Interfaces;
using EdgeAIKiosk1.Pipeline;

namespace EdgeAIKiosk1.Services;

public static class VerifierFactory
{
    public static string ModelPath => System.IO.Path.Combine(System.AppContext.BaseDirectory, KioskSettings.ModelFileName);

    public static ShoppingVerifier Create(
        ImageCapture imageCapture,
        IModelPreprocessor preprocessor,
        IModelLoader loader) =>
        new(imageCapture, preprocessor, loader, new MajorityFrames());
}
