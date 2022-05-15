Param
(
  [Parameter(ParameterSetName="listGroup", Mandatory)]
  [Switch]$List,
  
  [Parameter(ParameterSetName="offlineRemoveUnitGroup", Mandatory)]
  [Parameter(ParameterSetName="offlineListGroup", Mandatory)]
  [ValidateScript({
     if( -Not ($_ | Test-Path) ){ throw "Folder does not exist!" }
     return $true
  })]
  [System.IO.DirectoryInfo]$offlineImageFolderPath,

  [Parameter(ParameterSetName="onlineRemoveUnitGroup", Mandatory)]
  [Parameter(ParameterSetName="onlineListGroup", Mandatory)]
  [Switch]$isOnline,
  
  [Switch]$suppressPrompts,
  
  [Parameter(ParameterSetName="offlineRemoveUnitGroup", Mandatory)]
  [Parameter(ParameterSetName="onlineRemoveUnitGroup", Mandatory)]
  [String]$RemoveUnit
)

$unitsList = @(
[PSCustomObject]@{
  Name = "ProjFS_OptionalFeature"
  Packages = @(
  "Microsoft-Windows-ProjFS-OptionalFeature"
  )
  Description = "Windows Projected File System optional feature."
},
[PSCustomObject]@{
  Name = "Biometrics"
  Packages = @(
  "Microsoft-Windows-BioEnrollment-UX", 
  "Microsoft-OneCore-Biometrics-Fingerprint"
  )
  Description = 
@"
Windows Hello Setup UI.  
Biometric credential provider and sensor adaptor.
"@
},
[PSCustomObject]@{
  Name = "WalletService"
  Packages = @(
  "Microsoft-OneCore-WalletService"
  )
  Description = 
@"
Wallet feature.  
https://docs.microsoft.com/en-us/uwp/api/Windows.ApplicationModel.Wallet?view=winrt-19041#:~:text=Enums%20%20%20Wallet%20Action%20Kind%20%20,the%20position%20in%20the%20summary%20vi%20...%20
"@
},
[PSCustomObject]@{
  Name = "Printing"
  Packages = @(
  "Microsoft-Windows-Printer-Drivers",
  "Microsoft-Windows-Printing-Foundation",
  "Microsoft-Windows-Printing-PremiumTools",
  "Microsoft-Windows-Printing-XPSServices",
  "Microsoft-Windows-Printing-PrintToPDFServices",
  "Microsoft-Windows-Printing-3D"
  )
  Description = 
@"
Printer drivers.  
Printing Premium Tools Collection.  
XPS Document Writer & Print to PDF & other printing optional features.  
3D printing.  
Internet Printing Client | LPD Print Service | LPR Port Monitor.
"@
},
[PSCustomObject]@{
  Name = "MiscNetworking"
  Packages = @(
  "Microsoft-OneCore-Miracast-Transmitter",
  "Microsoft-Windows-DataCenterBridging",
  "Microsoft-Windows-RDC",
  "Microsoft-Windows-Telnet-Client",
  "Microsoft-Windows-SimpleTCP"
  )
  Description = 
@"
Miracast driver. 
Data Center Bridging optional feature. 
Remote Differential Compression API Support optional feature. 
Telnet Client optional feature. 
Simple TCPIP services optional feature. (i.e. echo, daytime etc).
"@
},
[PSCustomObject]@{
  Name = "Legacy_Components_OC"
  Packages = @(
  "Microsoft-Windows-Legacy-Components-OC"
  )
  Description = "NTVDM and DirectPlay legacy optional features.  NTVDM | DirectPlay | LegacyComponents"
},
[PSCustomObject]@{
  Name = "PEAuth_OneCore"
  Packages = @(
  "Microsoft-Windows-PEAuth-OneCore"
  )
  Description = 
@"
Protected Environment service for protecting DRM(Digital Rights Management) contents.     
https://docs.microsoft.com/en-us/windows/win32/medfound/protected-media-path#protected-environment
"@
},
[PSCustomObject]@{
  Name = "OneCore_SD"
  Packages = @(
  "Microsoft-OneCore-SD"
  )
  Description = "SD storage drivers."
},
[PSCustomObject]@{
  Name = "UsbConnectorManager"
  Packages = @(
  "Microsoft-OneCore-Connectivity-UsbConnectorManager",
  "Microsoft-OneCore-Connectivity-UsbFunction"
  )
  Description = 
@"
Required for systems with USB-C connectors.  
Required for systems that support USB device/function role.
"@
},
[PSCustomObject]@{
  Name = "WinOcr"
  Packages = @(
  "Microsoft-Windows-WinOcr"
  )
  Description = "Enables the indexing and searching of Tagged Image File Format (TIFF) files using Optical Character Recognition (OCR)."
},
[PSCustomObject]@{
  Name = "Identity_Foundation"
  Packages = @(
  "Microsoft-Windows-Identity-Foundation"
  )
  Description = "Windows Identity Foundation 3.5 optional feature."
},
[PSCustomObject]@{
  Name = "SensorDataService"
  Packages = @(
  "Microsoft-Windows-SensorDataService"
  )
  Description = "Legacy camera and image integration service to support Windows Hello for devices using old IR cameras."
},
[PSCustomObject]@{
  Name = "ScreenSavers"
  Packages = @(
  "Microsoft-Windows-Shell-Wallpaper-Common",
  "Microsoft-Windows-ScreenSavers-3D"
  )
  Description = "Wallpapers.  3D screen savers."
},
[PSCustomObject]@{
  Name = "LanguageFeatures_WordBreaking_Common_legacy"
  Packages = @(
  "LanguageFeatures-WordBreaking-Common-legacy"
  )
  Description = "Legacy neutral word breaker, should only be needed in very rare app compat scenarios."
},
[PSCustomObject]@{
  Name = "RemoteDesktopServices_Collaboration"
  Packages = @(
  "Microsoft-OneCore-RemoteDesktopServices-Collaboration"
  )
  Description = "Windows desktop sharing."
},
[PSCustomObject]@{
  Name = "DesktopTools"
  Packages = @(
  "Microsoft-Windows-win32calc",
  "Microsoft-Windows-ShellOptions",
  "Microsoft-Windows-SnippingTool"
  )
  Description = "Win32 Calculator. Calc.exe, Character Map. Snipping Tool."
},
[PSCustomObject]@{
  Name = "UserDeviceRegistration"
  Packages = @(
  "Microsoft-Windows-UserDeviceRegistration"
  )
  Description = 
@"
This package enables device registration with Azure AD, which creates a unique identity for the device in Azure AD. 
Device registration drives device-based authentication with Azure AD providing enhanced authorization and security controls, 
and single sign on for applications that rely on user and/or device identities. 
So, device registration is critical to maintaining a device identity to securely interact with overall Microsoft ecosystem.
"@
},
[PSCustomObject]@{
  Name = "SettingsAndAdminUX"
  Packages = @(
  "Microsoft-OneCore-SystemSettings-NetworkMobileHandlers",
  "Microsoft-Windows-RecoveryDrive",
  "Microsoft-Windows-NetProfilesUX",
  "Microsoft-Windows-MobilePC-Client-Basic",
  "Microsoft-OneCore-SystemSettings-Devices",
  "Microsoft-Windows-ComputerManagerLauncher",
  "Microsoft-Windows-Defrag-UI",
  "Microsoft-OneCore-SystemSettings-UserAccount"
  )
  Description = 
@"
Settings handler - Network and Mobile, Devices, User Accounts. Recovery Media Creator.  
Network and Sharing Center control panel. 
Windows Mobility Center. 
Computer Management system tool. 
Disk Defragmenter Admin UI. 
Windows Search optional feature.
"@
},
[PSCustomObject]@{
  Name = "FileSharingSyncing"
  Packages = @(
  "Microsoft-Windows-EnterpriseClientSync-Host",
  "Microsoft-Windows-NFS-ClientSKU",
  "Microsoft-Windows-SMB1",
  "Microsoft-Windows-SMB1Deprecation",
  "Microsoft-Windows-SmbDirect"
  )
  Description = 
@"
Work Folder client optional feature. 
NFS optional feature.  
Client for NFS | Administrative Tools. SMB1Protocol and SMB Direct optional features.  
SMB 1.0/CIFS Client | SMB 1.0/CIFS Server.  
Remote Direct Memory Access (RDMA) support for the SMB 3.x file sharing protocol.  
Automatically removes support for the legacy SMB 1.0/CIFS protocol when such support isn't actively needed during normal system usage. 
"@
},
[PSCustomObject]@{
  Name = "DestopShell"
  Packages = @(
  "Microsoft-Windows-DesktopFileExplorer",
  "Microsoft-OneCore-WWA",
  "Microsoft-Windows-SearchEngine-Client"
  )
  Description = "File Explorer system app. Windows Web App Host.  Windows Search optional feature."
},
[PSCustomObject]@{
  Name = "Audio"
  Packages = @(
  "Microsoft-Windows-3DAudio-HrtfData",
  "Microsoft-OneCore-Multimedia-Acx"
  )
  Description = 
@"
Data files for Windows Sonic spatial audio format.  
ACX Audio Class Extension for WDF, based audio drivers.
"@
},
[PSCustomObject]@{
  Name = "Virtualization"
  Packages = @(
  "Microsoft-Windows-HyperV-OptionalFeature-HypervisorPlatform-Disabled",
  "Microsoft-Windows-HyperV-OptionalFeature-VirtualMachinePlatform-Disabled",
  "Microsoft-Windows-AppManagement-UEV"
  )
  Description = 
@"
Hypervisor Platform and Virtual Machine Platform optional features.  
User Experience Virtualization (UE-V)
"@
},
[PSCustomObject]@{
  Name = "MultiPoint_Connector"
  Packages = @(
  "Microsoft-Windows-MultiPoint-Connector"
  )
  Description = 
@"
MultiPoint Connector optional features.  
MultiPoint Connector Services | MultiPoint Manager and MultiPoint Dashboard | MultiPoint help files
"@
},
[PSCustomObject]@{
  Name = "OneCore_Containers_Opt"
  Packages = @(
  "Microsoft-Windows-OneCore-Containers-Opt"
  )
  Description = "Containers optional feature."
},
[PSCustomObject]@{
  Name = "DirectoryServices_ADAM_Client"
  Packages = @(
  "Microsoft-Windows-DirectoryServices-ADAM-Client"
  )
  Description = 
@"
Active Directory Lightweight Directory Services optional feature.  
Active Directory Lightweight Directory Services | Active Directory Lightweight Directory Services_admin
"@
},
[PSCustomObject]@{
  Name = "Lxss_WithGraphics_OptionalWrapper"
  Packages = @(
  "Microsoft-Windows-Lxss-WithGraphics-OptionalWrapper"
  )
  Description = "Windows Subsystem for Linux optional feature."
},
[PSCustomObject]@{
  Name = "Containers_OptionalFeature_DisposableClientVM"
  Packages = @(
  "Containers-OptionalFeature-DisposableClientVM"
  )
  Description = "Windows Sandbox optional feature."
},
[PSCustomObject]@{
  Name = "Defender_AM_Default_Definitions_OptionalWrapper"
  Packages = @(
  "Windows-Defender-AM-Default-Definitions-OptionalWrapper"
  )
  Description = "Windows Defender engine and security signature files optional feature."
},
[PSCustomObject]@{
  Name = "Fonts_DesktopFonts_NonLeanSupplement"
  Packages = @(
  "Microsoft-OneCore-Fonts-DesktopFonts-NonLeanSupplement"
  )
  Description = "Additional Chinese, Japanese and Korean fonts."
},
[PSCustomObject]@{
  Name = "HVSI"
  Packages = @(
  "Microsoft-Windows-HVSI"
  )
  Description = 
@"
Defines the optional component for Windows Defender Application Guard.  
Microsoft Defender Application Guard | Container Server | 
Inherited Activation Virtual Device | Remote App Lifetime Manager | 
Microsoft NT Kernel Integration Virtual Device | Microsoft NT Kernel Integration VSC Driver
"@
},
[PSCustomObject]@{
  Name = "Hyper_V_ClientEdition"
  Packages = @(
  "Microsoft-Hyper-V-ClientEdition",
  "Microsoft-Windows-PAW-Feature"
  )
  Description = 
@"
Hyper-V and Guarded Host optional features.  
Hyper-V Module for Windows PowerShell | Hyper-V Hypervisor | 
Hyper-V BASE | Hyper-V Services | Inherited Activation Virtual Device | 
RemoteFX Graphics Virtualization Host | Hyper-V Container Networking |
Hyper-V GUI Management Tools | Hyper-V GUI Management Tools
"@
},
[PSCustomObject]@{
  Name = "IIS"
  Packages = @(
  "Microsoft-Windows-IIS-WebServer",
  "Microsoft-Windows-IIS-WebServer-AddOn",
  "Microsoft-Windows-IIS-WebServer-AddOn-2",
  "Microsoft-Windows-MSMQ-Client"
  )
  Description = 
@"
IIS WebServer.  MSMQ and IIS optional features.  
Internet Information Services | World Wide Web Services | 
IIS-StartClient | Common HTTP Features | HTTP Errors | 
HTTP Redirection | Application Development Features | Security | Request Filtering | 
.NET Extensibility 3.5 | .NET Extensibility 4.8 | Health and Diagnostics | 
HTTP Logging | Logging Tools | Request Monitor | Tracing | URL Authorization | 
IP Security | Performance Features | Dynamic Content Compression | Web Management Tools | 
IIS Management Scripts and Tools | IIS 6 Management Compatibility | 
IIS Metabase and IIS 6 configuration compatibility | Windows Process Activation Service | 
Process Model | .NET Environment | Configuration APIs | 
Internet Information Services Hostable Web Core | Static Content | Default Document | 
Directory Browsing | WebDAV Publishing | WebSocket Protocol | Application Initialization | 
ASP.NET 3.5 | ASP.NET 4.8 | ASP | CGI | ISAPI Extensions | ISAPI Filters | 
Server-Side Includes | Custom Logging | Basic Authentication | Static Content Compression | 
IIS Management Console | IIS Management Service | IIS 6 WMI Compatibility | 
IIS 6 Scripting Tools | IIS 6 Management Console | FTP Server | FTP Service | 
FTP Extensibility  |  Centralized SSL Certificate Support | Windows Authentication | 
Digest Authentication | Client Certificate Mapping Authentication | 
IIS Client Certificate Mapping Authentication | ODBC LoggingStatic Content | 
Default Document | Directory Browsing | WebDAV Publishing | WebSocket Protocol | 
Application Initialization | ASP.NET 3.5 | ASP.NET 4.8 | ASP | CGI | ISAPI Extensions | 
ISAPI Filters | Server-Side Includes | Custom Logging | Basic Authentication | 
Static Content Compression | IIS Management Console | IIS Management Service | 
IIS 6 WMI Compatibility | IIS 6 Scripting Tools | IIS 6 Management Console | FTP Server | 
FTP Service | FTP Extensibility 
"@
},
[PSCustomObject]@{
  Name = "BootEnvironment_Dvd"
  Packages = @(
  "Microsoft-Windows-BootEnvironment-Dvd"
  )
  Description = "DVD boot configuration."
},
[PSCustomObject]@{
  Name = "DevDiagnosticsTools"
  Packages = @(
  "Microsoft-Windows-SDKTools"
  )
  Description = "SDK tools including regsvr32.exe, regini.exe and debugger engine."
}
)


