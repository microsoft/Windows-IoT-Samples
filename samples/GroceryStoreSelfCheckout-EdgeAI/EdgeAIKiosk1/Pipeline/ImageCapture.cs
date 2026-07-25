using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EdgeAIKiosk1;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;

namespace EdgeAIKiosk1.Pipeline;

public sealed class ImageCapture
{
    // MediaCapture/MediaFrameReader stay in the WinRT camera stack, which avoids
    // native OpenCV/DirectShow dependencies and still exposes raw frames for NPU inference.
    private MediaCapture? _mediaCapture;
    private MediaFrameReader? _cameraFrameReader;
    private MediaFrameReference? _latestFrameReference;

    // Public status lets callers stop live loops when capture is no longer active.
    public bool IsInitialized { get; private set; }

    #region Startup

    /// <summary>Starts the camera preview and frame reader, then returns the preview media source.</summary>
    public async Task<MediaSource> StartPreview()
    {
        var frameSource = await this.InitializePreview();
        await this.StartFrameReader(frameSource);
        return MediaSource.CreateFromMediaFrameSource(frameSource);
    }

    /// <summary>Initializes MediaCapture and returns the color source used by preview and inference.</summary>
    private async Task<MediaFrameSource> InitializePreview()
    {
        try
        {
            // Initialize the selected video device.
            this._mediaCapture = new MediaCapture();
            await this._mediaCapture.InitializeAsync(await this.BuildSettings());
            this.IsInitialized = true;
        }
        catch (UnauthorizedAccessException exception)
        {
            this.Stop();
            throw new InvalidOperationException("Camera permission denied. Enable camera access in Windows Settings.", exception);
        }
        catch
        {
            this.Stop();
            throw;
        }

        // Resolve the color frame source used by preview and inference.
        return this.GetColorFrameSource();
    }

    /// <summary>Builds settings for the selected video device or the first available device.</summary>
    private async Task<MediaCaptureInitializationSettings> BuildSettings()
    {
        var devices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(
            Windows.Media.Devices.MediaDevice.GetVideoCaptureSelector());
        var selectedDeviceId = SelectCameraId(devices.Select(device => device.Id).ToList(), KioskSettings.CameraDeviceId);

        // Video-only capture keeps microphone/photo permissions and device setup out of the kiosk path.
        var settings = new MediaCaptureInitializationSettings();
        settings.VideoDeviceId = selectedDeviceId;
        settings.StreamingCaptureMode = StreamingCaptureMode.Video;
        return settings;
    }

    internal static string SelectCameraId(IReadOnlyList<string> deviceIds, string? selectedDeviceId)
    {
        if (deviceIds.Count == 0)
            throw new InvalidOperationException("No camera detected. Connect a camera and try again.");

        return deviceIds.FirstOrDefault(id => id == selectedDeviceId) ?? deviceIds[0];
    }

    private MediaFrameSource GetColorFrameSource() =>
        this._mediaCapture!.FrameSources.Values.First(this.IsVideoSource);

    private bool IsVideoSource(MediaFrameSource source) =>
        source.Info.SourceKind == MediaFrameSourceKind.Color &&
        (source.Info.MediaStreamType == MediaStreamType.VideoRecord ||
        source.Info.MediaStreamType == MediaStreamType.VideoPreview);

    /// <summary>Starts the frame reader used later by capture requests.</summary>
    /// <param name="frameSource">The color camera source returned during preview startup.</param>
    private async Task StartFrameReader(MediaFrameSource frameSource)
    {
        // BGRA8 is widely supported by Windows camera drivers and cheap to copy into
        // SoftwareBitmap/ImageSharp before converting to the model's RGB tensor.
        this._cameraFrameReader = await this._mediaCapture!.CreateFrameReaderAsync(frameSource, MediaEncodingSubtypes.Bgra8);
        await this._cameraFrameReader.StartAsync();
    }

    #endregion

    #region Capture

    /// <summary>Captures the latest camera frame as a CPU bitmap when the reader has one ready.</summary>
    public async Task<SoftwareBitmap?> CaptureFrame()
    {
        this._latestFrameReference?.Dispose();
        this._latestFrameReference = await Task.Run(() => this._cameraFrameReader!.TryAcquireLatestFrame());
        return await this.ConvertToBitmap(this._latestFrameReference);
    }

    /// <summary>Converts a camera frame reference into a CPU-readable bitmap when possible.</summary>
    /// <param name="frame">The latest MediaFrameReader frame, which may contain either a SoftwareBitmap or a GPU surface.</param>
    private async Task<SoftwareBitmap?> ConvertToBitmap(MediaFrameReference? frame)
    {
        // Prefer CPU bitmap frames when MediaFrameReader already provides one.
        var videoFrame = frame?.VideoMediaFrame?.GetVideoFrame();
        if (videoFrame?.SoftwareBitmap is not null) return videoFrame.SoftwareBitmap;

        // Fall back to copying GPU-backed frames.
        if (videoFrame?.Direct3DSurface is not null)
            return await SoftwareBitmap.CreateCopyFromSurfaceAsync(videoFrame.Direct3DSurface);
        return null;
    }

    #endregion

    #region Shutdown

    /// <summary>Disposes the active camera objects and marks capture as stopped.</summary>
    public void Stop()
    {
        this._latestFrameReference?.Dispose();
        this._cameraFrameReader?.Dispose();
        this._mediaCapture?.Dispose();
        this.IsInitialized = false;
    }

    #endregion
}
