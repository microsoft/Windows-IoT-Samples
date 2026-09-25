# Edge AI Kiosk - Hardware-Accelerated Self-Checkout Verification with YOLO

## Overview

Edge AI Kiosk is a WinUI 3 proof-of-concept for self-checkout basket verification. The app lets a cashier or customer scan items, captures camera frames at checkout, runs a YOLO ONNX model locally with Windows ML, and compares detected objects against the scanned cart before payment.

The goal is to show why edge AI is useful in a kiosk: low-latency verification, no cloud round trip for camera frames, and local inference that can use an NPU, GPU, or CPU.

<img width="1561" height="681" alt="SwimlaneDiagramJPG" src="https://github.com/user-attachments/assets/5bd480c8-b9e4-4845-a925-d8d0d43aeb21" />
<img width="1588" height="819" alt="EdgeAIXMLUMLFinal" src="https://github.com/user-attachments/assets/cae2a209-12de-4621-9f1d-82d293db944b" />



## Demo

The current demo flow is:

1. Start the app on the home screen.
2. Choose an ONNX model, inference hardware, and camera in settings. Hardware defaults to **Auto**, and the picker lists only the CPU, GPU, and NPU types detected on the device.
3. Optionally choose which COCO labels the kiosk should verify.
4. Scan items on the shopping screen. For this POC, scanner input is treated as both the barcode and item name, so use labels such as `apple`, `banana`, `orange`, `bottle`, or `cup`.
5. Select **Pay Now**.
6. The app captures camera frames, runs object detection, and opens the alert screen with either a successful checkout or a mismatch list.
<img width="761" height="486" alt="alertwindow" src="https://github.com/user-attachments/assets/ef9e39b7-8b9a-4cfb-ba4c-d7dbb8ee1f19" />



## Prerequisites

| Area | Requirement |
| --- | --- |
| Hardware | Windows device with an NPU, DirectX 12 GPU, or CPU. Windows ML prefers NPU, then GPU, then CPU. |
| OS | Windows 11 24H2 or newer recommended for dynamic execution-provider installation. The project targets `net8.0-windows10.0.19041.0` and package min version `10.0.18362.0`. |
| SDK | .NET 8 SDK. |
| IDE | Visual Studio 2022 17.8 or newer with .NET desktop development, Windows App SDK, and WinUI tooling. |
| Camera | Built-in camera or USB UVC-compatible webcam. |
| Scanner | Barcode scanner that behaves like a keyboard, or a keyboard for manual demo input. |
| Model | A compatible YOLO26 ONNX model copied into `EdgeAIKiosk\Models\`. |

## Quick Start

```powershell
git clone https://github.com/t-shrpathak_microsoft/EdgeAIKioskProject.git
cd EdgeAIKioskProject

New-Item -ItemType Directory -Force EdgeAIKiosk\Models

# Use an ONNX model from Microsoft Foundry, Hugging Face, your own training,
# or another model zoo, converted to the contract in Model Setup.
Copy-Item <path-to-model>\yolo26x.onnx EdgeAIKiosk\Models\yolo26x.onnx

dotnet restore EdgeAIKiosk\EdgeAIKiosk.csproj
dotnet build EdgeAIKiosk\EdgeAIKiosk.csproj -c Debug -r win-arm64
dotnet run --project EdgeAIKiosk\EdgeAIKiosk.csproj -c Debug -r win-arm64
```

Visual Studio is the recommended launch path for day-to-day WinUI debugging:

1. Open `EdgeAIKiosk.slnx`.
2. Select the `ARM64` platform for Snapdragon X hardware.
3. Confirm `EdgeAIKiosk\Models\yolo26x.onnx` exists before launching.
4. Press F5.

## Project Structure

```text
EdgeAIKiosk\
  App.xaml                         App startup and shared resources
  Configuration\KioskSettings.cs  Runtime model, hardware, camera, and label selections
  Styles\KioskStyles.xaml          Shared brushes, spacing, typography, and control styles
  Views\HomeWindow.xaml            Model, hardware, camera, and label selection
  Views\ShoppingView.xaml          Barcode input, cart UI, camera preview, checkout
  Views\AlertWindow.xaml           Verification result UI
  Pipeline\ImageCapture.cs         WinRT camera preview and frame capture
  Pipeline\Yolo26Preprocessor.cs   SoftwareBitmap to 640x640 tensor conversion
  Pipeline\Yolo26SnapdragonXLoader.cs
                                   Windows ML session selection and YOLO output parsing
  Pipeline\MajorityFrames.cs       Multi-frame confidence/count gate
  Services\ShoppingVerifier.cs     Compares scanned cart labels with detected labels
  Services\VerifierFactory.cs      Wires capture, preprocessing, model, and tracking
  DataModels\                      Cart, detection, model input/output, and result types
  Models\                          Local ONNX model files; not committed to Git

