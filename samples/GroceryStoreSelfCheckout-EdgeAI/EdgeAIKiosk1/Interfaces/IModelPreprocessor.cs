using EdgeAIKiosk1.Models;
using Windows.Graphics.Imaging;

namespace EdgeAIKiosk1.Interfaces;

public interface IModelPreprocessor
{
    ModelInput Preprocess(SoftwareBitmap frame);
}
