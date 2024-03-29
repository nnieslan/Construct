﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#common functions for all build environments (utility methods)

<#
.SYNOPSIS
Displays a formated string message with color-coding and borders
.PARAMETER message
The message to display
#>
function write-header
{
    param
    (
        [string]$message
    )
    
	if($Host.Name -eq "ConsoleHost")
	{
	    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
	    Write-Host -ForegroundColor White ""
	    Write-Host -ForegroundColor Green $message    
	    Write-Host -ForegroundColor White ""
	    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
	}
	elseif($msbuildLog -ne $null)
	{
		$msbuildLog.LogMessage([Microsoft.Build.Framework.MessageImportance]"High", `
                    "Construct PowerShell : {0}", $message)
	}
}

<#
.SYNOPSIS
Displays the message passed in to the host or log if possible.
.PARAMETER message
The message to display
#>
function write-message
{
	param
	(
		[string]$message,
		[string]$color = "Green"
	)
	
	if($Host.Name -eq "ConsoleHost")
	{
		Write-Host -ForegroundColor $color $message    
	}
	elseif($msbuildLog -ne $null)
	{
		$msbuildLog.LogMessage([Microsoft.Build.Framework.MessageImportance]"Normal", `
                  "Construct PowerShell : {0}", $message)
	}
}

<#
.SYNOPSIS
Displays the error passed in to the host or log if possible.
.PARAMETER message
The message to display
#>
function write-error
{
	param
	(
		[string]$message
	)
	
	if($Host.Name -eq "ConsoleHost")
	{
		Write-Host -ForegroundColor Red $message    
	}
	elseif($msbuildLog -ne $null)
	{
		$msbuildLog.LogError([Microsoft.Build.Framework.MessageImportance]"High", `
                  "Construct PowerShell : {0}", $message)
	}
}

<#
.SYNOPSIS
Displays the warning passed in to the host or log if possible.
.PARAMETER message
The message to display
#>
function write-warning
{
	param
	(
		[string]$message
	)
	
	if($Host.Name -eq "ConsoleHost")
	{
		Write-Host -ForegroundColor Yellow $message    
	}
	elseif($msbuildLog -ne $null)
	{
		$msbuildLog.LogWarning([Microsoft.Build.Framework.MessageImportance]"High", `
                  "Construct PowerShell : {0}", $message)
	}
}

<#
.SYNOPSIS
Tests the incoming path to ensure it's compatible with Construct.
.DESCRIPTION
Testing ensures that the path given has an MSBuild 'dirs.proj' traversal project file present.  
.PARAMETER candidate
The path to test.
#>
function test-buildpath {
	param
	(
		[string]$candidate
	)
	
	$srcRoot = Resolve-Path $candidate
	$dirs = Join-Path $srcRoot 'dirs.proj'
	return (Test-Path $dirs)
}

<#
.SYNOPSIS
Sets a set of env-specific aliases as defined by a file.
.PARAMETER aliasPath
The path containing the user-specified aliases.
#>
function set-env-aliases {
    param
    (
        [string]$aliasPath = $PSScriptRoot
    )
    
    if((Test-Path -PathType Leaf "$aliasPath\aliases"))
    {
        if((Test-Path -PathType Leaf "$aliasPath\aliases.func.ps1"))
        {
            Remove-Item "$aliasPath\aliases.func.ps1"
        }
        write-message "- Found $aliasPath\aliases...";
        $aliases = import-csv "$aliasPath\aliases"
        $aliases | foreach-object -process {  
            write-message " - Setting alias $_ ";
            
            if(!(Test-Path -PathType Leaf "$aliasPath\aliases.func.ps1"))
            {
                out-file -filepath "$aliasPath\aliases.func.ps1" -InputObject "" -Force
            }   
            
            $function = 'function ' + $_.Name + '_Func { ' + $_.Value + ' }';
            $alias = 'set-alias -Name ' + $_.Name + ' -Value ' + $_.Name  + '_Func -Force -Scope Global -Option AllScope;';
            
            out-file -filepath "$aliasPath\aliases.func.ps1" -InputObject $function -Append
            out-file -filepath "$aliasPath\aliases.func.ps1" -InputObject $alias -Append   
        }
    }
}

<#
.SYNOPSIS
Takes in a string and makes it an executable script block.
.PARAMETER string
The string to convert into a script block for execution
#>
function Convert-StringToScriptBlock {  
   param(   
   [parameter(ValueFromPipeline=$true,Position=0)] 
   [string]$string
   )
   $sb = [scriptblock]::Create($string)
   return $sb
}

