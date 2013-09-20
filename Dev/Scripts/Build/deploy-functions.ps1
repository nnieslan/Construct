#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#deploy-functions.ps1 - contains deployment functions to wrap MSDeploy as well as SCP copy functionality.

Write-Host -Foreground Green "- Creating alias for msdeploy"

#set alias for msdeploy though we'll currently use the deploy cmd file created during packaging.
$installpath = (get-itemproperty -path "hklm:\software\microsoft\IIS Extensions\MSDeploy\1").InstallPath
set-alias -Name msdeploy -Value "$installpath\msdeploy.exe"

<#
.SYNOPSIS
deploy-web-package - Invokes MSDeploy on the package name specified for the target computer

.PARAMETER config 	
The build config to deploy such as 'Debug' or 'Dev-Deploy'

.PARAMETER package 	
The name of the MSDeploy package.  

.PARAMETER computer 	
The target computer for deployment, defaults to the local machine.

.PARAMETER user 		
The username to use during auth to the target computer

.PARAMETER password 	
The password to use during auth to the target computer

.PARAMETER whatif		
Passes the WhatIf flag to MSDeploy to see what deployment would do, rather than actually doing it.
#>
function deploy-web-package
{
    param
    (
        [string]$config = 'Debug',
        [string]$package = 'iTVProducer',
        [string]$computer = "$env:COMPUTERNAME",
        [string]$user,
        [string]$password,
        [switch]$whatif
    )
    
    $packageCmd = join-path $env:ENLISTROOT "bin\$config\_PublishedWebsites\$package\Package\$package.deploy.cmd"
    
    if(!(test-path -Path $packageCmd -PathType Leaf))
    {
        throw "The package path $packageCmd doesn't exist.";
    }        
    
    write-header -message  "Deploying $package..."
    
    $action = '/T'
    if(-not $whatif) { $action= '/Y' }
    
    $options = "/M:$computer"
    
    if($user -ne $null -and $password -ne $null)
    {
        $options = "$options /U:$user /P:$password"
    }
    
    $cmd = "$packageCmd $action $options"
    
    write-header -message " MSDeploy command: `r`n $cmd"    
    
    Invoke-expression $cmd    
}

<#
.SYNOPSIS
deploy-qa-share - For use in the build lab only.  Copies the deployment package to an SCP location for QA consumption.

.PARAMETER config 	
The build config to deploy such as 'QA-Deploy'

.PARAMETER package
The name of the MSDeploy package to zip up and copy.  
#>
function deploy-qa-share
{
	param
	(
		$config = $(throw '$config is required.'),
		$package = $(throw '$package is required.')
	)
	
    $bindir = "$env:WORKSPACE\bin\$config"
    $zipFile = "$bindir\$package_1.0.0_$env:BUILD_NUMBER.zip"
    #create zip
    new-zip $zipFile
    #add files to zip
    dir (Join-Path $bindir "_PublishedWebsites\$package\Package") | add-zip $zipFile
    
    $winscp = "C:\winscp435\WinSCP.com"
    $iniFlag = "/ini=C:\winscp435\winscp.ini"
    $scriptFlag = "/script=C:\winscp435\upload-to-releaseTars.txt /parameter $zipFile"
    if(Test-Path $winscp -PathType Leaf)
    {
        $cmd = "$winscp $iniFlag $scriptFlag"
        write-header -message " winscp command: `r`n $cmd"    
        Invoke-Expression $cmd
    }
}

<#

.SYNOPSIS
New-Zip - Creates the zip file denoted in an empty state.

.PARAMETER zipfilename 
The name of the target zip file.
#>
function New-Zip
{
	param
	(
		[string]$zipfilename = $(throw 'A name for the zip file is required')
	)
    
    write-header -message "Creating $zipfilename"    
    
	set-content $zipfilename ("PK" + [char]5 + [char]6 + ("$([char]0)" * 18))
	(gci $zipfilename).IsReadOnly = $false
}

<#

.SYNOPSIS
Add-Zip - Adds the items from the array specified to the zip file denoted.

.PARAMETER zipfilename 
The name of the target zip file.

.PARAMETER filesToAdd  
The array of filenames to add to the zip.
#>
function Add-Zip
{
	param
	(
		[string]$zipFileName = $(throw 'A name for the zip file is required'),
		[string[]]$filesToAdd = $(throw 'A list of files to include in the zip file is required')
	)

    write-header -message "Adding files to $zipFileName"    

	if(-not (test-path($zipFileName)))
	{
		set-content $zipFileName  ("PK" + [char]5 + [char]6 + ("$([char]0)" * 18))
		(gci $zipFileName ).IsReadOnly = $false	
	}
	
	write-message -message "$zipFileName creation ensured."
	
	$resolvedZipFile = (dir $zipfilename).FullName
	
	$shellApplication = new-object -com shell.application
	if($shellApplication -eq $null)
	{
		write-message "Unable to create a new shell application object."
		return;
	}
	
	write-message -message "Shell application created."
	write-message -message "Resolved zip file to path $resolvedZipFile..."
	$zipPackage = $shellApplication.NameSpace($resolvedZipFile)
	if($zipPackage -ne $null)
	{
		write-message "Zip package opened."
		foreach($file in $filesToAdd) 
		{ 
	        write-message "Adding $file to $zipFileName"
	        $zipPackage.CopyHere($file)
	        Start-sleep -milliseconds 500
		}
	}
}

<#

.SYNOPSIS
Get-Zip - Fetches a list of the files contained in the zip file denoted.

.PARAMETER zipfilename 
The name of the target zip file.
#>
function Get-Zip
{
	param([string]$zipfilename = $(throw 'A zip file to get the contents of is required.'))
	
	if(test-path($zipfilename))
	{
		$resolvedZipFile = (dir $zipfilename).FullName
		$shellApplication = new-object -com shell.application
		if($shellApplication -eq $null)
		{
			write-message "Unable to create a new shell application object."
			return;
		}
		write-message -message "Shell application created."
		$zipPackage = $shellApplication.NameSpace($resolvedZipFile)
		$zipPackage.Items() | Select Path
	}
}


<#

.SYNOPSIS
Extract-Zip - Extracts the zip denoted to the location specified.

.PARAMETER zipfilename 
The name of the target zip file.

.PARAMETER destination 
The destination folder for the extracted files.
#>
function Extract-Zip
{
	param
	(
		[string]$zipfilename = $(throw 'A zip file to extract is required.'), 
		[string] $destination = $(throw 'A destination to extract to is required')
	)

	if(test-path($zipfilename))
	{	
		$resolvedZipFile = (dir $zipfilename).FullName
		$shellApplication = new-object -com shell.application
		if($shellApplication -eq $null)
		{
			write-message "Unable to create a new shell application object."
			return;
		}
		write-message -message "Shell application created."
		$zipPackage = $shellApplication.NameSpace($resolvedZipFile)
		$destinationFolder = $shellApplication.NameSpace($destination)
		$destinationFolder.CopyHere($zipPackage.Items())
	}
}
