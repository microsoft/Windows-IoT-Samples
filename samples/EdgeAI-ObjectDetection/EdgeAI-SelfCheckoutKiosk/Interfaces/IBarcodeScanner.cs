using System;
using System.Threading.Tasks;

namespace EdgeAI.SelfCheckoutKiosk.Interfaces;

public interface IBarcodeScanner : IDisposable
{
    event Action<string> BarcodeScanned;

    Task StartAsync();
}
