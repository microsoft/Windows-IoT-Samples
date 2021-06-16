<!--
   samplefwlink:  https://aka.ms/WinIoTSamples
--->

    ██╗       ██╗ ██╗ ███╗  ██╗ ██████╗   █████╗  ██╗       ██╗  ██████╗      ██╗        ████████╗
    ██║  ██╗  ██║ ██║ ████╗ ██║ ██╔══██╗ ██╔══██╗ ██║  ██╗  ██║ ██╔════╝      ██║        ╚══██╔══╝
    ╚██╗████╗██╔╝ ██║ ██╔██╗██║ ██║  ██║ ██║  ██║ ╚██╗████╗██╔╝ ╚█████╗       ██║  █████╗   ██║
     ████╔═████║  ██║ ██║╚████║ ██║  ██║ ██║  ██║  ████╔═████║   ╚═══██╗      ██║ ██╔══██║  ██║
     ╚██╔╝ ╚██╔╝  ██║ ██║ ╚███║ ██████╔╝ ╚█████╔╝  ╚██╔╝ ╚██╔╝  ██████╔╝      ██║ ╚█████╔╝  ██║  
      ╚═╝   ╚═╝   ╚═╝ ╚═╝  ╚══╝ ╚═════╝   ╚════╝    ╚═╝   ╚═╝   ╚═════╝       ╚═╝  ╚════╝   ╚═╝

# Windows 10 Internet of Things (IoT) Samples

This repo contains samples that demonstrate usage patterns for Microsoft's Windows 10 IoT.  These code samples were created with templates available in Visual Studio and are designed, but not limited to, run on devices using Windows IoT Enterprise.

> **Note:** If you are unfamiliar with Git and GitHub, you can download the entire collection as a
> [ZIP file](https://github.com/Microsoft/Windows-universal-samples/archive/master.zip), but be
> sure to unzip everything to access shared dependencies. For more info on working with the ZIP file,
> the samples collection, and GitHub, see [Get the UWP samples from GitHub](https://aka.ms/ovu2uq).
> For more samples, see the [Samples portal](https://aka.ms/winsamples) on the Windows Dev Center.

## Windows 10 IoT development
These samples require Visual Studio and the Windows Software Development Kit (SDK) for Windows 10.

   [Get a free copy of Visual Studio Community Edition with support for building Universal Windows Platform apps](http://go.microsoft.com/fwlink/p/?LinkID=280676)

Additionally, to stay on top of the latest updates to Windows and the development tools, become a Windows Insider by joining the Windows Insider Program.

   [Become a Windows Insider](https://insider.windows.com/)

   ## Using the samples

The easiest way to use these samples without using Git is to download the zip file containing the current version (using the following link or by clicking the "Download ZIP" button on the repo page). You can then unzip the entire archive and use the samples in Visual Studio.

   [Download the samples ZIP](../../archive/master.zip)

   **Notes:**
   * Before you unzip the archive, right-click it, select **Properties**, and then select **Unblock**.
   * Be sure to unzip the entire archive, and not just individual samples. The samples all depend on the SharedContent folder in the archive.   
   * In Visual Studio, the platform target defaults to ARM, so be sure to change that to x64 or x86 if you want to test on a non-ARM device.

The samples use Linked files in Visual Studio to reduce duplication of common files, including sample template files and image assets. These common files are stored in the SharedContent folder at the root of the repository and are referred to in the project files using links.

**Reminder:** If you unzip individual samples, they will not build due to references to other portions of the ZIP file that were not unzipped. You must unzip the entire archive if you intend to build the samples.

For more info about the programming models, platforms, languages, and APIs demonstrated in these samples, please refer to the guidance, tutorials, and reference topics provided in the Windows 10 documentation available in the [Windows Developer Center](http://go.microsoft.com/fwlink/p/?LinkID=532421). These samples are provided as-is in order to indicate or demonstrate the functionality of the programming models and feature APIs for Windows.

## Contributing
These samples are direct from the feature teams and we welcome your input on issues and suggestions for new samples. If you would like to see new coverage or have feedback, please consider contributing. You can edit the existing content, add new content, or simply create new issues. We’ll take a look at your suggestions and will work together to incorporate them into the docs.

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

**Note**:
* When contributing, make sure you are contributing from the **develop** branch and not the master branch. Your contribution will not be accepted if your PR is coming from the master branch.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact opencode@microsoft.com with any additional questions or comments.


## See also

For additional Windows samples, see:
* [Windows on GitHub](http://microsoft.github.io/windows/)
* [Windows IoT Core](https://github.com/microsoft/Windows-iotcore-samples/tree/develop/Samples)

## Samples by category

### Azure IoT Edge

| Name           | Description      |  
|----------------|------------------|  
| [interop-textmsg-consoleapp](https://aka.ms/WinIoTSamples-interop-textmsg-consoleapp) | Basic interop sample demonstrating text messaging between a Windows console app and an Edge module. |
| [interop-customvision-textmsg-uwpapp](https://aka.ms/WinIoTSamples-interop-customvision-textmsg-uwpapp) | <p>Two more advanced interop samples which demonstrate bidirectional communication between a Windows application and an Edge module. </p><ul><li>Text messaging between a UWP application and an Edge module. </li><li>A 'Custom vision' machine learning interop sample with a fruit classifier which uses a Windows UWP app to send camera frames to an Edge module for identification.</li></ul>|  
