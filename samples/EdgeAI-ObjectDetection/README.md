# Edge AI - Object Detection

This sample performs object detection locally on a Windows IoT device using Windows ML and YOLO models. It identifies objects and their locations without sending camera frames to the cloud.

The sample is split into three projects:
1. [EdgeAI.ObjectDetection.Yolo](#edgeaiobjectdetectionyolo): a reusable WinUI control.
2. [EdgeAI-ObjectDetection](#edgeai-objectdetection): an app for trying models.
3. [EdgeAI-SelfCheckoutKiosk](#edgeai-selfcheckoutkiosk): a self-checkout demo that compares detected objects with a scanned cart.

## EdgeAI.ObjectDetection.Yolo

`EdgeAI.ObjectDetection.Yolo` is a WinUI class library containing the `YoloInferenceView` XAML user control. It manages camera capture and YOLO inference, displays a live preview or captured still with bounding boxes, and publishes detection results. Configurable text below the preview shows status, model/provider information, and inference timings.

This project does not run on its own; it is referenced from your WinUI app.

See the [API reference](EdgeAI.ObjectDetection.Yolo/README.md) for public types, properties, events, methods, and lifecycle guidance.

### Prerequisites

These requirements also apply to both sample apps.

| Requirement | Details |
| --- | --- |
| Device | Windows x64 or ARM64 device; compatible GPU/NPU hardware and drivers are optional. |
| Windows | Windows 11 24H2 or newer recommended for dynamic execution-provider installation. Initial execution-provider setup requires internet access. |
| Development tools | .NET 8 SDK. Visual Studio 2022 with WinUI/.NET desktop development tool workflow installed is recommended |
| Camera | Built-in camera or USB webcam |
| Windows App SDK | NuGet package `Microsoft.WindowsAppSDK` version **2.5.1**. |
| Windows ML | NuGet package `Microsoft.Windows.AI.MachineLearning` version **2.1.74**, providing the Windows ML runtime, ONNX Runtime integration, and DirectML support. |
| Windows SDK build tools | NuGet package `Microsoft.Windows.SDK.BuildTools` version **10.0.28000.2705**. |
| Model | YOLO model exported to ONNX, details described below |

#### Model setup

The YOLO model is not supplied in this sample. Obtain or export a YOLO model with the contract below, or modify the sample to fit your model's parameters.

| Property | Required value |
| --- | --- |
| Input | One float32 tensor, shape `[1,3,640,640]`; RGB, NCHW, values in `0–1`. |
| Output | One float32 tensor, shape `[1,300,6]`. Each row is `x1, y1, x2, y2, confidence, classId`; coordinates are model-input pixels. |
| Detection Classes | Standard 80-class COCO ordering, available from `YoloInferenceView.GetSupportedClasses()`. |

YOLO's default NMS-free exports and exports with embedded NMS have been tested to work with this sample.

Dynamic shapes, custom class mappings, and segmentation/pose outputs are not supported. 

Review the model's license separately; `.onnx` files are excluded from Git.

### Usage

Add a project reference to `EdgeAI.ObjectDetection.Yolo\EdgeAI.ObjectDetection.Yolo.csproj` from your WinUI app. 

When integrating the control into a packaged app, declare camera access (webcam) and desktop full-trust execution (runFullTrust) in the host app's package manifest. Refer to either sample app's Package.appxmanifest file for an example.

Add the namespace to your page or window, then place the control in its layout:

```xml
<!-- Add xmlns:ai="using:EdgeAI.ObjectDetection.Yolo" to the page/window. -->
<ai:YoloInferenceView x:Name="Yolo"
                      Width="800" Height="600"
                      PreviewStretch="Uniform" />
```

#### Stream Mode

The default is Stream mode, which will show a camera preview stream and continuously perform detections on it. 

Run the following from an async UI-thread handler after the view has loaded. Replace the model path with your own. This example uses the first camera and execution provider.

```csharp
await ExecutionProviderCatalog.GetDefault().EnsureAndRegisterCertifiedAsync();
var executionProvider = OrtEnv.Instance().GetEpDevices().First();

var cameras = await MediaFrameSourceGroup.FindAllAsync();
var camera = cameras.First();

InferenceSettings settings = new()
{
    ModelPath = @"C:\Models\model.onnx",
    FrameSourceGroup = camera,
    ExecutionProvider = executionProvider
};

await Yolo.StartAsync(settings);
```

Always run ``` await Yolo.StopAsync(); ``` before changing the control's settings.

#### OneShot Mode

Set the InferenceType to OneShot mode to perform inference against a single camera frame. The camera frame used for inference is returned as a bitmap alongside the detected objects to allow the app to post-process the image and/or detections.

`InferenceSnapshot.Detections` contains [image-space detections](EdgeAI.ObjectDetection.Yolo/README.md#imagedetection) (`ImageDetection`, with `ImageDetectionBox` coordinates in image pixels). OneShot returns these results without raising `DetectionsUpdated`. In Stream mode, that event provides [preview-space detections](EdgeAI.ObjectDetection.Yolo/README.md#previewdetection) (`PreviewDetection`, with `PreviewDetectionBox` coordinates in XAML DIPs).

```csharp
InferenceSettings oneShotSettings = new()
{
    ModelPath = @"C:\Models\model.onnx",
    FrameSourceGroup = camera,
    ExecutionProvider = executionProvider,
    InferenceType = InferenceType.OneShot
};
await Yolo.StartAsync(oneShotSettings);
using InferenceSnapshot snapshot = await Yolo.InferOnceAsync();
// Use snapshot.Image and snapshot.Detections before disposing the snapshot.
```

## EdgeAI-ObjectDetection

This WinUI app hosts the control so you can try compatible models, cameras, and execution providers without writing integration code. It supports continuous detection and one-shot capture.

### Prerequisites

Use the [shared prerequisites](#prerequisites) and [model requirements](#model-setup). Keep one or more compatible `.onnx` files in a folder of your choice; no particular filename or repository location is required.

### Usage

1. Open `EdgeAI-ObjectDetection.slnx` in this folder in Visual Studio.
2. Set **EdgeAI-ObjectDetection** as the startup project. Select **ARM64** or **x64** to match your device, then press **F5**.
3. Choose your model folder, then select a model, camera, and execution provider. Streaming starts once those selections are available.
4. To capture a still, select **One shot**, then **Run one inference**. Each click replaces the displayed image and detections.

Changing model, camera, provider, or mode stops and restarts inference. This app uses the exact selected provider; it does not automatically fall back to another device. If a provider is unavailable, check Windows/driver updates and network access for initial registration.

## EdgeAI-SelfCheckoutKiosk

This WinUI self-checkout demo uses EdgeAI.ObjectDetection.Yolo to compare detected objects with a scanned cart. It demonstrates local inference in an application workflow.

### Prerequisites

Use the [shared prerequisites](#prerequisites) and [model requirements](#model-setup). A supported HID barcode scanner is optional; **Keyboard Mode** lets you enter item names manually.

Before building, create `EdgeAI-SelfCheckoutKiosk\Models\` and copy a compatible `.onnx` model into it. Any filename is accepted. The build copies models into the app's output directory, where the model picker discovers them.

### Usage

1. Open `EdgeAI-ObjectDetection.slnx`, set **EdgeAI-SelfCheckoutKiosk** as the startup project, select **ARM64** or **x64**, and press **F5**.
2. Open the gear-button settings. Select a model and camera, and choose **Keyboard Mode** if you are not using a scanner. Hardware defaults to **Auto**, which tries NPU, then GPU, then CPU; an explicit hardware selection does not fall back.
3. Use **Edit Labels** to restrict which objects count toward verification.
4. Select **Start Now**, place objects in the preview, and scan items or type their labels and press Enter. This demo treats scanner input as an item name, not a product-database lookup.
5. Select **Pay Now** after the first inference result arrives. The app compares the cart with the latest completed streaming result, without capturing another frame or voting across frames.

Verification compares label presence. A mismatch shows the differing labels and keeps the live preview running; a match stops inference.

#### Deploy to another device

For a loose-file deployment, run from this folder, using either win-arm64 or win-x64 depending on your target device's architecture.

```powershell
dotnet publish EdgeAI-SelfCheckoutKiosk\EdgeAI-SelfCheckoutKiosk.csproj -c Release -r win-arm64 --self-contained true -o .\publish\win-arm64
```

## License

See [LICENSE](LICENSE). Model weights (not included in this solution) have their own licenses.
