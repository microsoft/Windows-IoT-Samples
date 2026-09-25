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
  [String]$RemoveUnit,
  [String]$os_variant,
  [String]$DismLogFile
)
if (!$os_variant) {
  $os_variant = "IoTEnterpriseS"
}
$dismLogFileArg = ""
if ($DismLogFile) {
  $dismLogFileArg = "/LogPath:$DismLogFile"
}

$unitsList = @(
[PSCustomObject]@{
  Name = "Shell_Wallpaper"
  Packages = @(
  "Microsoft-Windows-Shell-Wallpaper-Common"
  )
  Description = "Wallpapers."
},
[PSCustomObject]@{
  Name = "Fonts_DesktopFonts_NonLeanSupplement"
  Packages = @(
  "Microsoft-OneCore-Fonts-DesktopFonts-NonLeanSupplement"
  )
  Description = "Additional Chinese, Japanese and Korean fonts."
},
[PSCustomObject]@{
  Name = "SensorDataService"
  Packages = @(
  "Microsoft-Windows-SensorDataService"
  )
  Description = "Legacy camera and image integration service to support Windows Hello for devices using old IR cameras."
},
[PSCustomObject]@{
  Name = "LanguageFeatures_WordBreaking_Common_legacy"
  Packages = @(
  "LanguageFeatures-WordBreaking-Common-legacy"
  )
  Description = "Legacy neutral word breaker, should only be needed in very rare app compat scenarios."
},
[PSCustomObject]@{
  Name = "BioEnrollment_UX"
  Packages = @(
  "Microsoft-Windows-BioEnrollment-UX"
  )
  Description = "Windows Hello Setup UI. "
},
[PSCustomObject]@{
  Name = "Printer_Drivers"
  Packages = @(
  "Microsoft-Windows-Printer-Drivers"
  )
  Description = "Printer drivers."
},
[PSCustomObject]@{
  Name = "RecoveryDrive"
  Packages = @(
  "Microsoft-Windows-RecoveryDrive"
  )
  Description = "Recovery Media Creator."
},
[PSCustomObject]@{
  Name = "ScreenSavers"
  Packages = @(
  "Microsoft-Windows-ScreenSavers-3D"
  )
  Description = "3D screen savers."
},
[PSCustomObject]@{
  Name = "ShellOptions"
  Packages = @(
  "Microsoft-Windows-ShellOptions"
  )
  Description = "Calc.exe, Character Map."
},
[PSCustomObject]@{
  Name = "AppManagement_UEV"
  Packages = @(
  "Microsoft-Windows-AppManagement-UEV"
  )
  Description = "User Experience Virtualization (UE-V)"
},
[PSCustomObject]@{
  Name = "win32calc"
  Packages = @(
  "Microsoft-Windows-win32calc"
  )
  Description = "Win32 Calculator."
},
[PSCustomObject]@{
  Name = "Printing_PremiumTools"
  Packages = @(
  "Microsoft-Windows-Printing-PremiumTools"
  )
  Description = "Printing Premium Tools Collection."
},
[PSCustomObject]@{
  Name = "BootEnvironment_Dvd"
  Packages = @(
  "Microsoft-Windows-BootEnvironment-Dvd"
  )
  Description = "DVD boot configuration."
},
[PSCustomObject]@{
  Name = "Desktop_SharedPackages"
  Packages = @(
  "LanguageFeatures-WordBreaking-Common-legacy",
  "Microsoft-Windows-BioEnrollment-UX",
  "Microsoft-Windows-Printer-Drivers",
  "Microsoft-Windows-RecoveryDrive",
  "Microsoft-Windows-ScreenSavers-3D",
  "Microsoft-Windows-ShellOptions",
  "Microsoft-OneCore-Fonts-DesktopFonts-NonLeanSupplement"
  )
  Description = "Desktop Shared Features"
},
[PSCustomObject]@{
  Name = "Common_RegulatedPackages"
  Packages = @(
  "Microsoft-Media-Foundation",
  "Microsoft-Windows-Media-Format",
  "Microsoft-Windows-Media-Streaming",
  "Microsoft-Windows-MediaPlayback-OC",
  "Microsoft-Windows-Portable-Devices",
  "Microsoft-Windows-WebcamExperience",
  "Microsoft-Windows-WinSATMediaFiles"
  )
  Description = 
@"
Media features.  https://support.microsoft.com/en-us/topic/media-feature-pack-for-windows-10-n-may-2020-ebbdf559-b84c-0fc2-bd51-e23c9f6a4439
"@
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
if(!($currEditionValue -like $os_variant)) {
  throw "The current edition is $currEditionValue.  This scrip is only supported in IoTEnterpriseS/EnterpriseG."
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
    $pkgsremoved = 0
    foreach($pkg in $pkgsToRemove) {
      $iter++
      Write-Progress -Activity "Removing Packages" -Status "Uninstalling $pkg ..." -PercentComplete ($iter / $pkgsToRemove.Count * 100)           

      $removalOut = $(& $env:windir\system32\Dism.exe $onOfflineArg $dismLogFileArg /NoRestart /Disable-Feature /FeatureName:$pkg  /PackageName:'@Package' )
      $errsubstr = $($removalOut -match 'Error: (0x)*[\d\w]+')
      $generrsubstr = $($removalOut -match 'Error: [\d\w]+')
      if($null -ne $errsubstr) {
        Write-Host "Dism Removal of package $pkg Failed with $errsubstr" -ForegroundColor White -BackgroundColor Red
      } elseif($null -ne $generrsubstr) {
        Write-Host "Dism Removal of package $pkg Failed with $generrsubstr" -ForegroundColor White -BackgroundColor Red
      } else {
        $pkgsremoved++
      }
    }

    Write-Host "`r`n $pkgsremoved - Packages removal finished."

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
