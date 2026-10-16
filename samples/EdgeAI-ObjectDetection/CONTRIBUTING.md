# Contributing

Thank you for your interest in contributing to the Edge AI object detection samples.

## Prerequisites

- Windows 11 recommended
- .NET 8 SDK for the app target framework
- A .NET SDK or Visual Studio version that supports `.slnx` files, or use the project-file build fallback below
- Visual Studio 2022 with WinUI and .NET desktop workloads
- Windows App SDK tooling
- Qualcomm Snapdragon X Windows device for the intended `win-arm64` QNN path
- A compatible YOLO26 ONNX model copied to `EdgeAI-SelfCheckoutKiosk\Models\`

## Build

From `samples\EdgeAI-ObjectDetection` in a Visual Studio Developer PowerShell:

```powershell
msbuild EdgeAI-ObjectDetection.slnx /restore /p:Configuration=Debug /p:Platform=ARM64
```

If your SDK does not support `.slnx`, build the project file directly:

```powershell
msbuild EdgeAI-SelfCheckoutKiosk\EdgeAI-SelfCheckoutKiosk.csproj /restore /p:Configuration=Debug /p:Platform=ARM64
```

## Test

After building, run both test assemblies from the sample directory:

```powershell
dotnet vstest EdgeAI-SelfCheckoutKiosk.Tests\bin\ARM64\Debug\net8.0-windows10.0.19041.0\EdgeAI-SelfCheckoutKiosk.Tests.dll EdgeAI.ObjectDetection.Yolo.Tests\bin\ARM64\Debug\net8.0-windows10.0.19041.0\EdgeAI.ObjectDetection.Yolo.Tests.dll
```

On an x64 device, use `/p:Platform=x64` when building and replace `ARM64` with `x64` in the test paths.

Memory stress tests are skipped by default. Set `$env:RUN_MEMORY_STABILITY_TESTS = "1"` before running tests to enable them. The inference stress test requires a compatible model named `yolo26x.onnx` in `EdgeAI-SelfCheckoutKiosk\Models` and uses the CPU execution provider.

## Pull Requests

1. Open an issue or comment on an existing issue before large changes.
2. Keep PRs focused on one issue or behavior change.
3. Include screenshots or logs when changing UI, camera, model, or deployment behavior.
4. Do not commit ONNX model files, build output, local logs, or secrets.
5. Run the build and test commands above before requesting review.

## Code Style

- Follow the existing C# and XAML style in the file you are editing.
- Prefer small, direct fixes over new abstractions.
- Keep error handling explicit for camera, model, hardware, and checkout paths.
- Update README or troubleshooting notes when setup, model, or deployment behavior changes.
