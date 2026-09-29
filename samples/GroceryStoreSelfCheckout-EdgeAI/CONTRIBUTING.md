# Contributing

Thank you for your interest in contributing to Edge AI Kiosk.

## Prerequisites

- Windows 11 recommended
- .NET 8 SDK for the app target framework
- A .NET SDK or Visual Studio version that supports `.slnx` files, or use the project-file build fallback below
- Visual Studio 2022 with WinUI and .NET desktop workloads
- Windows App SDK tooling
- Qualcomm Snapdragon X Windows device for the intended `win-arm64` QNN path
- A compatible YOLO26 ONNX model copied to `EdgeAIKiosk\Models\`

## Build

From the repository root:

```powershell
dotnet restore EdgeAIKiosk\EdgeAIKiosk.csproj
dotnet build EdgeAIKiosk.slnx -r win-arm64
```

If your SDK does not support `.slnx`, build the project file directly:

```powershell
dotnet build EdgeAIKiosk\EdgeAIKiosk.csproj -r win-arm64
```

## Test

From the sample directory:

```powershell
dotnet test EdgeAIKiosk.Tests\EdgeAIKiosk.Tests.csproj -r win-x64 -p:Platform=x64
```

For ARM64, use `-r win-arm64 -p:Platform=ARM64` on a Windows ARM64 device.

Memory stress tests are skipped by default. Set `RUN_MEMORY_STABILITY_TESTS=1` in the test process environment to enable them. See [Opt-in memory stress tests](README.md#opt-in-memory-stress-tests) for Visual Studio/command-line instructions and model/runtime requirements.

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