function Get-Proxy([string]$cmd = $(Read-Host "Command")) { 
    $md = New-Object Management.Automation.CommandMetaData (Get-Command $cmd)
    $params = $pipe = ''
    foreach ($a in $args) { 
        if ($a -is [ScriptBlock]) {
         $pipe += "| $a" -replace '\$', '$$$$'
        } else {
         $params += " $a" -replace '\$', '$$$$'
         if ($a[0] -eq '-') { [void]$md.Parameters.Remove($a.Substring(1)) } 
        }
    }
    [Management.Automation.ProxyCommand]::Create($md) `
        -replace 'wrappedCmd @PSBoundParameters', "wrappedCmd$params @PSBoundParameters $pipe"
}

function New-ProxyFunction([string]$name = $(Read-Host "Name"), [string]$cmd = $(Read-Host "Command")) { 
    [void](New-Item -Path function: -Name global:$name -Value (Get-Proxy $cmd @args))
}

<#
.SYNOPSIS
Converts a JSON string into a PowerShell hashtable using the .NET System.Web.Script.Serialization.JavaScriptSerializer
.PARAMETER json
The string of JSON to deserialize
#>
function ConvertFrom-Json {	
	param(
		[string] $json
	)
	
	# load the required dll
    [void][System.Reflection.Assembly]::LoadWithPartialName("System.Web.Extensions")
    $deserializer = New-Object -TypeName System.Web.Script.Serialization.JavaScriptSerializer
	$dict = $deserializer.DeserializeObject($json)
	
	return $dict
}

function Sign-PsModule{
	param(
		[string]$pathToPfxCertificate = $(throw 'A path to a certificate file is required'),
		[string]$psModulePath = $(throw 'A path to a module is required.'))
		
		$signatureFile=Get-PfxCertificate -FilePath $pathToPfxCertificate;
		
		$tempFilePath = "$psModulePath.tmp"
		if (Test-Path $tempFilePath) {
			Remove-Item -Path $tempFilePath
		}
		
		# this step is necessary because only UTF8 files can be reliably signed.
		Move-Item -Path $psModulePath -Destination $tempFilePath
		Get-Content $tempFilePath | Set-Content $psModulePath -Encoding UTF8
		Set-AuthenticodeSignature -FilePath $psModulePath -Certificate $signatureFile 
}

<#
.SYNOPSIS
Signs Powershell Modules with the specified private key.
.PARAMETER pathToPfxCertificate
The code signing cert to use.
.PARAMETER psModulePath
The path to the folder containing the PowerShell Module to sign scripts for.
#>
function Sign-PSModules{
	param(
		[string]$pathToPfxCertificate = $(throw 'A MSBuild project file is required'),
		[string]$psModulePath)
	
	if ($psModulePath -eq $null -or $psModulePath -eq "") {
		$psModulePath = Get-Location
	}
	
	$signatureFile=Get-PfxCertificate -FilePath $pathToPfxCertificate
	
	Get-ChildItem "$psModulePath\*" -Include @("*.ps1", "*.psm1") | % { 
		$tempFilePath = "$_.tmp"
		if (Test-Path $tempFilePath) {
			Remove-Item -Path $tempFilePath
		}
		
		# this step is necessary because only UTF8 files can be reliably signed.
		Move-Item -Path $_ -Destination $tempFilePath
		Get-Content $tempFilePath | Set-Content $_ -Encoding UTF8
		Set-AuthenticodeSignature -FilePath $_ -Certificate $signatureFile 
	}
}

<#
.SYNOPSIS
Signs a Binary such as an exe or DLL with the specified private key.
.PARAMETER pathToPfxCertificate
The code signing cert to use.
.PARAMETER $filePath
The path to the binary to sign.
#>
function Sign-Binary {
	param(
		[string]$pathToPfxCertificate = $(throw 'A PFX file is required'),
		[string]$filePath = $(throw 'A binary file to sign is required')
		)
	
	$signatureFile=Get-PfxCertificate -FilePath $pathToPfxCertificate
	Set-AuthenticodeSignature -FilePath $filePath `
							-Certificate $signatureFile `
							-TimestampServer "http://timestamp.verisign.com/scripts/timstamp.dll"
}

<#
.SYNOPSIS
Converts a PowerShell hashtable into a JSON string using the .NET System.Web.Script.Serialization.JavaScriptSerializer
.PARAMETER dict
The object to serialize into JSON.
#>
function CovertTo-Json {
	param(
		[Object] $dict
		)
		
		# load the required dll
    [void][System.Reflection.Assembly]::LoadWithPartialName("System.Web.Extensions")
    $serializer = New-Object -TypeName System.Web.Script.Serialization.JavaScriptSerializer
	$json = $serializer.DeserializeObject($dict)
	
	return $json
	
}

<#
.SYNOPSIS
Performs synchronous HTTP communication with the target specified.
.DESCRIPTION
Using the System.Net.WebRequest object, creates an HTTP web request 
populating it with the $content specified and submitting the request 
to the $target indicated using the incoming $verb.
.PARAMETER target
The target URL to communicate with
.PARAMETER authHeader
The content of an AUTHORIZATION header to add to the HTTP request.
Examples: "Basic someEncodedUserPass" or "token someOAuthToken"
.PARAMETER verb
The HTTP verb to assign the request.
.PARAMETER content
The string content to encode and add to the request body.
#>
function Execute-HttpCommand() {
    param(
        [string] $target,
		[string] $authHeader,
		[string] $verb,	
		[string] $content
    )

	$webRequest = [System.Net.WebRequest]::Create($target)
    $encodedContent = [System.Text.Encoding]::UTF8.GetBytes($content)
    $webRequest.Headers.Add("Authorization", $authHeader);
    $webRequest.Method = $verb

	write-message -message "Http Url: $target"
	write-message -message "Http Verb: $verb"
	write-message -message "Http authorization header: $authHeader"
	write-message -message "Http Content: $content"
	if($encodedContent.length -gt 0) {
		
		$webRequest.ContentLength = $encodedContent.length
    	$requestStream = $webRequest.GetRequestStream()
    	$requestStream.Write($encodedContent, 0, $encodedContent.length)
    	$requestStream.Close()
	}

    [System.Net.WebResponse] $resp = $webRequest.GetResponse();
	if($resp -ne $null) {
    	$rs = $resp.GetResponseStream();
    	[System.IO.StreamReader] $sr = New-Object System.IO.StreamReader -argumentList $rs;
    	[string] $results = $sr.ReadToEnd();
	    return $results;
	}
	return '';
}