using EdgeAIKiosk1.Models;
using System.Threading.Tasks;

namespace EdgeAIKiosk1.Interfaces;

public interface IModelLoader : System.IDisposable
{
    Task<ModelOutput> RunInference(ModelInput input);
}
