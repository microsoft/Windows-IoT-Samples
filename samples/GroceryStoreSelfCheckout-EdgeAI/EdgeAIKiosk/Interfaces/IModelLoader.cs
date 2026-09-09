using EdgeAIKiosk.Models;
using System.Threading.Tasks;

namespace EdgeAIKiosk.Interfaces;

public interface IModelLoader : System.IDisposable
{
    Task<ModelOutput> RunInference(ModelInput input);
}