EdgeAIKiosk.Tests\
  xUnit tests for bounding boxes, tracking, scanned items, letterbox math, hardware options, and verification
```

## Model Setup

ONNX model files are intentionally ignored by Git because model artifacts can be large or restricted. The app expects model files under:

```text
EdgeAIKiosk\Models\
```

ONNX is an open machine-learning model standard. That is why this sample uses it: Windows ML and ONNX Runtime can run models from many training ecosystems once they are exported or converted to ONNX.

Acquisition path:

1. Find a compatible object-detection model from Microsoft Foundry, Hugging Face, a custom training run, or another model zoo.
2. If the model is not already ONNX, convert it with the model's recommended open-source exporter or the Windows ML CLI.
3. Export or rename the final file to `yolo26x.onnx`.
4. Copy it to `EdgeAIKiosk\Models\yolo26x.onnx`.
5. Rebuild the app so MSBuild copies the model into the output `Models` folder.

This repository does not include an ONNX model because model licenses and sizes vary. The fastest path is to use a model that is already available as ONNX; conversion is for cases where the source model is in another format.

The default model path is configured in `KioskSettings.cs`:

```csharp
public static string ModelFileName { get; set; } = "Models\\yolo26x.onnx";
```

At build time, any `EdgeAIKiosk\Models\*.onnx` file is copied to the output directory. At runtime, the home screen lists those copied `.onnx` files in the model picker.

The current loader expects:

- Input name: `images`
- Input layout: `NCHW`
- Input shape: `1 x 3 x 640 x 640`
- Opset: use the opset required by your export tool and supported by the installed ONNX Runtime QNN package
- Pixel format: RGB values normalized to `0.0` through `1.0`
- Output shape: `1 x 300 x 6`, containing `x1`, `y1`, `x2`, `y2`, confidence, and COCO class id per detection row
- Label mapping: class ids must match `CocoLabels.cs`

If your model uses a different input name, shape, output layout, or label set, update `Yolo26SnapdragonXLoader.cs`, `Yolo26Preprocessor.cs`, and `CocoLabels.cs` before running checkout verification.

## Configuration

Configuration is currently in code and through the home-screen settings UI:

| Setting | Location | Notes |
| --- | --- | --- |
| Model file | `KioskSettings.ModelFileName` and the model picker | Defaults to `Models\yolo26x.onnx`. |
| Inference hardware | `KioskSettings.PreferredHardware` and the hardware picker | Defaults to Auto, which tries NPU, GPU, then CPU. The picker lists only detected types; an explicit choice uses only that hardware type. |
| Camera | `KioskSettings.CameraDeviceId` and the camera picker | Defaults to the first available video device. |
| Labels to verify | `KioskSettings.AcceptedLabels` and the label dialog | Defaults to `apple`, `banana`, `orange`, `bottle`, and `cup`. |

## Architecture

Checkout verification is intentionally split into small pipeline stages:

1. `ShoppingView` collects scanned items and owns the live camera preview.
2. `ImageCapture` starts `MediaCapture`, reads color frames, and returns `SoftwareBitmap` frames.
3. `Yolo26Preprocessor` converts frames to ImageSharp RGB images, letterboxes them to 640x640, writes CHW tensor data, and stores scale/padding metadata.
4. `Yolo26SnapdragonXLoader` runs the ONNX model on the selected Windows ML hardware. Auto tries NPU, GPU, then CPU; an explicit choice uses only that hardware type.
5. `MajorityFrames` filters detections by confidence and observation count.
6. `ShoppingVerifier` compares verified detected labels with the scanned cart labels.
7. `AlertWindow` shows the pass/fail result and mismatch details.

### Design Notes

- Camera capture uses WinRT `MediaCapture` and `MediaFrameReader` because OpenCvSharp does not provide a reliable `win-arm64` native path for this target.
- Preprocessing uses ImageSharp after frames are converted from `SoftwareBitmap`, then preserves letterbox padding and scale so detections can be mapped back to camera-frame coordinates.
- The app uses async camera initialization because WinRT camera APIs are async-first.
- WinUI 3 does not support WPF-style `DataTrigger`, so the alert UI uses explicit visibility changes between success and failure panels.
- Windows ML downloads and registers certified providers when available. The Home picker lists detected hardware; Auto falls back through NPU, GPU, and CPU, while an explicit choice does not fall back.

## Deployment

For a loose-file deployment to a kiosk device:

```powershell
dotnet publish EdgeAIKiosk\EdgeAIKiosk.csproj -c Release -r win-arm64 --self-contained true -o .\publish\win-arm64
Copy-Item EdgeAIKiosk\Models\yolo26x.onnx .\publish\win-arm64\Models\yolo26x.onnx
```

Copy the published folder to the target Snapdragon X device and run `EdgeAIKiosk.exe`.

For MSIX packaging, use Visual Studio **Package and Publish** on the `EdgeAIKiosk` project. Sign the package with a trusted certificate before installing on a kiosk device.

## Testing

Run the xUnit test project from the repository root:

```powershell
dotnet test EdgeAIKiosk.Tests\EdgeAIKiosk.Tests.csproj
```

The tests cover core verification logic, bounding box math, majority-frame tracking, scanned item behavior, letterbox coordinate conversion, and hardware-picker filtering and ordering.

## Known Limitations and Non-Goals

- This is a proof-of-concept, not a complete production point-of-sale system.
- The current barcode flow treats scanner input as the item label. A production integration should map real barcodes to product records.
- Dynamic NPU provider installation requires Windows 11 24H2 or newer and may require network access on first run.
- The model artifact is not included in the repository.
- This sample verifies configured COCO object labels. It does not solve product lookalikes, occlusion, weighing, payment processing, fraud policy, or inventory synchronization.

## Troubleshooting

| Symptom | Fix |
| --- | --- |
| Model picker is empty or startup cannot find `Models` | Create `EdgeAIKiosk\Models\` and copy a compatible `.onnx` model into it before building or running. |
| NPU or GPU does not appear in the hardware picker | The picker only shows hardware types reported by Windows ML. Update Windows and hardware drivers, and allow network access for initial certified-provider registration. |
| An explicit hardware choice fails during startup | Select Auto to allow fallback, or select another detected type. Explicit CPU, GPU, or NPU choices intentionally do not fall back. |
| Camera list is empty | Connect a UVC-compatible webcam or enable the built-in camera in Windows Settings. |
| Camera access is denied | Enable camera permissions for desktop apps in Windows Settings > Privacy & security > Camera. |
| `APPX1101` or architecture errors when building | Build with an explicit runtime, for example `dotnet build EdgeAIKiosk\EdgeAIKiosk.csproj -r win-arm64`. WinUI packaged apps should not rely on Any CPU for this target. |
| `byte[].AsBuffer()` is not found while editing preprocessing code | Ensure `System.Runtime.InteropServices.WindowsRuntime` is referenced where WinRT buffer conversion is used. |
| XAML compiler exits with a generic error | Check that each `Window` has a single root child element. Multiple direct root grids can cause markup compilation failures. |
| `onnxruntime.dll` is missing at runtime | Restore and rebuild for an explicit runtime such as `win-arm64` or `win-x64`; Windows ML supplies the matching native runtime. |
| Verification always mismatches | Make sure scanned values match the configured labels and the model class ids align with `CocoLabels.cs`. |

## Contributing and License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
