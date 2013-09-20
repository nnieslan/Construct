#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

param(
	[string] $pathToConfigFile = $null
	)
	
$Global:ConstructConfig = $null

<#
.SYNOPSIS
Utility function for initializing the construct configuration file from the proper location.
#>
function InitGlobalConfig{
	if ($Global:ConstructConfig -eq $null) {
		if (($pathToConfigFile -ne $null) -and ($pathToConfigFile -ne "") -and (Test-Path -Path $pathToConfigFile)) {
			write-host "An alternate Construct config file was detected at $pathToConfigFile." -ForegroundColor Yellow 
			$Global:ConstructConfig = [xml](Get-Content $pathToConfigFile)
		} else {
        	$Global:ConstructConfig = [xml] (Get-Content (Join-Path $PSScriptRoot "Construct.config"))
		}
    }
}

<#
.SYNOPSIS
Utility function for getting a nested configuration setting from the Construct configuration file.
#>
function GetConfigurationSetting{
param (
        [string] $moduleName,
		[string] $settingName
    )

    InitGlobalConfig
	
    $result = $null

    $node = $Global:ConstructConfig.configuration.constructModules.module | ? { $_.name -eq $moduleName } 
    if ($node -ne $null) {
        $result = $node.$settingName
    }

    return $result
}

<#
.SYNOPSIS
Utility function for determining if a configurable Construct module is enabled for use.
#>
function IsConfigurationModuleEnabled{
    param (
        [string] $moduleName
    )

    InitGlobalConfig
	
    $result = $false

    $node = $Global:ConstructConfig.configuration.constructModules.module | ? { $_.name -eq $moduleName } 
    
    if (($node -ne $null) -and ($node.enabled -ne "false")) {
        $result = $true
    }

    return $result
}

<#
.SYNOPSIS
Helper function for consistently logging module loads.
#>
function LogModuleLoad{
    param (
        [string] $message
    )

    Write-Host -ForegroundColor Cyan $message    

}
