// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Code derived from below UWP sample
// https://github.com/microsoft/Windows-universal-samples/blob/master/Samples/CameraStarterKit

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Azure.Devices.Client;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;

namespace DeviceCustomVisionUWPApp
{
    public sealed partial class MainPage : Page
    {
        private DeviceClient deviceClient;
        private static EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
        private static bool sendingFrames = false;
        private string leafDeviceConnectionString = string.Empty;
        private string gatewayDNSName = string.Empty;

        /// <summary>
        /// Lock object for implementing threadsafe property.
        /// </summary>
        public static readonly object SendingFramesLock = new object();

        /// <summary>
        /// Thread safe SendingFrames property.
        /// </summary>
        public static bool SendingFrames
        {
            get
            {
                lock (SendingFramesLock)
                {
                    return sendingFrames;
                }
            }
            set
            {
                lock (SendingFramesLock)
                {
                    sendingFrames = value;
                }
            }
        }

        private string UtcDateTime
        {
            get
            {
                return DateTime.Now.ToUniversalTime().ToString("G");
            }
        }

        /// <summary>
        /// This method will be invoked upon direct method (LeafDeviceDirectMethod) call from edge.
        /// </summary>
        /// <param name="methodRequest">direct method request object contains the details of request.</param>
        /// <param name="userContext">Context object passed in when the callback was registered.</param>
        /// <returns>MethodResponse task.</returns>
        /// This method will be invoked upon direct method (LeafDeviceDirectMethod) call.
        private Task<MethodResponse> DeviceCallback(MethodRequest methodRequest, object userContext)
        {
            var data = Encoding.UTF8.GetString(methodRequest.Data);
            if (!String.IsNullOrEmpty(data))
            {
                if(data.Contains("predictions"))
                {
                    try
                    {
                        var o = JObject.Parse(JsonConvert.DeserializeObject<JValue>(data).ToString());
                        if (o != null)
                        {
                            var maxValue = (from predictionResult in o["predictions"]
                                            orderby (float)predictionResult["probability"] descending
                                            select predictionResult).First();
                            string tag = (string)maxValue["tagName"];
                            float probability = (float)maxValue["probability"];
                            data = $"tagname: {tag}\nprobability:{probability:#0.##%}";
                        }
                    }
                    catch (Exception ex)
                    {
                        DisplayLog(ex.Message);
                    }
                }
                DisplayLog($"{UtcDateTime} Received:{data}");
                DisplayErrorOrResponse(data);
                string jString = JsonConvert.SerializeObject("Success");
                eventWaitHandle.Set();
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(jString), 200));
            }
            else
            {
                DisplayLog($"{UtcDateTime} Received:No reply");
                DisplayErrorOrResponse("No reply");
                string jString = JsonConvert.SerializeObject("EmptyData");
                return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(jString), 400));
            }
        }

        /// <summary>
        /// Initializes the Azure device connectivity.
        /// </summary>
        private void InitializeAzureDeviceConnection()
        {
            try
            {
                string deviceConnectionString = leafDeviceConnectionString + ";GatewayHostName=" + gatewayDNSName;
                const string azureIotTestRootCertificateFilePath = "azure-iot-test-only.root.ca.cert.pem";
                CertificateManager.InstallCACert(azureIotTestRootCertificateFilePath);

                deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString);
                deviceClient.SetMethodHandlerAsync("LeafDeviceDirectMethod", DeviceCallback, null);
                DisplayLog($"{UtcDateTime} Azure device connection established successfully.");
            }
            catch
            {
                deviceClient = null;
                DisplayLog($"Please input vaild connection string or gateway DNS!");
            }
        }

        private async void DisplayLog(string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                TextBlockLog.Text = message + "\n" + TextBlockLog.Text;
            });
        }

        private async void DisplayErrorOrResponse(string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                TextBlockPredictions.Text = message;
            });
        }

        private void InitializeGatewayDeviceConnectionButtonClick(object sender, RoutedEventArgs e)
        {
            leafDeviceConnectionString = LeafDeviceConnectionStringTextBox.Text.Trim();
            gatewayDNSName = GatewayDNSNameTextBox.Text.Trim();
            InitializeAzureDeviceConnection();
            ConfigureGatewayDevicePopup.IsOpen = false;
        }

        private void ConfigureGatewayDeviceButtonClick(object sender, RoutedEventArgs e)
        {
            ConfigureGatewayDevicePopup.IsOpen = true;
        }

        private void ConfigureGatewayDevicePopupClosed(object sender, object e)
        {
            if (deviceClient == null)
            {
                DisplayLog($"{UtcDateTime} Please input vaild connection string or gateway DNS!");
                ConfigureGatewayDevicePopup.IsOpen = true;
            }
            else
            {
                LeafDeviceConnectionStringTextBox.Text = leafDeviceConnectionString;
                GatewayDNSNameTextBox.Text = gatewayDNSName;
            }
        }

        /// <summary>
        /// Uses the DeviceClient to send a message to the IoT Hub
        /// </summary>
        /// <param name="deviceClient">Azure Devices client for connecting and send data to IoT Hub</param>
        /// <param name="message">JSON string representing serialized device data</param>
        /// <returns>Task for async execution</returns>
        private async void SendEvent(DeviceClient deviceClient, string message)
        {
            using (var eventMessage = new Message(Encoding.UTF8.GetBytes(message)))
            {
                // Set the content type and encoding so the IoT Hub knows to treat the message body as JSON
                eventMessage.ContentEncoding = "utf-8";
                eventMessage.ContentType = "image/jpeg";
                eventMessage.MessageId = Guid.NewGuid().ToString("D");
                await deviceClient.SendEventAsync(eventMessage).ConfigureAwait(false);
            }
        }

        // Rotation metadata to apply to the preview stream and recorded videos (MF_MT_VIDEO_ROTATION)
        // Reference: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        // Prevent the screen from sleeping while the camera is running
        private readonly DisplayRequest _displayRequest = new DisplayRequest();

        // MediaCapture and its state variables
        private MediaCapture _mediaCapture;
        private bool _isInitialized;
        private bool _isPreviewing;

        // UI state
        private bool _isSuspending;
        private bool _isActivePage;
        private bool _isUIActive;
        private Task _setupTask = Task.CompletedTask;

        // Information about the camera device
        private bool _mirroringPreview;
        private bool _externalCamera;

        // Rotation Helper to simplify handling rotation compensation for the camera streams
        private CameraRotationHelper _rotationHelper;

        #region Constructor, lifecycle and navigation

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeAzureDeviceConnection();
            // Do not cache the state of the UI when suspending/navigating
            NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        private void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            _isSuspending = false;

            var deferral = e.SuspendingOperation.GetDeferral();
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                await SetUpBasedOnStateAsync();
                deferral.Complete();
            });
        }

        private void Application_Resuming(object sender, object o)
        {
            _isSuspending = false;

            var task = Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                await SetUpBasedOnStateAsync();
            });
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Useful to know when to initialize/clean up the camera
            Application.Current.Suspending += Application_Suspending;
            Application.Current.Resuming += Application_Resuming;
            Window.Current.VisibilityChanged += Window_VisibilityChanged;

            _isActivePage = true;
            await SetUpBasedOnStateAsync();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            // Handling of this event is included for completenes, as it will only fire when navigating between pages and this sample only includes one page
            Application.Current.Suspending -= Application_Suspending;
            Application.Current.Resuming -= Application_Resuming;
            Window.Current.VisibilityChanged -= Window_VisibilityChanged;

            _isActivePage = false;
            await SetUpBasedOnStateAsync();
        }

        #endregion Constructor, lifecycle and navigation


        #region Event handlers
        private async void Window_VisibilityChanged(object sender, VisibilityChangedEventArgs args)
        {
            await SetUpBasedOnStateAsync();
        }

        /// <summary>
        /// Send camera frame images to IoT edge module for analysis.
        /// </summary>
        private async void SendFrames()
        {
            SendingFrames = true;
            do
            {
                await TakePhotoAsync();
                // Set timeout in case edge not responded.
                if(!eventWaitHandle.WaitOne(30000))
                {
                    DisplayLog("Edge not responding for 30 seconds, sending another frame for classification.");
                }
            } while (SendingFrames);
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                PhotoButton.Content = "Start";
            });
        }

        private void SendFramesButton_Click(object sender, RoutedEventArgs e)
        {
            if (PhotoButton.Content.ToString() == "Start")
            {
                PhotoButton.Content = "Stop";
                // Running sending frames in background thread.
                Thread t1 = new Thread(() =>
                {
                    SendFrames();
                });
                t1.Start();
            }
            else
            {
                sendingFrames = false;
                PhotoButton.Content = "Stopping";
            }
        }

        private async void MediaCapture_Failed(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            Debug.WriteLine("MediaCapture_Failed: (0x{0:X}) {1}", errorEventArgs.Code, errorEventArgs.Message);
            await CleanupCameraAsync();

        }

        #endregion Event handlers


        #region MediaCapture methods

        /// <summary>
        /// Initializes the MediaCapture, registers events, gets camera device information for mirroring and rotating, starts preview and unlocks the UI
        /// </summary>
        /// <returns></returns>
        private async Task InitializeCameraAsync()
        {
            Debug.WriteLine("InitializeCameraAsync");

            if (_mediaCapture == null)
            {
                // Attempt to get the back camera if one is available, but use any camera device if not
                var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Back);

                if (cameraDevice == null)
                {
                    Debug.WriteLine("No camera device found!");
                    return;
                }

                // Create MediaCapture and its settings
                _mediaCapture = new MediaCapture();

                _mediaCapture.Failed += MediaCapture_Failed;

                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

                // Initialize MediaCapture
                try
                {
                    await _mediaCapture.InitializeAsync(settings);
                    _isInitialized = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("The app was denied access to the camera");
                }

                // If initialization succeeded, start the preview
                if (_isInitialized)
                {
                    // Figure out where the camera is located
                    if (cameraDevice.EnclosureLocation == null || cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Unknown)
                    {
                        // No information on the location of the camera, assume it's an external camera, not integrated on the device
                        _externalCamera = true;
                    }
                    else
                    {
                        // Camera is fixed on the device
                        _externalCamera = false;

                        // Only mirror the preview if the camera is on the front panel
                        _mirroringPreview = (cameraDevice.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front);
                    }

                    // Initialize rotationHelper
                    _rotationHelper = new CameraRotationHelper(cameraDevice.EnclosureLocation);
                    _rotationHelper.OrientationChanged += RotationHelper_OrientationChanged;

                    await StartPreviewAsync();
                }
            }
        }

        /// <summary>
        /// Handles an orientation changed event
        /// </summary>
        private async void RotationHelper_OrientationChanged(object sender, bool updatePreview)
        {
            if (updatePreview)
            {
                await SetPreviewRotationAsync();
            }
        }

        /// <summary>
        /// Starts the preview and adjusts it for for rotation and mirroring after making a request to keep the screen on
        /// </summary>
        /// <returns></returns>
        private async Task StartPreviewAsync()
        {
            // Prevent the device from sleeping while the preview is running
            _displayRequest.RequestActive();

            // Set the preview source in the UI and mirror it if necessary
            PreviewControl.Source = _mediaCapture;
            PreviewControl.FlowDirection = _mirroringPreview ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            // Start the preview
            await _mediaCapture.StartPreviewAsync();
            _isPreviewing = true;

            // Initialize the preview to the current orientation
            if (_isPreviewing)
            {
                await SetPreviewRotationAsync();
            }
        }

        /// <summary>
        /// Gets the current orientation of the UI in relation to the device (when AutoRotationPreferences cannot be honored) and applies a corrective rotation to the preview
        /// </summary>
        private async Task SetPreviewRotationAsync()
        {
            // Only need to update the orientation if the camera is mounted on the device
            if (_externalCamera) return;

            // Add rotation metadata to the preview stream to make sure the aspect ratio / dimensions match when rendering and getting preview frames
            var rotation = _rotationHelper.GetCameraPreviewOrientation();
            var props = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(RotationKey, CameraRotationHelper.ConvertSimpleOrientationToClockwiseDegrees(rotation));
            await _mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
        }

        /// <summary>
        /// Stops the preview and deactivates a display request, to allow the screen to go into power saving modes
        /// </summary>
        /// <returns></returns>
        private async Task StopPreviewAsync()
        {
            // Stop the preview
            _isPreviewing = false;
            await _mediaCapture.StopPreviewAsync();

            // Use the dispatcher because this method is sometimes called from non-UI threads
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                // Cleanup the UI
                PreviewControl.Source = null;

                // Allow the device screen to sleep now that the preview is stopped
                if (_displayRequest != null)
                {
                    _displayRequest.RequestRelease();
                }
            });
        }

        /// <summary>
        /// Takes a photo to a StorageFile and adds rotation metadata to it
        /// </summary>
        /// <returns></returns>
        private async Task TakePhotoAsync()
        {

            try
            {
                var stream = new InMemoryRandomAccessStream();
                Debug.WriteLine("Taking photo...");
                await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

                var photoOrientation = CameraRotationHelper.ConvertSimpleOrientationToPhotoOrientation(_rotationHelper.GetCameraCaptureOrientation());

                var decoder = await BitmapDecoder.CreateAsync(stream);
                var encoder = await BitmapEncoder.CreateForInPlacePropertyEncodingAsync(decoder);
                var properties = new BitmapPropertySet { { "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16) } };
                await encoder.BitmapProperties.SetPropertiesAsync(properties);
                await encoder.FlushAsync();
                Debug.WriteLine("Photo saved in stream with orientation set!");

                using (var dr = new DataReader(stream.GetInputStreamAt(0)))
                {
                    var bytes = new byte[stream.Size];
                    await dr.LoadAsync((uint)stream.Size);
                    dr.ReadBytes(bytes);
                    SendEvent(deviceClient, Convert.ToBase64String(bytes));
                }
                stream.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception when taking a photo: " + ex.ToString());
            }
        }

        /// <summary>
        /// Cleans up the camera resources (after stopping any video recording and/or preview if necessary) and unregisters from MediaCapture events
        /// </summary>
        /// <returns></returns>
        private async Task CleanupCameraAsync()
        {
            Debug.WriteLine("CleanupCameraAsync");

            if (_isInitialized)
            {
                if (_isPreviewing)
                {
                    // The call to stop the preview is included here for completeness, but can be
                    // safely removed if a call to MediaCapture.Dispose() is being made later,
                    // as the preview will be automatically stopped at that point
                    await StopPreviewAsync();
                }

                _isInitialized = false;
            }

            if (_mediaCapture != null)
            {
                _mediaCapture.Failed -= MediaCapture_Failed;
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }

            if (_rotationHelper != null)
            {
                _rotationHelper.OrientationChanged -= RotationHelper_OrientationChanged;
                _rotationHelper = null;
            }
        }

        #endregion MediaCapture methods


        #region Helper functions

        /// <summary>
        /// Initialize or clean up the camera and our UI,
        /// depending on the page state.
        /// </summary>
        /// <returns></returns>
        private async Task SetUpBasedOnStateAsync()
        {
            // Avoid reentrancy: Wait until nobody else is in this function.
            while (!_setupTask.IsCompleted)
            {
                await _setupTask;
            }

            // We want our UI to be active if
            // * We are the current active page.
            // * The window is visible.
            // * The app is not suspending.
            bool wantUIActive = _isActivePage && Window.Current.Visible && !_isSuspending;

            if (_isUIActive != wantUIActive)
            {
                _isUIActive = wantUIActive;

                Func<Task> setupAsync = async () =>
                {
                    if (wantUIActive)
                    {
                        await SetupUiAsync();
                        await InitializeCameraAsync();
                    }
                    else
                    {
                        await CleanupCameraAsync();
                        await CleanupUiAsync();
                    }
                };
                _setupTask = setupAsync();
            }

            await _setupTask;
        }

        /// <summary>
        /// Attempts to lock the page orientation, hide the StatusBar (on Phone) and registers event handlers for hardware buttons and orientation sensors
        /// </summary>
        /// <returns></returns>
        private async Task SetupUiAsync()
        {
            // Attempt to lock page to landscape orientation to prevent the CaptureElement from rotating, as this gives a better experience
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;

            // Hide the status bar
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            }
        }

        /// <summary>
        /// Unregisters event handlers for hardware buttons and orientation sensors, allows the StatusBar (on Phone) to show, and removes the page orientation lock
        /// </summary>
        /// <returns></returns>
        private async Task CleanupUiAsync()
        {

            // Show the status bar
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().ShowAsync();
            }

            // Revert orientation preferences
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
        }

        /// <summary>
        /// Attempts to find and return a device mounted on the panel specified, and on failure to find one it will return the first device listed
        /// </summary>
        /// <param name="desiredPanel">The desired panel on which the returned device should be mounted, if available</param>
        /// <returns></returns>
        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // Get available devices for capturing pictures
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // Get the desired camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // If there is no device mounted on the desired panel, return the first device found
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        #endregion Helper functions

    }
}