# Dual Operator

Dual Operator is a conceptual sample application which demonstrates how input from 2 physical keyboards can be routed to specific applications docked full screen to separate display. Dual Operator receives keystrokes from both physical keyboards and routes them to the application to which the keyboard is associated.   

Teaser Trailer: http://aka.ms/dualoperatordemo
MS Build 2022: https://aka.ms/MSBuild2022-DualOperator

## Requirements

- 2 physical keyboards
- 2 monitors

Dual Operator requires the full device ID as used by Windows for each keyboard registered.  It also requires a unique portion of the title bar text to be paired with the keyboard output.

Applications targeted by Dual Operator must have a visible window on any available monitor.  This means that Windows services or any application that runs as a background task is not a target candidate because this type of application does not present a handle to the operating system.  This handle is a necessity for receiving keystrokes whether they come from the OS or from Dual Operator.

In addition to having a visible window, each application targeted should also be running in full screen mode on its own monitor running on Windows IoT Enterprise edition.

## Devices

Dual Operator demonstrates input isolation using 2 physical keyboards. Dual Operator does not provide input isolation for mouse or touch imput.

## How it works
In normal operation, on startup Dual Operator will read from a file named OPERATORS.JSON located in the same folder as the executable file for the system.  The contents of the file look like this:

```
[
  {
    "ApplicationTitle": "Document1 - Word",
    "Keyboard": "\\\\?\\HID#VID_046D&PID_C541&MI_01&Col01#b&865bce8&0&0000#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}"
  },
  {
    "ApplicationTitle": "Untitled - Notepad",
    "Keyboard": "\\\\?\\HID#{00001812-0000-1000-8000-00805f9b34fb}_Dev_VID&02045e_PID&0832_REV&0138_eb5864041ec3&Col01#9&19514786&0&0000#{884b96c3-56ef-11d1-bc8c-00a0c91405dd}"
  }
]
```

The file is an array of application and keyboard objects paired together.  The ApplicationTitle element in each object is the unique window text for the application.  This value can be as much or little as desired providing the value uniquely identifies a specific window.

The second element is the full device identifier.  This is what Dual Operator uses to differentiate between keyboards and application targets.  If input is received from any other keyboard attached to the system, Dual Operator will ignore the input and allow Windows to direct the keystroke to another application.

Please note: the device identifier does not always follow the same naming convention across devices and in many cases, even coming from the same device manufacturer.  In the example above, the first device is a Logitech G9 keyboard while the second device is a Logitech Bluetooth keyboard.  Dual Operator provides a method of learning the various keyboards attached to the system - see the section on Command Line Parameters below.

There can be only 2 array items defined.  Dual Operator will ignore any items beyond the first two found.

Dual Operator uses the Raw Input functions contained within the USER32.DLL module contained in Windows 8 and above.  Through the use of a custom key mapper, Dual Operator can handle almost all recognizable keys along with their commonly used modifiers like ALT, CONTROL and SHIFT.

Because Windows requires a visible window to receive keystrokes, Dual Operator has been written as a Windows Forms application in C# version 6 and .NET Core 5.  Even though the application targets .NET Core, Dual Operator runs solely on the Windows platform because of its use of the native keyboard handling functions built into Windows.

The window the application provides contains a text control that will show the device that produced a keystroke along with the scan code, the key state and other pertinent information about the keypress event.  Although KEY DOWN key state events are shown in the text control, Dual Operator does not act on these as it only signifies that a key action has started.  If the application responded to the KEY DOWN event, then multiple combinations of keys could be achieved (e.g., CTL-SHIFT-ESC).  Only when all key combinations have generated their KEY UP events (signifying that the user is no longer pressing any key) does Dual Operator find the appropriate match for the event and pass the combined keystroke to the target application.

## Command Line Parameters

Dual Operator accepts two different command line parameters and an optional companion parameter for the first (the parameter is not case-sensitive):

/LIST
This parameter will generate a list of Human Interface Device items connected to the system.  If the optional parameter is not used, the /LIST parameter will generate a file named DeviceAudit.txt in the same directory as the Dual Operator application.  This list can be opened in any standard text editor and will help guide the selection of the proper keyboard identifiers to use in the OPERATORS.JSON file.

{optional} Full file path and name
This optional parameter will specify where to place the output from /LIST parameter.

/SCAN
This parameter will open a form which will allow the user to press any key on an attached keyboard to learn the full name for the device.  Once a key has been pressed, the name can be copied to the clipboard or saved directly in the OPERATORS.JSON file as either keyboard 1 or 2.

### Examples

``
dualoperator.exe /list
``
This will create a file in the current directory named DeviceAudit.txt

``
dualoperator.exe /list "C:\Temp\DeviceList.txt"
``
This will create the same output in a file named DeviceList.txt in the C:\Temp directory

``
dualoperator.exe /scan
``
This will open the Scan Operator form and allow identification of any attached keyboard


## Demo app

There is a demo target application provided with the Dual Operator code.  This application looks very similar to the Dual Operator app but is intended to be a target for Dual Operator to use while testing.

In the demo app directory there is a file named APPTITLE.JSON.  This provides a unique title for the demo app making it easy to run 2 instances with different window names.  The structure of the file looks like this:

```
[
  {
    "ApplicationTitle": "Demo App 1"
  }
]
```

The ApplicationTitle element defines the text for the app window.  It is possible to put a copy of this demo app in separate directories and by changing the ApplicationTitle to be unique values in each directory, two versions of this app can run as targets for Dual Operator.

## Code structure

### Root
OperatorManager: the form used by Dual Operator to intercept keystrokes and all other associated top-level code and configuration objects

### Enumerations
Enumerations used throughout the application, such as KeyEvent

### Helpers
Code that helps drive function within the application (e.g. the custom Key Mapper component)

### Models
Classes used to serialize/deserialze data between systems (e.g., reading configuration from the OPERATORS.JSON file)

### Structures
C# STRUCT objects used to pass and receive information from the Raw Input functions (e.g., DeviceInfo which has metadata about the device which generated an event)


