using EdgeAIKiosk.Models;
using Windows.Graphics.Imaging;

namespace EdgeAIKiosk.Interfaces;

public interface IModelPreprocessor
{
    ModelInput Preprocess(SoftwareBitmap frame);
}
