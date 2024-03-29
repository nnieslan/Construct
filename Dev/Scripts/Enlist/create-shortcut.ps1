﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

set-alias -Name ces -Value create-enlistment-shortcut -Force

<#
.SYNOPSIS
Creates an enlistment shortcut for the workspace passed in if the appropriate build tools are found within that enlistment.
.PARAMETER enlistmentDir
The path to the workspace for the branch the shortcut should be created for.
.PARAMETER name
The name to give the shortcut
#>
function create-enlistment-shortcut
{
    param
    (
        [string]$enlistmentDir = $(throw 'An enlistment directory for the shortcut is required.'),
        [string]$name = $(throw 'A workspace name is required.'),
		[switch]$elevated
    )
    # PowerShell Com Object
    $WshShell = New-Object -comObject WScript.Shell

    if(!(Test-Path $enlistmentDir))
    {
        throw "$enlistmentDir doesn't exist!";
    }
    
	$buildTools = gci $enlistmentDir "construct.psm1" -Recurse
	
    if($buildTools -ne $null)
    {
        $buildToolsDir = $buildTools.Directory.FullName
		$shortcutTarget = "$env:windir\system32\windowspowershell\v1.0\powershell.exe"
        $shortcutArgs = "-NoExit -ExecutionPolicy RemoteSigned -Command ""&{ Import-Module $buildToolsDir\construct -DisableNameChecking }"""
        Write-Host -ForegroundColor White ""
        Write-Host -ForegroundColor White "- Creating shortcut on desktop :"    
        Write-Host -ForegroundColor Green "-    Name              - $name"    
        Write-Host -ForegroundColor Green "-    Working Directory - $enlistmentDir"    
        Write-Host -ForegroundColor White ""    
        
		$LinkPath = "$env:USERPROFILE\Desktop\$name.lnk"
		if(!(Test-Path -Path $LinkPath)) {
	        # Build a Shortcut Shell
	        $Shortcut = $WshShell.CreateShortcut($LinkPath)
	        $Shortcut.Description ="$name Build Environment"
	        $Shortcut.TargetPath = $shortcutTarget
	        $Shortcut.Arguments = $shortcutArgs
	        $Shortcut.WorkingDirectory = $enlistmentDir
	        $Shortcut.Save()
			
			if($elevated) {
				$LinkFile = New-Object System.IO.FileInfo($LinkPath)
				$tempFileName = [IO.Path]::GetRandomFileName()
				$tempFile = [IO.FileInfo][IO.Path]::Combine("$env:USERPROFILE\Desktop",$tempFileName)
				
				$writer = New-Object System.IO.FileStream $tempFile.FullName,([System.IO.FileMode]::Create)
				$reader = $LinkFile.OpenRead()
				
				while($reader.Position -lt $reader.Length) {
					$byte = $reader.ReadByte()
					if($reader.Position -eq 22) {
						$byte = 34
					}
					
					$writer.WriteByte($byte)
					
				}
				
				$reader.Close()
				$writer.Close()
				$LinkFile.Delete()
				Rename-Item -Path $tempFile.FullName -NewName $LinkPath
				
			}
		}
    }
    else
    {
        Write-Host -ForegroundColor White ""
        Write-Host -ForegroundColor Yellow "- Skipping Shortcut Creation : This workspace doesn't support it."    
        Write-Host -ForegroundColor White ""    
    }
}