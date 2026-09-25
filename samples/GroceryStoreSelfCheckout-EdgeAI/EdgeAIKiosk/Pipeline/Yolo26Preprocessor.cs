using System;
using System.Runtime.InteropServices.WindowsRuntime;
using EdgeAIKiosk.Interfaces;
using EdgeAIKiosk.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Windows.Graphics.Imaging;

namespace EdgeAIKiosk.Pipeline;

public sealed class Yolo26Preprocessor : IModelPreprocessor
{
    // YOLO models are trained at a fixed square input size; matching that shape avoids
    // rescaling drift between training, ONNX export, and runtime inference.
    public int TargetWidth { get; } = 640;
    public int TargetHeight { get; } = 640;
    public int Channels { get; } = 3;

    #region Main entrypoint

    /// <summary>Converts a camera bitmap into the tensor and letterbox metadata expected by YOLO26.</summary>
    /// <param name="frame">The SoftwareBitmap captured from the WinRT camera pipeline.</param>
    public ModelInput Preprocess(SoftwareBitmap frame)
    {
        using var sourceImage = this.ConvertToImage(frame);
        var (letterboxed, padLeft, padTop, scale) = this.LetterboxResize(sourceImage);
        using (letterboxed)
        {
            return this.BuildTensor(letterboxed, padLeft, padTop, scale);
        }
    }

    #endregion

    #region Steps

    /// <summary>Converts the WinRT bitmap into an ImageSharp RGB image.</summary>
    /// <param name="frame">The camera frame to copy out of WinRT bitmap memory.</param>
    private Image<Rgb24> ConvertToImage(SoftwareBitmap frame)
    {
        // SoftwareBitmap is the WinRT camera format; ImageSharp gives portable pixel access
        // on ARM64 without adding an OpenCV native binary dependency.
        using var rgbaBitmap = SoftwareBitmap.Convert(frame, BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore);
        var rgbaBuffer = new byte[rgbaBitmap.PixelWidth * rgbaBitmap.PixelHeight * 4];
        rgbaBitmap.CopyToBuffer(rgbaBuffer.AsBuffer());
        return this.DecodeRgba(rgbaBuffer, rgbaBitmap.PixelWidth, rgbaBitmap.PixelHeight);
    }

    /// <summary>Copies RGBA bytes into an RGB image while dropping the alpha channel.</summary>
    /// <param name="rgbaBuffer">Raw RGBA bytes copied from the SoftwareBitmap.</param>
    private Image<Rgb24> DecodeRgba(byte[] rgbaBuffer, int imageWidth, int imageHeight)
    {
        var rgbImage = new Image<Rgb24>(imageWidth, imageHeight);

        // The model was trained on RGB pixels; alpha is camera/container metadata, not
        // visual evidence, so it is dropped before normalization.
        rgbImage.ProcessPixelRows(pixelAccessor =>
        {
            for (int y = 0; y < imageHeight; y++)
            {
                var row = pixelAccessor.GetRowSpan(y);
                for (int x = 0; x < imageWidth; x++)
                {
                    int rgbaOffset = (y * imageWidth + x) * 4;
                    row[x] = new Rgb24(rgbaBuffer[rgbaOffset], rgbaBuffer[rgbaOffset + 1], rgbaBuffer[rgbaOffset + 2]);
                }
            }
        });
        return rgbImage;
    }

    /// <summary>Resizes the source image onto a square YOLO canvas while preserving aspect ratio.</summary>
    /// <param name="sourceImage">The decoded camera image; this method mutates it during resize.</param>
    private (Image<Rgb24>, float padLeft, float padTop, float scale) LetterboxResize(Image<Rgb24> sourceImage)
    {
        // Letterboxing preserves object shape; stretching the image would move boxes away
        // from the coordinates the YOLO head was trained to predict.
        float resizeScale = Math.Min((float)this.TargetWidth / sourceImage.Width, (float)this.TargetHeight / sourceImage.Height);
        var (resizedWidth, resizedHeight) = ((int)(sourceImage.Width * resizeScale), (int)(sourceImage.Height * resizeScale));
        var (leftPadding, topPadding) = ((this.TargetWidth - resizedWidth) / 2f, (this.TargetHeight - resizedHeight) / 2f);
        sourceImage.Mutate(imageOperation => imageOperation.Resize(resizedWidth, resizedHeight));

        // 114 gray is the standard YOLO letterbox fill: neutral enough not to look like
        // a strong edge or object class while keeping preprocessing consistent with training.
        var letterboxCanvas = new Image<Rgb24>(this.TargetWidth, this.TargetHeight, new Rgb24(114, 114, 114));
        letterboxCanvas.Mutate(imageOperation => imageOperation.DrawImage(sourceImage, new Point((int)leftPadding, (int)topPadding), 1f));
        return (letterboxCanvas, leftPadding, topPadding, resizeScale);
    }

    /// <summary>Builds the channel-first tensor and stores padding metadata used to unscale detections later.</summary>
    /// <param name="letterboxedImage">The resized image placed on the fixed-size YOLO input canvas.</param>
    private ModelInput BuildTensor(Image<Rgb24> letterboxedImage, float padLeft, float padTop, float scale)
    {
        // Fill CHW tensor data.
        var tensor = new float[this.Channels * this.TargetHeight * this.TargetWidth];
        this.FillChwTensor(letterboxedImage, tensor);

        // Keep the letterbox math with the tensor so output boxes can be mapped back to
        // the original camera frame instead of the padded model canvas.
        var input = new ModelInput(tensor, this.TargetWidth, this.TargetHeight, this.Channels);
        input = input with { PadLeft = padLeft, PadTop = padTop, Scale = scale };
        return input;
    }

    #endregion

    #region Helpers

    /// <summary>Writes RGB pixels into the supplied NCHW tensor buffer.</summary>
    /// <param name="letterboxedImage">The image whose pixels should be normalized into tensor data.</param>
    /// <param name="tensor">The preallocated tensor buffer ordered as all R, then all G, then all B values.</param>
    private void FillChwTensor(Image<Rgb24> letterboxedImage, float[] tensor)
    {
        // ONNX vision models commonly consume NCHW/CHW tensors even though images are
        // stored row-major HWC; writing channels contiguously matches the exported model.
        letterboxedImage.ProcessPixelRows(pixelAccessor =>
        {
            for (int y = 0; y < this.TargetHeight; y++)
            {
                var row = pixelAccessor.GetRowSpan(y);
                for (int x = 0; x < this.TargetWidth; x++)
                {
                    int tensorOffset = y * this.TargetWidth + x;
                    tensor[tensorOffset] = row[x].R / 255f;
                    tensor[this.TargetWidth * this.TargetHeight + tensorOffset] = row[x].G / 255f;
                    tensor[2 * this.TargetWidth * this.TargetHeight + tensorOffset] = row[x].B / 255f;
                }
            }
        });
    }

    #endregion
}
