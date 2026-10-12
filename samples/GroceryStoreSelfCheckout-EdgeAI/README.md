# Edge AI Kiosk - Hardware-Accelerated Self-Checkout Verification with YOLO

## Overview

Edge AI Kiosk is a WinUI 3 proof-of-concept for self-checkout basket verification. The app lets a cashier or customer scan items, continuously analyzes camera frames using a YOLO ONNX model locally with Windows ML, and compares the latest completed detections against the scanned cart before payment.

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
6. The app copies the latest completed streaming detections and opens the alert screen with either a successful checkout or a mismatch list. A mismatch screen keeps the same live stream running without reloading the camera or model; the checkout decision remains fixed to the copied result.
<img width="761" height="486" alt="alertwindow" src="https://github.com/user-attachments/assets/ef9e39b7-8b9a-4cfb-ba4c-d7dbb8ee1f19" />



## Prerequisites

| Area | Requirement |
| --- | --- |
| Hardware | Windows device with an NPU, DirectX 12 GPU, or CPU. The kiosk's Auto mode tries NPU, then GPU, then CPU; ObjectDetection uses the explicitly selected execution provider. |
| OS | Windows 11 24H2 or newer recommended for dynamic execution-provider installation. Both applications target `net8.0-windows10.0.19041.0`. |
| SDK | .NET 8 SDK. |
| IDE | Visual Studio with `.slnx` support (Visual Studio 2022 17.14 or newer), .NET desktop development, Windows App SDK, and WinUI tooling. Use a Visual Studio Developer PowerShell for MSBuild commands. |
| Camera | Built-in camera or USB UVC-compatible webcam. |
| Scanner | Barcode scanner that behaves like a keyboard, or a keyboard for manual demo input. |
| Model | A compatible ONNX model with the input/output contract below. Copy it into `EdgeAIKiosk\Models\` for checkout, or select its folder in ObjectDetection. |

## Quick Start

```powershell
git clone https://github.com/microsoft/Windows-IoT-Samples.git
Set-Location Windows-IoT-Samples\samples\GroceryStoreSelfCheckout-EdgeAI

New-Item -ItemType Directory -Force EdgeAIKiosk\Models

# Use an ONNX model from Microsoft Foundry, Hugging Face, your own training,
# or another model zoo, converted to the contract in Model Setup.
Copy-Item <path-to-model>\model.onnx EdgeAIKiosk\Models\

msbuild EdgeAIKiosk\EdgeAIKiosk.csproj /restore /t:Build /p:Configuration=Debug /p:Platform=ARM64 /p:RuntimeIdentifier=win-arm64
```

Visual Studio is the recommended launch path for day-to-day WinUI debugging:

1. Open `EdgeAIKiosk.slnx`.
2. Set `EdgeAIKiosk` as the startup project and select the `ARM64` platform for Snapdragon X hardware, or `x64` for an x64 device.
3. Copy a compatible `.onnx` model into `EdgeAIKiosk\Models\` before launching.
4. Press F5.

To run the standalone detection demo instead, set `EdgeAI-ObjectDetection` as the startup project and press F5. Choose a folder containing compatible `.onnx` models, then select a model, execution provider, and camera. It does not require copying models into the kiosk's `Models` folder.

## Project Structure

```text
EdgeAIKiosk\
  App.xaml                         App startup and shared resources
  Configuration\KioskSettings.cs  App-owned model, hardware, camera, and label selections
  Styles\KioskStyles.xaml          Shared brushes, spacing, typography, and control styles
  Views\MainWindow.xaml            Persistent window and navigation frame
  Views\HomePage.xaml              Model, hardware, camera, and label selection
  Views\ShoppingPage.xaml          Barcode input, cart UI, camera preview, checkout
  Views\AlertPage.xaml             Verification result UI
  YoloInferenceView\               Copy of the reusable control, including Pipeline and Types
  Services\KioskInference.cs       Starts the control with the selected camera, model, and classes
  Services\ExecutionProviderPolicy.cs
                                   Automatic provider fallback or explicit hardware selection
  Services\ShoppingVerifier.cs     Compares scanned cart labels with detected labels
  DataModels\                      Cart and verification result types
  Models\                          Local ONNX model files; not committed to Git

