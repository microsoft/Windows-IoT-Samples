# EdgeAI.ObjectDetection.Yolo Reference

`EdgeAI.ObjectDetection.Yolo` is a WinUI class library for camera-based YOLO object detection using Windows ML. Its [`YoloInferenceView`](#yoloinferenceview) control manages camera capture, inference, preview rendering, and detection overlays.

All public types documented here belong to the `EdgeAI.ObjectDetection.Yolo` namespace. See the [sample overview](../README.md#prerequisites) for development prerequisites and package versions.

## YoloInferenceView

```csharp
public sealed partial class YoloInferenceView : UserControl, IAsyncDisposable
```

The XAML control that owns capture, inference, preview, and overlay resources.

### Constructors

| Signature | Description |
| --- | --- |
| `YoloInferenceView()` | Initializes the control in the `Stopped` state. Does not open a camera or load a model. |

### Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `State` | [`YoloInferenceState`](#yoloinferencestate) | [`Stopped`](#yoloinferencestate) | Read-only lifecycle state. |

### DependencyProperties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `PreviewStretch` | `Stretch` | `UniformToFill` | Preview scaling and camera-region selection. Supports `None`, `Uniform`, and `UniformToFill`. Change only while stopped; `Fill` is not supported. |
| `ShowCameraPreview` | `bool` | `true` | Shows the live preview or captured still. Hiding it does not stop capture or inference. |
| `ShowBoundingBoxes` | `bool` | `true` | Shows detection rectangles. |
| `ShowLabels` | `bool` | `true` | Shows class names. Independent of confidence visibility. |
| `ShowConfidence` | `bool` | `true` | Shows confidence as a percentage, even if class names are hidden. |
| `ShowInferenceStatus` | `bool` | `true` | Shows startup, model/provider, and error status text. |
| `ShowInferenceTime` | `bool` | `true` | Shows the inference duration reported by the pipeline. |
| `ShowEndToEndInferenceTime` | `bool` | `true` | Shows elapsed time from the pipeline's frame-processing start through result presentation. This is not camera sensor-to-display latency. |

### Events

| Event | Type | Raised when |
| --- | --- | --- |
| `StateChanged` | `EventHandler` | The lifecycle state changes. Read [`State`](#properties) for the new value. |
| `DetectionsUpdated` | `EventHandler<`[`DetectionsUpdatedEventArgs`](#detectionsupdatedeventargs)`>` | A completed [`Stream`](#inferencetype) result is presented. Detections use preview coordinates, including when the built-in overlays are hidden. Not raised in OneShot mode. |
| `Faulted` | `EventHandler<`[`InferenceFaultedEventArgs`](#inferencefaultedeventargs)`>` | An active capture, preview, or inference failure has been processed and cleanup attempted. The control transitions to [`Stopped`](#yoloinferencestate) before notification. |

Events are delivered on the control's UI thread. Keep handlers short and handle exceptions in your own handlers. [`Faulted`](#events) does not replace handling exceptions from awaited lifecycle methods.

### Methods

#### StartAsync

```csharp
public Task StartAsync(
    InferenceSettings settings,
    CancellationToken cancellationToken = default)
```

Validates settings, loads the model with the selected execution provider, initializes the camera, and starts the requested mode.

- Call on the UI thread after the control is loaded and has a usable layout.
- Requires [`State`](#properties) to be [`YoloInferenceState.Stopped`](#yoloinferencestate).
- Completes when capture and inference are ready, not when the first detection arrives.
- In [`Stream`](#inferencetype) mode, starts the continuous inference loop.
- In [`OneShot`](#inferencetype) mode, prepares capture; call [`InferOnceAsync`](#inferonceasync) to produce a result.
- The cancellation token applies to startup. After startup completes, use [`StopAsync`](#stopasync) to stop the active session.

Invalid settings raise argument exceptions. Calling while not stopped or without a supported color stream raises `InvalidOperationException`. A missing model raises `FileNotFoundException`; model, execution-provider, and camera initialization errors also propagate to the caller. Cancellation raises `OperationCanceledException`, and use after disposal raises `ObjectDisposedException`.

If initialization fails, the control logs the error, attempts cleanup, sets its state to [`Stopped`](#yoloinferencestate), displays a startup error, and rethrows. If cleanup also fails, the returned task faults with an `AggregateException` containing both errors.

#### StopAsync

```csharp
public Task StopAsync()
```

Cancels startup or active work, waits for it to finish, and releases capture, model, and preview resources. Clears the displayed result and finishes in [`Stopped`](#yoloinferencestate).

- Call and await on the UI thread before restarting with different [inference settings](#inferencesettings) or changing [`PreviewStretch`](#dependencyproperties).
- Applies to both [`Stream`](#inferencetype) and [`OneShot`](#inferencetype).
- Safe to call when already stopped. After disposal, it has no effect.
- Cleanup errors are logged and propagated to the caller.

#### InferOnceAsync

```csharp
public Task<InferenceSnapshot> InferOnceAsync(
    CancellationToken cancellationToken = default)
```

Processes an available camera frame, displays the captured still and overlays, and returns an owned image with its detections. Does not raise [`DetectionsUpdated`](#events); read results from the returned [`InferenceSnapshot`](#inferencesnapshot).

- Requires a running session started with [`InferenceType.OneShot`](#inferencetype); otherwise throws `InvalidOperationException`.
- Call on the UI thread. Requests are serialized.
- Waits for a processable frame and valid preview geometry. No built-in timeout is applied; supply a cancellation token when a deadline is needed.
- The control remains running after completion. Repeat this method without restarting to capture another frame.
- Dispose the returned [`InferenceSnapshot`](#inferencesnapshot) when finished with its image.

Cancellation, including cancellation caused by stopping the control, propagates as `OperationCanceledException`. Processing failures propagate to the caller and also initiate runtime fault handling. Use after disposal raises `ObjectDisposedException`.

#### DisposeAsync

```csharp
public ValueTask DisposeAsync()
```

Permanently releases the control's resources and finishes in [`Disposed`](#yoloinferencestate). Call and await on the UI thread when the host no longer needs the control. Repeated disposal is safe. Cleanup errors propagate.

Unloading the control does not automatically stop or dispose it. A disposed instance cannot be restarted.

#### GetSupportedClasses

```csharp
public static IReadOnlyList<string> GetSupportedClasses()
```

Returns the read-only, standard 80-class COCO label list in class-ID order. The returned list is independent of [`InferenceSettings.ClassMask`](#inferencesettings).

#### IsSupportedCamera

```csharp
public static bool IsSupportedCamera(MediaFrameSourceGroup camera)
```

Returns `true` if the camera group advertises at least one color `VideoPreview` or `VideoRecord` source. This optional discovery filter does not open the camera, check permissions, or guarantee successful capture. A null argument throws `ArgumentNullException`.

Startup independently selects a supported color stream, preferring preview over recording.

## InferenceSettings

```csharp
public sealed record InferenceSettings
```

Configuration for one running session. Properties are init-only; create a new instance or use a record `with` expression to change settings between sessions.

### Constructors

`InferenceSettings()` uses an object initializer to supply the three required properties below.

### Properties

| Property | Type | Default | Description |
| --- | --- | --- | --- |
| `ModelPath` | `string` | Required | Nonempty path to a compatible ONNX model. The file must exist when starting. |
| `FrameSourceGroup` | `MediaFrameSourceGroup` | Required | Camera group to capture from. Must expose a color preview or recording stream. |
| `ExecutionProvider` | `OrtEpDevice` | Required | Execution-provider device passed to ONNX Runtime. The host discovers and registers providers before starting. |
| `InferenceType` | [`InferenceType`](#inferencetype) | [`Stream`](#inferencetype) | Continuous streaming or caller-triggered one-shot inference. |
| `MaxEndToEndFps` | `double` | `30` | Streaming inference-rate limit. Must be finite and between `0.1` and `240`, inclusive. Does not set the camera frame rate or schedule one-shot calls. |
| `ClassMask` | `IReadOnlyCollection<string>?` | `null` | Optional class-name filter, matched case-insensitively against [`GetSupportedClasses()`](#getsupportedclasses). |

For `ClassMask`, `null` includes all supported classes, an empty collection includes none, and unknown names are ignored. The class selection is copied during startup: do not mutate the collection during [`StartAsync`](#startasync); changes afterward do not affect the active session.

The control does not choose a provider or implement a hardware-selection/fallback policy. It passes the supplied device to ONNX Runtime; registration alone does not guarantee model compatibility.

## InferenceSnapshot

```csharp
public sealed class InferenceSnapshot : IDisposable
```

A one-shot result returned by [`InferOnceAsync`](#inferonceasync). It has no public constructor.

### Properties

| Property | Type | Description |
| --- | --- | --- |
| `Image` | `SoftwareBitmap` | Read-only, owned 640x640 model-input image in BGRA8 format with premultiplied alpha. Includes letterbox padding but no drawn bounding boxes. Throws `ObjectDisposedException` after disposal. |
| `Detections` | `IReadOnlyList<`[`ImageDetection`](#imagedetection)`>` | Read-only detections in image-pixel coordinates. Remains accessible after the snapshot image is disposed. |

### Methods

| Signature | Description |
| --- | --- |
| `void Dispose()` | Disposes the owned image. Repeated calls are safe. Dispose the snapshot rather than disposing its image separately. |

## ImageDetection

In [`OneShot`](#inferencetype) mode, this describes the detection returned when calling [`InferOnceAsync`](#inferonceasync). Access the `ImageDetection` through [`InferenceSnapshot.Detections`](#inferencesnapshot).

### Constructors

```csharp
public ImageDetection(int ClassId, string Label, float Confidence, ImageDetectionBox BoundingBox)
```

### Properties

| Property | Type | Description |
| --- | --- | --- |
| `ClassId` | `int` | Zero-based COCO class ID. |
| `Label` | `string` | Corresponding COCO class name. |
| `Confidence` | `float` | Score from the model. Results emitted by the control have scores from `0.5` to `1`, inclusive. |
| `BoundingBox` | [`ImageDetectionBox`](#imagedetectionbox) | Position and dimensions in the 640x640 snapshot image. |

These positional record properties have `get` and `init` accessors.

### ImageDetectionBox

```csharp
public readonly record struct ImageDetectionBox(float X, float Y, float Width, float Height)
```

The positional constructor initializes four `float` properties with `get` and `init` accessors. `X` and `Y` are the upper-left corner; `Width` and `Height` are dimensions. Units are pixels relative to the 640x640 snapshot image's top-left, including letterbox padding. Fractional pixels are preserved.

Default initialization produces zero-valued coordinates and dimensions.

## PreviewDetection

In [`Stream`](#inferencetype) mode, this describes the detection returned in the [`DetectionsUpdated`](#events) event. Access the `PreviewDetection` through [`DetectionsUpdatedEventArgs`](#detectionsupdatedeventargs)`.Detections`.

### Constructors

```csharp
public PreviewDetection(int ClassId, string Label, float Confidence, PreviewDetectionBox BoundingBox)
```

### Properties

| Property | Type | Description |
| --- | --- | --- |
| `ClassId` | `int` | Zero-based COCO class ID. |
| `Label` | `string` | Corresponding COCO class name. |
| `Confidence` | `float` | Model confidence, unchanged by coordinate mapping. |
| `BoundingBox` | [`PreviewDetectionBox`](#previewdetectionbox) | Position and dimensions in preview-relative XAML DIPs. |

These positional record properties have `get` and `init` accessors.

### PreviewDetectionBox

```csharp
public readonly record struct PreviewDetectionBox(double X, double Y, double Width, double Height)
```

The positional constructor initializes four `double` properties with `get` and `init` accessors. Coordinates and dimensions are XAML device-independent pixels (DIPs), relative to the preview area's top-left, excluding the status text below it.

Default initialization produces zero-valued coordinates and dimensions.

## DetectionsUpdatedEventArgs

A sealed `EventArgs` subclass for completed [`Stream`](#inferencetype) results.

### Constructors

```csharp
public DetectionsUpdatedEventArgs(IReadOnlyList<PreviewDetection> detections)
```

### Properties

| Property | Type | Description |
| --- | --- | --- |
| `Detections` | `IReadOnlyList<`[`PreviewDetection`](#previewdetection)`>` | Read-only list for the layout at the time of the event. May be empty. |

## InferenceFaultedEventArgs

A sealed `EventArgs` subclass for runtime failures.

### Constructors

```csharp
public InferenceFaultedEventArgs(Exception exception)
```

### Properties

| Property | Type | Description |
| --- | --- | --- |
| `Exception` | `Exception` | Read-only failure. May be an `AggregateException` if cleanup also failed. |

## Enumerations

### InferenceType

| Value | Numeric value | Meaning |
| --- | --- | --- |
| `Stream` | `0` | Continuously process camera frames. |
| `OneShot` | `1` | Process a camera frame when [`InferOnceAsync`](#inferonceasync) is called. |

### YoloInferenceState

| Value | Numeric value | Meaning |
| --- | --- | --- |
| `Stopped` | `0` | No active session; the control can be started. |
| `Starting` | `1` | Model and capture initialization is underway. |
| `Running` | `2` | Streaming, or ready for one-shot requests. |
| `Stopping` | `3` | Resources are being released. |
| `Disposed` | `4` | Permanently retired; cannot be restarted. |

## Remarks

### Lifecycle and error handling

Call lifecycle methods from the control's UI thread and await their completion. Start after the control is loaded with a nonzero preview area; inference depends on the preview geometry.

- Await [`StopAsync`](#stopasync) before starting a new session, including changing model, camera, provider, class mask, or inference mode.
- One-shot completion does not stop the session. Repeat [`InferOnceAsync`](#inferonceasync) with the same settings without stopping.
- Receive Stream results through [`DetectionsUpdated`](#events). Receive OneShot results from the snapshot returned by [`InferOnceAsync`](#inferonceasync); it does not also raise the event.
- Catch startup and one-shot exceptions in the host and report them to the user. Subscribe to [`Faulted`](#events) for custom handling of runtime failures; the control also logs errors and updates its built-in status.
- Stop/dispose requests cancel active work. Cancellation of a single [`InferOnceAsync`](#inferonceasync) request does not itself stop an otherwise running one-shot session.
- Await [`DisposeAsync`](#disposeasync) when permanently removing the control; a XAML unload alone does not release its resources.

### Images, coordinates, and model contract

The control selects the visible camera region, resizes it proportionally, and letterboxes it into a 640x640 input. 

The [`PreviewStretch`](#dependencyproperties) property determines the visible region: `Uniform` includes the full camera image; `UniformToFill` can crop it. `None` uses native-pixel scaling, accounting for display rasterization scale, and can also crop to the viewport.

The ONNX model must have one float32 input shaped `[1,3,640,640]` (RGB, NCHW, normalized to `0-1`) and one float32 output shaped `[1,300,6]`. Each output row is `x1, y1, x2, y2, confidence, classId`, using model-input pixels and standard COCO class IDs. The output must already contain processed detections; the control does not perform non-maximum suppression. Confidence below `0.5`, invalid rows, and classes excluded by [`ClassMask`](#inferencesettings) are filtered out.

| Result | Element type | Coordinate space | Intended use |
| --- | --- | --- | --- |
| [`DetectionsUpdatedEventArgs.Detections`](#detectionsupdatedeventargs) | [`PreviewDetection`](#previewdetection) | Preview-relative DIPs | Stream-mode application logic or custom overlays aligned to the live preview. |
| [`InferenceSnapshot.Detections`](#inferencesnapshot) | [`ImageDetection`](#imagedetection) | 640x640 image pixels | Processing or annotating [`InferenceSnapshot.Image`](#inferencesnapshot). |

Preview coordinates describe the layout when the event was raised. Resizing can make saved coordinates stale; resizing alone does not raise another [`DetectionsUpdated`](#events) event.

In one-shot mode, the captured model-input image is displayed as a fitted square with overlays. The returned bitmap itself has no overlays and is not the original full-resolution camera frame. Disposing the returned snapshot does not remove the control's displayed still.

### Example: start streaming

Add `xmlns:ai="using:EdgeAI.ObjectDetection.Yolo"` to the containing XAML page or window:

```xml
<ai:YoloInferenceView x:Name="Yolo"
                      Width="800" Height="600"
                      PreviewStretch="Uniform" />
```

The following omits imports and host-specific error handling. It assumes a model at the supplied path, an available camera with a color stream, and a compatible registered provider. Run it from an async UI-thread handler after the view is loaded.

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

`ExecutionProviderCatalog` belongs to `Microsoft.Windows.AI.MachineLearning`, `OrtEnv` to `Microsoft.ML.OnnxRuntime`, and `MediaFrameSourceGroup` to `Windows.Media.Capture.Frames`. `First()` is a LINQ extension. Selecting the first provider does not mean selecting the fastest device.

### Example: switch to one-shot capture

Reuse the settings from the preceding example:

```csharp
await Yolo.StopAsync();
await Yolo.StartAsync(settings with { InferenceType = InferenceType.OneShot });

using InferenceSnapshot snapshot = await Yolo.InferOnceAsync();
// Process snapshot.Image and snapshot.Detections within this scope.
```

Use or copy the image before the `using` scope ends. To capture another frame, call [`InferOnceAsync`](#inferonceasync) again and dispose that result as well.

### Example: restrict detection classes

```csharp
await Yolo.StopAsync();
await Yolo.StartAsync(settings with { ClassMask = new[] { "apple", "banana", "orange" } });
```

This filters returned detections and overlays; it does not change the model or train new classes.