if($List) {
  $unitsList | sort Name 
  return
}


##
## Check current edition is IoTEnterpriseS 
##
$currEditionOutput = $null
if($isOnline) {
  $onOfflineArg = "/Online"
} else {
  $onOfflineArg = "/Image:$offlineImageFolderPath"
}
$currEditionOutput = & $env:windir\system32\Dism.exe $onOfflineArg /Get-CurrentEdition

$currEditionValue = $null
$currEditionOutput | foreach {
   if($_.StartsWith('Current Edition :')) {
      $prefix,$currEditionValue = $_.Split(':')
      $currEditionValue = $currEditionValue.Trim()
    }
}
if(!($currEditionValue -like 'IoTEnterpriseS')) {
  throw "The current edition is $currEditionValue.  This scrip is only supported in IoTEnterpriseS."
}


Function Remove
{
  param(
  [parameter(Mandatory)]
  [String[]]$pkgsToRemove,
  [Switch]$suppressPrompts
  )

    ##
    ## package-parent table
    ##
    ## TODO: make it future composition change proof 
    ## 
    $parentMap = @{
      "Windows-Defender-AM-Default-Definitions-OptionalWrapper" = "Windows-Defender-Client"
      "Microsoft-Hyper-V-ClientEdition" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-HVSI" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-Lxss-WithGraphics-OptionalWrapper" = "Microsoft-Windows-Lxss-Optional"
      "Containers-OptionalFeature-DisposableClientVM" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-HyperV-OptionalFeature-HypervisorPlatform-Disabled" = "Microsoft-Windows-Client-Optional-Features"
      "Microsoft-Windows-HyperV-OptionalFeature-VirtualMachinePlatform-Disabled" = "Microsoft-Windows-Client-Optional-Features"
      "Microsoft-Windows-Identity-Foundation" = "Microsoft-Windows-Client-Optional-Features"
      "Microsoft-Windows-ProjFS-OptionalFeature" = "Microsoft-Windows-Client-Optional-Features"
      "Microsoft-Windows-SimpleTCP" = "Microsoft-Windows-Client-Optional-Features"
      "Microsoft-Windows-DirectoryServices-ADAM-Client" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-OneCore-Containers-Opt" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-MSMQ-Client" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-EnterpriseClientSync-Host" = "Microsoft-Windows-Client-Features"
      "Microsoft-Windows-NFS-ClientSKU" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-PAW-Feature" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-SearchEngine-Client" = "Microsoft-Windows-Desktop-Required-SharedWithServer-Removable"
      "Microsoft-Windows-Legacy-Components-OC" = "Microsoft-Windows-Client-Features"
      "Microsoft-Windows-Printing-Foundation" = "Microsoft-Windows-Client-Features"
      "Microsoft-Windows-WinOcr" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-Windows-Telnet-Client" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-Windows-RDC" = "Microsoft-Windows-Client-Features"
      "Microsoft-Windows-Printing-XPSServices" = "Microsoft-Windows-Desktop-Required-SharedWithServer-Removable"
      "Microsoft-Windows-Printing-PrintToPDFServices" = "Microsoft-Windows-Desktop-Required-ClientOnly-Removable"
      "Microsoft-Windows-SMB1" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-SMB1Deprecation" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-MultiPoint-Connector" = "Microsoft-Windows-Enterprise-Desktop-Shared"
      "Microsoft-Windows-SmbDirect" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-DataCenterBridging" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-IIS-WebServer-AddOn" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-IIS-WebServer" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-IIS-WebServer-AddOn-2" = "Microsoft-Windows-EditionPack-Professional"

      "Microsoft-OneCore-Fonts-DesktopFonts-NonLeanSupplement" = "Microsoft-Windows-Desktop-Shared"
      "Adobe-Flash-For-Windows" = "Microsoft-Windows-Desktop-Required-ClientOnly"
      "Microsoft-Windows-SDKTools" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-Windows-Printer-Drivers" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-Windows-UI-ShellCommon-TileControl" = "Microsoft-Windows-Composable-Switcher;Microsoft-ShellCommon" 
      "Microsoft-OneCore-Biometrics-Fingerprint" = "Microsoft-OneCore-DeviceRuntime"
      "Microsoft-OneCore-SystemSettings-UserAccount" = "Microsoft-Windows-Client-Features;Microsoft-Windows-Desktop-Required-ClientOnly"
      "Microsoft-OneCore-WalletService" = "Microsoft-OneCore-AppRuntime"
      "Microsoft-OneCore-Miracast-Transmitter" = "Microsoft-OneCore-DeviceRuntime"
      "Microsoft-Windows-Printing-PremiumTools" = "Microsoft-Windows-Enterprise-Desktop-Shared"
      "Microsoft-Windows-Defrag-UI" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-Windows-ComputerManagerLauncher" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-OneCore-Cellcore-Data-Provisioning" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-OneCore-Multimedia-Acx" = "Microsoft-Windows-Desktop-Shared-Drivers"
      "Microsoft-OneCore-SystemSettings-Devices" = "Microsoft-Windows-Desktop-Required-SharedWithServer-Removable"
      "Microsoft-OneCore-Connectivity-UsbFunction" = "Microsoft-WindowsCoreHeadless-DeviceRuntime"
      "Microsoft-Windows-PEAuth-OneCore" = "Microsoft-Windows-Desktop-Required-ClientOnly-Removable"
      "Microsoft-Windows-MobilePC-Client-Basic" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-Windows-NetProfilesUX" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-OneCore-SD" = "Microsoft-WindowsCoreHeadless-DeviceRuntime"
      "Microsoft-OneCore-Connectivity-UsbConnectorManager" = "Microsoft-WindowsCoreHeadless-DeviceRuntime"
      "Microsoft-Windows-WebcamExperience" = "Microsoft-Windows-Common-RegulatedPackages"
      "Microsoft-Windows-RecoveryDrive" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-Windows-SensorDataService" = "Microsoft-Windows-Desktop-Required-SharedWithServer-Removable"
      "Microsoft-Windows-ScreenSavers-3D" = "Microsoft-Windows-Desktop-Shared"
      "LanguageFeatures-WordBreaking-Common-legacy" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-OneCore-RemoteDesktopServices-Collaboration" = "Microsoft-OneCore-AppRuntime"
      "Microsoft-Windows-ShellOptions" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-Windows-win32calc" = "Microsoft-Windows-EditionSpecific-EnterpriseS"
      "Microsoft-OneCore-WWA" = "Microsoft-OneCore-AppRuntime"
      "Microsoft-Windows-UserDeviceRegistration" = "Microsoft-Windows-Desktop-Required-ClientOnly-Removable"
      "Microsoft-Windows-BioEnrollment-UX" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-Windows-Printing-3D" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-OneCore-SystemSettings-NetworkMobileHandlers" = "Microsoft-Windows-Desktop-Required-SharedWithServer-Removable"
      "Microsoft-Windows-SnippingTool" = "Microsoft-Windows-Desktop-Shared"
      "Microsoft-Windows-DesktopFileExplorer" = "Microsoft-Windows-Required-ShellExperiences-Desktop"
      "Microsoft-Windows-3DAudio-HrtfData" = "Multimedia-AudioCore-Full"
      "Microsoft-Windows-Shell-Wallpaper-Common" = "Microsoft-Windows-Client-Features"
      "Microsoft-Windows-AppManagement-UEV" = "Microsoft-Windows-EditionPack-Professional"
      "Microsoft-Windows-BootEnvironment-Dvd" = "Microsoft-Windows-RecDisc-SDP"
    }

    Write-Host "The following packages will be removed: "
    $pkgsToRemove -join "`r`n" | Out-String | Write-Host

    if(!$suppressPrompts) {
        $goahead = Read-Host -Prompt "Are you sure you want to remove these packages? [Y|N]"
        if($goahead -notlike 'Y') {
          Write-Host "Canceled.  No packages were removed."
          return
        }
    }

    Write-Host -ForegroundColor DarkYellow @"

WARNING!!! 
Removing these packages is irreversible.  There is no way to re-install them unless you re-image your Windows.
"@

    if(!$suppressPrompts) {
        $goahead2 = Read-Host -Prompt "Please type 'remove' to confirm you want to proceed to remove the packages "
        if($goahead2 -notlike 'remove') {
            Write-Host "Canceled.  No packages were removed."
            return
        }
    }

    Write-Host "Begins packages removal..."

    if($isOnline) {
        $onOfflineArg = "/Online"
        $pkgMumsFolderPath = Join-Path -Path $env:SystemDrive -ChildPath "Windows\servicing\Packages"
    } else {
        $onOfflineArg = "/Image:$offlineImageFolderPath"
        $pkgMumsFolderPath = Join-Path -Path $offlineImageFolderPath -ChildPath "Windows\servicing\Packages"
    }

    $iter = 0
    foreach($pkg in $pkgsToRemove) {
      $iter++
      Write-Progress -Activity "Removing Packages" -Status "Uninstalling $pkg ..." -PercentComplete ($iter / $pkgsToRemove.Count * 100)           

      # some removable packages have multiple parents
      $parentsValue = $parentMap[$pkg]
      $parents = $parentsValue.Split(';')
      
      foreach($parentPkgName in $parents) {
          Get-ChildItem -Path $pkgMumsFolderPath -Name | where { $_ -match "$parentPkgName-Package~[0-9a-zA-Z]*~[.0-9a-zA-Z]*~~[.0-9]*\.mum$" } | foreach {
            $parentFQN = [System.IO.Path]::GetFileNameWithoutExtension("$_")
            & $env:windir\system32\Dism.exe $onOfflineArg /NoRestart /Disable-Feature /FeatureName:$pkg  /PackageName:$parentFQN | Write-Verbose
          }
      }
    }

    Write-Host "`r`nPackages removal finished."

    if($isOnline -and !$suppressPrompts) {
        ##
        ## if reboot is required, prompt it and reboot. 
        ##
        $reboot = Read-Host -Prompt "Reboot is required.  Do you want to reboot now? [Y|N]"
        if($reboot -like 'Y') {
          Write-Host "Rebooting now..."
  
          Restart-Computer
        }
    }

    return
}



$unitsSet = @{}
foreach($unit in $unitsList) {
  $unitsSet.Add($unit.Name, $unit) | Out-Null
}

if($RemoveUnit -ne 'All' -and !$unitsSet.Contains($RemoveUnit)) {
  throw "Removal unit named [$RemoveUnit] does not exit.  Please use 'List' command to find valid removal unit names."
}

if($RemoveUnit -eq 'All') {
  $pkgsToRemove = $unitsList.Packages | sort Name | Get-Unique
} else {
  $unitToRemove = $unitsSet[$RemoveUnit]
  $pkgsToRemove = $unitToRemove.Packages
}

Remove -pkgsToRemove $pkgsToRemove -suppressPrompts:$suppressPrompts

return