EdgeAIKiosk.Tests\
  xUnit tests for provider fallback, hardware options, startup handling, and single-result verification

EdgeAI-ObjectDetection\
  MainWindow.xaml                 Model-folder, provider, camera, and inference-mode selection
  YoloInferenceView\              Reusable control source, including Pipeline and Types

EdgeAI-ObjectDetection.Tests\
  xUnit tests for preview geometry, model contracts, detection decoding, label filtering, and snapshots
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
3. Export the final file with an `.onnx` extension; no particular filename is required.
4. Copy it to `EdgeAIKiosk\Models\`.
5. Rebuild the app so MSBuild copies the model into the output `Models` folder.

This repository does not include an ONNX model because model licenses and sizes vary. The fastest path is to use a model that is already available as ONNX; conversion is for cases where the source model is in another format.

`KioskSettings.cs` has no hardcoded model preference:

```csharp
public string? ModelFileName { get; set; }
```

At build time, any `EdgeAIKiosk\Models\*.onnx` file is copied to the output directory. At runtime, the home screen lists those copied `.onnx` files in the model picker. It selects the first available model on a fresh launch and restores the chosen model when returning home within the same session.

Both applications use the same control and model contract:

- Input name: read from model metadata (one input)
- Input layout: `NCHW`
- Input shape: `1 x 3 x 640 x 640`
- Opset: use the opset required by your export tool and supported by the installed Windows ML runtime and selected execution provider
- Pixel format: RGB values normalized to `0.0` through `1.0`
- Output shape: `1 x 300 x 6`, containing `x1`, `y1`, `x2`, `y2`, confidence, and COCO class id per detection row
- Class mapping: class ids must match the standard COCO ordering returned by `YoloInferenceView.GetSupportedClasses()`

Other shapes, output layouts, and label sets require changes to the shared `YoloInferenceView` pipeline. See [Supported model contract](#supported-model-contract).

## Configuration

Configuration is currently in code and through the home-screen settings UI:

| Setting | Location | Notes |
| --- | --- | --- |
| Model file | `KioskSettings.ModelFileName` and the model picker | Selects the first available model; preserves the selection when returning home. |
| Inference hardware | `KioskSettings.PreferredHardware` and the hardware picker | Defaults to Auto, which tries NPU, GPU, then CPU. The picker lists only detected types; an explicit choice uses only that hardware type. |
| Camera | `KioskSettings.CameraGroupId` and the camera picker | Defaults to the first available group with a color preview or recording stream. |
| Labels to verify | `KioskSettings.AcceptedLabels` and the label dialog | Defaults to all supported COCO labels; use the dialog to restrict the selection. |

## Architecture

Each app compiles its own identical copy of `YoloInferenceView`; the kiosk no longer has a separate app-specific camera pipeline or bounding-box renderer. Update the reusable component in `EdgeAI-ObjectDetection` first, then copy the entire `YoloInferenceView` folder unchanged into `EdgeAIKiosk`. Keep kiosk integration outside that folder. Kiosk settings are passed between pages as an app-owned instance, not read globally by the inference control.

1. `HomePage` selects settings and passes a camera group and inference options to `ShoppingPage` through the navigation frame.
2. `ShoppingPage` starts `YoloInferenceView` through `KioskInference`. Auto tries NPU, GPU, then CPU; an explicit choice uses only that hardware type.
3. The control captures frames, letterboxes the visible crop, runs the model, and publishes class-filtered detections with preview-local boxes.
4. Pay Now is enabled after the first completed result, including a valid empty result. It copies the latest streaming detections; it does not capture a new frame or perform multi-frame voting.
5. `ShoppingVerifier` compares selected label sets, not quantities. Later frames do not change the checkout decision.
6. `ShoppingPage` transfers the same control to `AlertPage`. A mismatch keeps streaming without a restart; a successful checkout disposes the control. Returning home awaits disposal and preserves the selected settings.

### Design Notes

- One `MainWindow` hosts a `Frame` for the home, shopping, and result pages. Navigation replaces page content without closing/recreating the native window or animating through a blank background. Navigation history is disabled so completed checkout sessions cannot be revisited.
- Pages initialize screen/session state in `OnNavigatedTo`. Reattaching a page's visual tree does not recreate its scanner or restart model loading. Navigation does not await async initialization, so checkout remains disabled until startup finishes and the first inference result arrives. Keyboard input is focused after startup enables it.
- The label dialog uses WinUI `ItemsView` with multiple selection and a uniform grid. Save reads selected data items; Cancel discards edits when the dialog is reopened. XAML child order defines overlay stacking without explicit Z-index values.
- Shopping transfers the running control to the result page without pausing inference. Success and returning home await inference cleanup; application exit is not deferred for cleanup.
- Camera capture uses WinRT `MediaCapture` and `MediaFrameReader` because OpenCvSharp does not provide a reliable `win-arm64` native path for this target.
- Both apps use the shared managed preprocessor, preserving crop/letterbox geometry through decoding and presentation. Neither app depends on ImageSharp.
- Label selection applies consistently to overlays, detection events, and snapshots. The kiosk always supplies its selection; selecting no labels ignores all detections and cart labels. ObjectDetection defaults to all labels.
- Boxes no longer distinguish scanned from unscanned items by color. The kiosk's item-count text consumes the control's filtered detection events; verification remains app logic, separate from the control.
- The app uses async camera initialization because WinRT camera APIs are async-first.
- WinUI 3 does not support WPF-style `DataTrigger`, so the alert UI uses explicit visibility changes between success and failure panels.
- Windows ML downloads and registers certified providers when available. The Home picker lists detected hardware; Auto falls back through NPU, GPU, and CPU, while an explicit choice does not fall back.

## Deployment

For a loose-file deployment to a kiosk device:

```powershell
dotnet publish EdgeAIKiosk\EdgeAIKiosk.csproj -c Release -r win-arm64 --self-contained true -o .\publish\win-arm64
Copy-Item EdgeAIKiosk\Models\*.onnx .\publish\win-arm64\Models\
```

Copy the published folder to the target Snapdragon X device and run `EdgeAIKiosk.exe`.

For MSIX packaging, use Visual Studio **Package and Publish** on the `EdgeAIKiosk` project. Sign the package with a trusted certificate before installing on a kiosk device.

### Publish profiles and package signing

Both applications reference `Properties\PublishProfiles\win-$(Platform).pubxml`. The shared platform profiles are included in source control and contain only publishing settings, not credentials. Keep personal profiles, `*.pubxml.user` files, and `*.pfx` signing certificates out of Git.

`EdgeAI-ObjectDetection` defaults to unsigned packaging and does not depend on a temporary certificate. Unsigned MSIX packages are not ready for normal installation. To create an installable package, configure signing in Visual Studio **Package and Publish**, or supply these MSBuild properties through local or CI configuration:

```powershell
# Run from a Visual Studio Developer PowerShell in the sample directory.
# Keep the certificate outside the repository; its subject must match the manifest Publisher.
msbuild EdgeAI-ObjectDetection\EdgeAI-ObjectDetection.csproj /restore /t:Build /p:Configuration=Release /p:Platform=x64 /p:RuntimeIdentifier=win-x64 /p:GenerateAppxPackageOnBuild=true /p:AppxPackageSigningEnabled=true /p:PackageCertificateKeyFile="C:\Signing\EdgeAI.pfx"
```

Provide any certificate password through your local signing setup or CI secret handling, never through committed files. The target device must trust the signing certificate. If Visual Studio writes a private certificate path or thumbprint back into the project, keep that setting local rather than committing it.

## Reusable ObjectDetection view

`EdgeAI-ObjectDetection` hosts `YoloInferenceView\YoloInferenceView.xaml`. The component's implementation lives in the `YoloInferenceView` folder, with `Pipeline` and `Types` subfolders. The window only discovers models, execution providers, and camera groups; the control owns its camera reader, preview player, inference pipeline, and overlays. Changing any selection performs a coordinated stop/start. Select **One shot** and press **Run one inference** to display an analyzed still image with its detections, without a live preview; streaming is the default.

The view reuses clip geometries, a shared detection brush, and overlay rectangles/labels. Overlay elements are allocated as the detection count grows (up to the model's 300 detections), hidden when unused, retained across stops, and released on disposal. Preview layout properties are updated only when their calculated bounds change; camera resolution, stretch mode, and DPI remain part of the geometry calculation. The inference engine caches managed tensor/input wrappers around the reusable input buffer; inference outputs and published detection results are still separately owned per inference.

The control is currently source-level reusable, not a separate NuGet package. `EdgeAIKiosk\YoloInferenceView` is an exact copy of `EdgeAI-ObjectDetection\YoloInferenceView`, including XAML, code-behind, pipeline/engine, preprocessing, and types. Namespaces are preserved to keep the copies identical. EdgeAIKiosk compiles its local files normally; it has no source links or project reference to ObjectDetection. To use it in another WinUI 3 app, copy the entire `YoloInferenceView` folder and use the same Windows ML and Windows App SDK dependencies as ObjectDetection. The host must have camera access and the appropriate package capabilities (`webcam`, `runFullTrust`, and `systemAIModels` as in the sample manifest). The host discovers/registers execution providers before supplying a selected `OrtEpDevice`.

The shared, independently implemented managed preprocessor proportionally resizes the visible rectangular crop to fit 640x640, then centers it on RGB (114,114,114) padding. It uses separable Catmull-Rom bicubic filtering, widened support for downsampling, and normalized crop-edge weights. Resized dimensions are rounded to whole pixels (at least one pixel per axis); an odd padding remainder goes on the right or bottom. Preprocessing returns the tensor, actual letterbox geometry, and optional snapshot pixels together. That geometry travels with the result through decoding and display, rather than being reconstructed from the current viewport. It clamps and rounds channels to bytes before RGB tensor normalization; one-shot snapshots use those same bytes. A per-pipeline workspace caches horizontal and vertical weights and reuses source, scratch, and tensor arrays. OneShot lazily allocates its pixel buffer once and reuses it on subsequent captures. These internal buffers are borrowed until the next preprocessing call: inference consumes the tensor synchronously, and `InferenceSnapshot` copies the pixels into its own bitmap before another call can overwrite them. Earlier snapshots and the uploaded frozen image remain independent. BGRA8 frames with ignored alpha avoid format conversion.

```xml
<!-- Add xmlns:ai="using:EdgeAI_ObjectDetection.Controls" to the containing view. -->
<ai:YoloInferenceView x:Name="Yolo"
                      Width="800" Height="600"
                      PreviewStretch="UniformToFill"
                      ShowEndToEndInferenceTime="True"
                      ShowInferenceTime="True" />
