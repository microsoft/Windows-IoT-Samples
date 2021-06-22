---
page_type: sample
urlFragment: memory-status
languages:
  - cpp
products:
  - Windows 10
  - Windows IoT
  - Windows 10 IoT Enterprise
description: Create a console app that monitors memory usage on devices running Windows 10 IoT Enterprise.
---

# Memory Status Monitor

This sample is intended to show how to create a console application that can be used to query the memory usage on your Windows 10 IoT Enterprise device.


## Step 1: Load the project in Visual Studio 2019
1. Open the application in Visual Studio 2019
2. Set the architecture in the toolbar dropdown. If you’re building for the UP Board, select x64.
3. Select **Local Machine** to point to IoT device and hit F5 to deploy to your device.


## Step 2: Deploy your app  
1. [Generate an app package](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#generate-an-app-package)

1. [Install your app package using an install script](https://docs.microsoft.com/windows/msix/package/packaging-uwp-apps#install-your-app-package-using-an-install-script)

1. If you are using an UP Board, you have to setup the BIOS UART configuration.

    1. Once you power on the UP board, select the Del or F7 key on your keyboard to enter the BIOS setting.

    1. Navigate to Boot > OS Image ID tab, and select Windows 10 IoT Core.

    1. Navigate to the Advance tab and select the Hat Configuration and select GPIO Configuration in Pin Order.

    1. Configure the Pins you are using in the sample as INPUT or OUTPUT.

    1. For more information, please review the [UP Board Firmware Settings](https://www.annabooks.com/Articles/Articles_IoT10/Windows-10-IoT-UP-Board-BIOS-RHPROXY-Rev1.3.pdf).

1. Click the Start button to search for the app by name, and then launch it.

* To add some functionality to our console, add the following memory status query and display code:

```C++
#include "pch.h"

#include <windows.h>
#include <chrono>
#include <thread>

using namespace std;

// Use to convert bytes to KB
#define DIV 1024

// Specify the width of the field in which to print the numbers.
#define MESSAGE_WIDTH 30
#define NUMERIC_WIDTH 10

void printMessage(LPCSTR msg, bool addColon)
{
    cout.width(MESSAGE_WIDTH);
    cout << msg ;
    if (addColon)
    {
        cout << " : ";
    }
}

void printMessageLine(LPCSTR msg)
{
    printMessage(msg, false);
    cout << endl;
}

void printMessageLine(LPCSTR msg, DWORD value)
{
    printMessage(msg, true);
    cout.width(NUMERIC_WIDTH);
    cout << right << value << endl;
}

void printMessageLine(LPCSTR msg, DWORDLONG value)
{
    printMessage(msg, true);
    cout.width(NUMERIC_WIDTH);
    cout << right << value << endl;
}

void checkInput(HANDLE exitEvent)
{
    for (;;)
    {
        char character;
        cin.get(character);
        if (character == 'q')
        {
            ::SetEvent(exitEvent);
            break;
        }
    }
}

int main(int argc, char **argv)
{
    printMessageLine("Starting to monitor memory consumption! Press enter to start monitoring");
    printMessageLine("You can press q and enter at anytime to exit");
    cin.get();
    HANDLE exitEvent = ::CreateEvent(NULL, FALSE, FALSE, NULL);
    if (NULL == exitEvent)
    {
        printMessageLine("Failed to create exitEvent.");
        return -1;
    }
    std::thread inputThread(checkInput, exitEvent);
    for (;;)
    {
        MEMORYSTATUSEX statex;
        statex.dwLength = sizeof(statex);

        BOOL success = ::GlobalMemoryStatusEx(&statex);
        if (!success)
        {
            DWORD error = GetLastError();
            printMessageLine("*************************************************");
            printMessageLine("Error getting memory information", error);
            printMessageLine("*************************************************");
        }
        else
        {
            DWORD load = statex.dwMemoryLoad;
            DWORDLONG physKb = statex.ullTotalPhys / DIV;
            DWORDLONG freePhysKb = statex.ullAvailPhys / DIV;
            DWORDLONG pageKb = statex.ullTotalPageFile / DIV;
            DWORDLONG freePageKb = statex.ullAvailPageFile / DIV;
            DWORDLONG virtualKb = statex.ullTotalVirtual / DIV;
            DWORDLONG freeVirtualKb = statex.ullAvailVirtual / DIV;
            DWORDLONG freeExtKb = statex.ullAvailExtendedVirtual / DIV;

            printMessageLine("*************************************************");

            printMessageLine("Percent of memory in use", load);
            printMessageLine("KB of physical memory", physKb);
            printMessageLine("KB of free physical memory", freePhysKb);
            printMessageLine("KB of paging file", pageKb);
            printMessageLine("KB of free paging file", freePageKb);
            printMessageLine("KB of virtual memory", virtualKb);
            printMessageLine("KB of free virtual memory", freeVirtualKb);
            printMessageLine("KB of free extended memory", freeExtKb);

            printMessageLine("*************************************************");

        }

        if (WAIT_OBJECT_0 == ::WaitForSingleObject(exitEvent, 100))
        {
            break;
        }
    }

    inputThread.join();
    ::CloseHandle(exitEvent);
    printMessageLine("No longer monitoring memory consumption!");
}
```

* Make sure the app builds correctly invoking the Build \| Build Solution menu command.
