﻿using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using DualOperator.Enumerations;
using DualOperator.Structures;

namespace DualOperator.Helpers
{
	public sealed class RawKeyboard
	{
		private readonly Dictionary<IntPtr, KeyPressEvent> _deviceList = new Dictionary<IntPtr, KeyPressEvent>();
		public delegate void DeviceEventHandler(object sender, RawInputEventArg e);
		public event DeviceEventHandler KeyPressed;
		readonly object _padLock = new object();
		public int NumberOfKeyboards { get; private set; }
		static InputData _rawBuffer;

		public RawKeyboard(IntPtr hwnd, bool captureOnlyInForeground)
		{
			var rid = new RawInputDevice[1];

			rid[0].UsagePage = HIDUsagePage.GENERIC;
			rid[0].Usage = HIDUsage.Keyboard;
			rid[0].Flags = (captureOnlyInForeground ? RawInputDeviceFlags.NONE : RawInputDeviceFlags.INPUTSINK) | RawInputDeviceFlags.DEVNOTIFY;
			rid[0].Target = hwnd;

			if (!Win32.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0])))
			{
				throw new ApplicationException("Failed to register raw input device(s).");
			}
		}

		public void EnumerateDevices()
		{
			lock (_padLock)
			{
				_deviceList.Clear();

				var keyboardNumber = 0;

				var globalDevice = new KeyPressEvent
				{
					DeviceName = "Global Keyboard",
					DeviceHandle = IntPtr.Zero,
					DeviceType = Win32.GetDeviceType(DeviceType.RimTypekeyboard),
					Name = "Fake Keyboard. Some keys (ZOOM, MUTE, VOLUMEUP, VOLUMEDOWN) are sent to rawinput with a handle of zero.",
					Source = keyboardNumber++.ToString(CultureInfo.InvariantCulture)
				};

				_deviceList.Add(globalDevice.DeviceHandle, globalDevice);

				var numberOfDevices = 0;
				uint deviceCount = 0;
				var dwSize = (Marshal.SizeOf(typeof(RawInputDeviceList)));

				if (Win32.GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, (uint)dwSize) == 0)
				{
					var pRawInputDeviceList = Marshal.AllocHGlobal((int)(dwSize * deviceCount));
					Win32.GetRawInputDeviceList(pRawInputDeviceList, ref deviceCount, (uint)dwSize);

					for (var i = 0; i < deviceCount; i++)
					{
						uint pcbSize = 0;

						// On Window 8 64bit when compiling against .Net > 3.5 using .ToInt32 you will generate an arithmetic overflow. Leave as it is for 32bit/64bit applications
						var rid = (RawInputDeviceList)Marshal.PtrToStructure(new IntPtr((pRawInputDeviceList.ToInt64() + (dwSize * i))), typeof(RawInputDeviceList));

						Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, IntPtr.Zero, ref pcbSize);

						if (pcbSize <= 0) continue;

						var pData = Marshal.AllocHGlobal((int)pcbSize);
						Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, pData, ref pcbSize);
						var deviceName = Marshal.PtrToStringAnsi(pData);

						if (rid.dwType == DeviceType.RimTypekeyboard || rid.dwType == DeviceType.RimTypeHid)
						{
							var deviceDesc = Win32.GetDeviceDescription(deviceName);

							var dInfo = new KeyPressEvent
							{
								DeviceName = Marshal.PtrToStringAnsi(pData),
								DeviceHandle = rid.hDevice,
								DeviceType = Win32.GetDeviceType(rid.dwType),
								Name = deviceDesc,
								Source = keyboardNumber++.ToString(CultureInfo.InvariantCulture)
							};

							if (!_deviceList.ContainsKey(rid.hDevice))
							{
								numberOfDevices++;
								_deviceList.Add(rid.hDevice, dInfo);
							}
						}

						Marshal.FreeHGlobal(pData);
					}

					Marshal.FreeHGlobal(pRawInputDeviceList);

					NumberOfKeyboards = numberOfDevices;
					Debug.WriteLine("EnumerateDevices() found {0} Keyboard(s)", NumberOfKeyboards);
					return;
				}
			}

			throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		public void ProcessRawInput(IntPtr hdevice)
		{
			lock (_padLock)
            {
                if (_deviceList.Count == 0) return;
            }

			var dwSize = 0;
			Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, IntPtr.Zero, ref dwSize, Marshal.SizeOf(typeof(RawInputHeader)));

			if (dwSize != Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, out _rawBuffer, ref dwSize, Marshal.SizeOf(typeof(RawInputHeader))))
			{
				Debug.WriteLine("Error getting the Raw Input buffer");
				return;
			}

			int virtualKey = _rawBuffer.data.keyboard.VKey;
			int makeCode = _rawBuffer.data.keyboard.Makecode;
			int flags = _rawBuffer.data.keyboard.Flags;

			if (virtualKey == Win32.KEYBOARD_OVERRUN_MAKE_CODE) return;

			var isE0BitSet = ((flags & Win32.RI_KEY_E0) != 0);

			KeyPressEvent keyPressEvent;

			lock (_padLock)
            {
                if (_deviceList.ContainsKey(_rawBuffer.header.hDevice))
                {
                    lock (_padLock)
                    {
                        keyPressEvent = _deviceList[_rawBuffer.header.hDevice];
                    }
                }
                else
                {
                    Debug.WriteLine("Handle: {0} was not in the device list.", _rawBuffer.header.hDevice);
                    return;
                }
            }

            var operatorApp = LoadOperator.OperatorApps.FirstOrDefault(x => x.Keyboard == keyPressEvent.DeviceName);
            if (operatorApp != null)
            {
                keyPressEvent.TargetApp = operatorApp;
            }

			var isBreakBitSet = ((flags & Win32.RI_KEY_BREAK) != 0);

			keyPressEvent.KeyPressState = isBreakBitSet ? "BREAK" : "MAKE";
			keyPressEvent.Message = _rawBuffer.data.keyboard.Message;
			keyPressEvent.VKeyName = KeyMapper.GetKeyName(VirtualKeyCorrection(virtualKey, isE0BitSet, makeCode)).ToUpper();
			keyPressEvent.VKey = virtualKey;

            KeyPressed(this, new RawInputEventArg(keyPressEvent));
        }

		private static int VirtualKeyCorrection(int virtualKey, bool isE0BitSet, int makeCode)
		{
			var correctedVKey = virtualKey;

			if (_rawBuffer.header.hDevice == IntPtr.Zero)
			{
				// When hDevice is 0 and the vkey is VK_CONTROL indicates the ZOOM key
				if (_rawBuffer.data.keyboard.VKey == Win32.VK_CONTROL)
				{
					correctedVKey = Win32.VK_ZOOM;
				}
			}
			else
            {
                correctedVKey = virtualKey switch
                {
                    // Right-hand CTRL and ALT have their e0 bit set 
                    Win32.VK_CONTROL => isE0BitSet ? Win32.VK_RCONTROL : Win32.VK_LCONTROL,
                    Win32.VK_MENU => isE0BitSet ? Win32.VK_RMENU : Win32.VK_LMENU,
                    Win32.VK_SHIFT => makeCode == Win32.SC_SHIFT_R ? Win32.VK_RSHIFT : Win32.VK_LSHIFT,
                    _ => virtualKey
                };
            }

			return correctedVKey;
		}
	}
}