```

```csharp
InferenceSettings settings = new()
{
    FrameSourceGroup = selectedCameraGroup,
    ModelPath = selectedModel.Path,
    ExecutionProvider = selectedDevice,
    MaxEndToEndFps = 30,
    InferenceType = InferenceType.Stream
};

Yolo.DetectionsUpdated += (_, args) =>
{
    // Empty means inference completed with no detections.
    foreach (PreviewDetection detection in args.Detections)
        Debug.WriteLine($"{detection.Label}: {detection.Confidence:P0}");
};
Yolo.Faulted += (_, args) => Debug.WriteLine(args.Exception);
await Yolo.StartAsync(settings);

// Before replacing a model/camera or navigating away from a reusable view:
await Yolo.StopAsync();
await Yolo.StartAsync(settings with { ModelPath = anotherModel.Path });

// When permanently retiring the instance:
await Yolo.DisposeAsync();
```

### Lifecycle and settings

Call lifecycle methods on the UI thread. `StartAsync` retains the supplied init-only settings and returns when the pipeline and preview are started, without waiting for a detection. Optional `ClassMask` accepts an `IReadOnlyCollection<string>` of included class names, not an array of Boolean flags or class IDs. Null includes all classes; an empty collection includes none. Names are case-insensitive, duplicates are accepted, and unsupported names are ignored. A mask containing only unsupported names includes no classes; it does not enable all classes. The engine resolves the names once into its own immutable class-ID filter and excludes masked classes before constructing detections, so overlays, events, and snapshots stay consistent. The supported-class list and class IDs are never renumbered. Do not modify the input collection while `StartAsync` is in progress; subsequent caller edits do not change the active filter. Apply a new mask by stopping and starting again.

`GetSupportedClasses()` always returns the view's complete supported class list, regardless of any mask or current detections. The caller can use this list to populate an editable class-selection UI, then pass the selected names as `ClassMask`. The kiosk's selection dialog follows this pattern.

```csharp
IReadOnlyList<string> classes = YoloInferenceView.GetSupportedClasses();
List<string> classMask = classes.Where(className => className is "apple" or "banana").ToList();
await Yolo.StartAsync(settings with { ClassMask = classMask });
```

The control must be stopped before starting; calling `StartAsync` while running throws `InvalidOperationException`, even with equivalent settings. Await `StopAsync` before starting again. Startup cancellation unwinds acquired resources. `StopAsync` cancels startup/inference, detaches preview consumers, waits for outstanding model execution, and releases the pipeline; it is safe to repeat. `DisposeAsync` also permanently retires the instance.

Pipeline cleanup attempts to release the reader, camera capture, and model even if an earlier cleanup step fails, then reports any failures together in an `AggregateException`.

For one-shot operation, start with `InferenceType.OneShot`, then call `await Yolo.InferOnceAsync(cancellationToken)`. The control displays the analyzed still image and its boxes together, and returns an `InferenceSnapshot` containing image-relative `Detections` and `Image` (a caller-owned `SoftwareBitmap`). Unlike the event's `PreviewDetection` list, snapshot detections use image pixels, independent of view layout. The image is the exact 640 x 640 letterboxed RGB image used to build the model tensor, including gray padding, stored as opaque BGRA8 with premultiplied alpha for XAML display. It contains the visible camera crop, not necessarily the full camera frame, and has no drawn overlays. Dispose the snapshot after copying, displaying, or saving its image; its detections remain usable after disposal. The control retains its own uploaded image, so the sample host can dispose the returned snapshot immediately without affecting the display.

```csharp
// Yolo has already been started in OneShot mode.
using InferenceSnapshot snapshot = await Yolo.InferOnceAsync(cancellationToken);
Microsoft.UI.Xaml.Media.Imaging.SoftwareBitmapSource imageSource = new();
await imageSource.SetBitmapAsync(snapshot.Image);
SnapshotImage.Source = imageSource; // A host-owned XAML Image.
IReadOnlyList<Detection> detections = snapshot.Detections;
```

Calls are serialized. A request waits for a new available frame and a usable viewport layout; use cancellation when the host no longer needs the result. One-shot mode keeps capture and the model ready but does not create or play a live preview. The viewport starts empty. Each successful inference replaces the still image and boxes together; they remain visible while waiting for another result, including when that request is canceled. Stop, disposal, or a pipeline fault clears the display. Returned snapshots remain valid across subsequent inferences or stopping/disposing the control until the caller disposes them. Streaming still delivers detection-only events and does not allocate snapshot image buffers. `InferOnceAsync` still requires one-shot mode; stop and restart with `InferenceType.OneShot` when switching from streaming. `MaxEndToEndFps` caps streaming cycles only (0.1-240 FPS); it does not throttle preview playback. Repeated frames are not reprocessed.

`State` and `StateChanged` expose `Stopped`, `Starting`, `Running`, `Stopping`, and `Disposed`. Startup errors propagate to the caller. Asynchronous capture, preview, and streaming failures stop the pipeline and raise `Faulted`; one-shot execution errors also propagate to its caller. Public events run on the UI thread; handlers should be quick and should not synchronously block on lifecycle tasks.

The host must await `StopAsync` or `DisposeAsync` when retiring or replacing an inference session. `Unloaded` does not automatically dispose the control. Both samples rely on process exit to reclaim inference resources when the app closes; neither defers window closure to await inference shutdown. The kiosk transfers the same running control from shopping to the mismatch screen by detaching it from its old host and attaching it to the new host, without stopping the session. It awaits disposal on successful checkout and before returning home. Hosts that continue running after retiring the control must explicitly await cleanup.

### Preview and detections

Normal XAML `Width`, `Height`, and layout constraints size the whole control, including its optional status lines. The remaining preview area can resize at runtime. `PreviewStretch` is fixed for each run: `None`, `Uniform`, and `UniformToFill` are supported; `Fill` is rejected. `None` displays one source pixel per physical screen pixel. Preview placement is calculated explicitly so display scaling and overlay mapping use the same geometry.

Inference consumes the entire visible rectangular camera region **as laid out by `PreviewStretch`**, rounded inward to source-pixel boundaries, excluding empty preview space and camera pixels cropped away by that layout. This rectangle is proportionally resized and letterboxed to 640 x 640 with gray padding. `Uniform` includes the full camera frame; `UniformToFill` and `None` can crop it to the viewport. Objects are no longer truncated at an artificial square boundary, but can still be truncated at the actual viewport edge. This input-crop policy is the same in streaming and one-shot modes, even though one-shot mode does not show the live camera. Resize/DPI changes update the next input crop without restarting the model or camera and discard in-flight results with obsolete capture geometry. Streaming clears old boxes and removes model padding when mapping new boxes onto the visible camera rectangle. One-shot mode instead retains the last result and uniformly fits its entire square model-input image (including padding) and matching boxes to the viewport, independent of `PreviewStretch`; resizing does not crop or invalidate the displayed still. A zero-sized viewport pauses inference, not capture. Hiding the camera image does not change the input crop.

`DetectionsUpdated` delivers a read-only list of immutable `PreviewDetection` records containing `ClassId`, `Label`, `Confidence`, and a `PreviewDetectionBox`. Its `X`, `Y`, `Width`, and `Height` are in XAML logical units (DIPs), relative to the preview area's top-left, excluding the status lines. Letterbox removal, preview scaling, and offsets are already applied. In one-shot mode the coordinates instead follow the fitted still image, including its padding. The event and built-in overlays share the same mapping; results are available even when built-in boxes and labels are hidden. A host can position rectangles directly on an overlay aligned with the preview area, without multiplying coordinates by its size. These are not native-window or screen coordinates. They describe the layout when the event fires; resizing redraws built-in overlays but does not emit another inference event. Custom overlays should clear stale results on resize and await the next result.

Snapshot `Detection` records use a distinct `DetectionBox` type: `X`, `Y`, `Width`, and `Height` in pixels relative to the top-left of `InferenceSnapshot.Image`, the 640 x 640 letterboxed model-input image. Values are floats, preserving fractional pixels; they are not normalized or rounded to integer pixel indices. For example, `(160, 200, 320, 240)` starts 160 pixels from the left and 200 pixels from the top, and is 320 pixels wide by 240 pixels high. Their coordinates remain stable after resizing the view and are suitable for drawing over a saved snapshot. Boxes are clipped to the camera-content rectangle within that image; padding-only detections are discarded. Callers migrating from normalized boxes must stop multiplying by the image dimensions. Drawing a snapshot at a different size still requires scaling to that display, or using the control's `PreviewDetection` event for its own preview. Hosts interested only in classes/counts can ignore either box type. `GetSupportedClasses()` returns the read-only COCO label list, indexed by class ID.

These dependency properties can change while running: `ShowCameraPreview`, `ShowBoundingBoxes`, `ShowLabels`, `ShowConfidence`, `ShowInferenceStatus`, `ShowInferenceTime`, and `ShowEndToEndInferenceTime`. They default to true. `ShowCameraPreview` controls live-image visibility in streaming and still-image visibility in one-shot mode; it never enables a live preview in one-shot mode. Both timing displays update for each accepted result in streaming and one-shot modes. E2E inference time runs from acquiring the available camera frame through preprocessing, inference, postprocessing, dispatch back to the UI, and bounding-box/label element updates, including the image upload in one-shot mode. It excludes waiting for a new frame, stream throttling, event-handler work, and deferred XAML layout/rendering or screen presentation. It measures the work of updating the presentation, not camera preview FPS or camera-to-display latency. The separate inference time covers the engine call (including its output decoding) without preprocessing or presentation updates.

### Supported model contract

Model selection is strict: the control loads exactly the requested file or fails. It accepts one float32 input `[1,3,640,640]` (RGB NCHW, values normalized to 0-1) and one float32 output `[1,300,6]`. Each output row is `x1, y1, x2, y2, confidence, classId`, with coordinates in model-input pixels and class IDs using the standard 80-class COCO ordering. Confidence filtering is currently fixed at 0.5.

Both YOLO26 NMS-free one-to-one exports and exports with embedded NMS can satisfy this contract. The control does not perform additional NMS. Raw candidate outputs, dynamic shapes, other batch sizes, segmentation/pose outputs, and custom class mappings are not supported. Tensor metadata is validated, but cannot prove label semantics or training preprocessing; supplying a model trained/exported for this contract remains the host's responsibility.

Camera capture uses the selected `MediaFrameSourceGroup` in `SharedReadOnly` mode, prefers a color preview stream and otherwise uses a color recording stream. It accepts the current camera format; it does not request 1080p or another resolution.

### ObjectDetection checks

The crop, model-shape, normalized-detection, and snapshot tests reference the application directly. They require Windows, the same WinUI build tools as the application, and the Windows App SDK runtime matching the app's SDK version and test-process architecture, but do not need a camera or model. The test project explicitly enables Windows App SDK bootstrapping, following Microsoft's [non-WinUI testing guidance](https://learn.microsoft.com/windows/apps/develop/testing/#testing-non-winui-functionality). The app disables Deployment Manager auto-initialization because it does not use Main/Singleton features; this also allows its assembly to load in an unpackaged test host. The project reference uses the normal app build without test-specific packaging overrides.

In Visual Studio, build the solution with the platform matching your machine (`x64` or `ARM64`), then run `EdgeAI-ObjectDetection.Tests` in Test Explorer. From a Visual Studio Developer PowerShell, build and run them:

```powershell
msbuild EdgeAI-ObjectDetection.Tests\EdgeAI-ObjectDetection.Tests.csproj /t:Restore /p:Configuration=Debug /p:Platform=x64 /p:RuntimeIdentifier=win-x64
msbuild EdgeAI-ObjectDetection.Tests\EdgeAI-ObjectDetection.Tests.csproj /t:Build /p:Configuration=Debug /p:Platform=x64 /p:RuntimeIdentifier=win-x64
dotnet test EdgeAI-ObjectDetection.Tests\EdgeAI-ObjectDetection.Tests.csproj --no-build --no-restore -c Debug -r win-x64 -p:Platform=x64
```

For ARM64, use `Platform=ARM64` and `RuntimeIdentifier=win-arm64` for both build commands, and `-r win-arm64 -p:Platform=ARM64` for the test command on a Windows ARM64 device.

On a device with a camera and compatible model, also exercise streaming/one-shot modes, model/camera/provider switching, a missing or incompatible model, and camera disconnection. Resize wide/tall/square previews with each supported stretch mode, including high-DPI displays; boxes must align across the entire visible camera rectangle and stay within its edges. In one-shot mode, confirm the viewport is initially empty, each request displays a matching letterboxed still image and boxes, padding has no boxes, and moving the camera afterward does not change that result. Resize after a shot, toggle image/box visibility, dispose the returned snapshot, and cancel a subsequent request: the retained result must remain aligned and usable. Stop must clear it, and switching back to streaming must restore the live preview. Check that stopping during startup or inference releases the camera and permits a subsequent start. These device checks require WinUI and hardware and are not covered by the hardware-independent suite.

## Testing

Run the xUnit test project from the `samples\GroceryStoreSelfCheckout-EdgeAI` directory:

```powershell
msbuild EdgeAIKiosk.Tests\EdgeAIKiosk.Tests.csproj /restore /t:Build /p:Configuration=Debug /p:Platform=x64 /p:RuntimeIdentifier=win-x64
dotnet test EdgeAIKiosk.Tests\EdgeAIKiosk.Tests.csproj --no-build --no-restore -c Debug -r win-x64 -p:Platform=x64
```

The kiosk tests cover single-result label-set verification, explicit selection semantics, Auto/provider fallback, hardware-picker filtering and ordering, and startup error reporting. Reusable geometry, model validation, decoder, preprocessing, snapshot, and label-filter coverage lives in `EdgeAI-ObjectDetection.Tests`. After updating the control in ObjectDetection, copy the entire component unchanged to EdgeAIKiosk and run both test projects.

For ARM64, use `-r win-arm64 -p:Platform=ARM64` on a Windows ARM64 device.

### Opt-in memory stress tests

The two memory stability tests report **Skipped** during ordinary runs, including Visual Studio Test Explorer, unless `RUN_MEMORY_STABILITY_TESTS=1` is present in the test process environment. They are labeled `Category=MemoryStability`.

To enable them in Visual Studio Test Explorer, set the environment variable before launching Visual Studio, then rediscover the tests. Run the memory tests individually. Alternatively, enable them for a terminal session:

```powershell
$env:RUN_MEMORY_STABILITY_TESTS = "1"
dotnet test EdgeAIKiosk.Tests\EdgeAIKiosk.Tests.csproj --no-build --no-restore -c Debug -r win-x64 -p:Platform=x64 --filter "FullyQualifiedName~PreprocessorMemoryStabilityTests"
dotnet test EdgeAIKiosk.Tests\EdgeAIKiosk.Tests.csproj --no-build --no-restore -c Debug -r win-x64 -p:Platform=x64 --filter "FullyQualifiedName~YoloInferenceMemoryStabilityTests"
Remove-Item Env:\RUN_MEMORY_STABILITY_TESTS
```

For ARM64, use `-r win-arm64 -p:Platform=ARM64` on a Windows ARM64 device. Requirements:

- The same .NET SDK/runtime and Windows App SDK/WinUI build tooling as the regular tests.
- For inference, a compatible model at `EdgeAIKiosk\Models\yolo26x.onnx` with the input/output contract described in Model Setup, and a working Windows ML CPU execution provider for the selected architecture.
- No camera is needed: these tests exercise the shared preprocessor and inference engine with synthetic bitmaps. They measure memory growth, not detection accuracy.

After warm-up, each test runs 1,000 operations. The allowed process-private memory growth after garbage collection is 32 MiB for preprocessing and 64 MiB for inference. Runtime/provider caching can affect these measurements. The tests disable parallel execution to avoid overlapping measurements. The separate `dotnet test` commands above additionally give each test a fresh process.

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
| Verification always mismatches | Make sure scanned values match the configured labels and the model class ids use the standard COCO ordering. Keep items inside the visible preview crop. |

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

Third-party packages retain their respective licenses. The former ImageSharp-specific notice was removed with that dependency. Model weights are not included in this repository and are licensed separately. Review the license of the specific model you use.