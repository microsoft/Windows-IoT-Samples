using System;
using System.Threading.Tasks;

namespace EdgeAIKiosk.Interfaces;

public interface IBarcodeScanner : IDisposable
{
    event Action<string> BarcodeScanned;

    Task StartAsync();

    Task StopAsync();
}
