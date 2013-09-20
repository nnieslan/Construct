#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#env-functions.ps1 - a set of utility functions for the build environment.

set-alias -name help -value get-help -Force -Scope Global -Option AllScope -Description "Construct build environment alias"
set-alias -name grep -value findstr.exe -Force -Scope Global -Option AllScope -Description "Construct build environment alias"
set-alias -name ff -value where.exe -Force -Scope Global -Option AllScope -Description "Construct build environment alias"
set-alias -name isr -value iisreset.exe -Force -Scope Global -Option AllScope -Description "Construct build environment alias"
set-alias -name ll -value gci -Force -Scope Global -Option AllScope -Description "Construct build environment alias"

<#
.SYNOPSIS
 Sets visual studio environment vars for the Construct session based on the Visual Studio command prompt location passed in.
.PARAMETER visualStudioInitFilePath
The quoted path (containing powershell env:VAR syntax if necessary) to the Visual Studio Command Prompt to load and initialize from.
#>
function init-visualstudio-envvars
{
	param
	(
		[string] $visualStudioInitFilePath = $null
	)
		#write-message "Path from Configuration File : $visualStudioInitFilePath" "Red"
		if($env:PROCESSOR_ARCHITECTURE  -ne 'AMD64') {
			$visualStudioInitFilePath = $visualStudioInitFilePath.Replace("env:PROGRAMFILES(x86)","env:PROGRAMFILES")
		}
		#write-message "Normalized Path : $visualStudioInitFilePath" "Red"
		$expanded = $ExecutionContext.InvokeCommand.ExpandString($visualStudioInitFilePath)
		$expanded = $expanded.Replace("""", """""")
		$cmd = "$env:ComSpec /c ""$expanded & set"""
		#write-message $cmd "Red"
		 $ExecutionContext.InvokeCommand.InvokeScript($cmd) | % {
			$tokenPart = $_.Substring(0, 5)
			if ($tokenPart -eq "PATH=") {
			$pv = $_.split("=");

			$pathPart = $pv[1]
			$env:PATH = $env:PATH + ";$pathPart"
			}
	  		elseif ($_ -match "=") {
    			$v = $_.split("="); set-item -force -path "ENV:\$($v[0])"  -value "$($v[1])"
				#write-message "ENV:\$($v[0]) $($v[1])"				
  			}
		}
		write-message "Visual Studio Command Prompt environment variables set."
	}

<#
.SYNOPSIS
Utility function for loading addition user-defined paths into the PATH environment variable.
#>
function add-user-paths
{
	if ($Global:UserDefinedPaths -ne $null)
	
	{
		foreach ($path in $Global:UserDefinedPaths) {
			$env:PATH = $env:PATH + ";$path"
		}
	}
}


<#
.SYNOPSIS
Sets a set of env-specific aliases as defined by a file.
#>
function set-env-aliases
{
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
        write-host -ForegroundColor Green "- Found $aliasPath\aliases...";
        $aliases = import-csv "$aliasPath\aliases"
        $aliases | foreach-object -process {  
            write-host -ForegroundColor Green " - Setting alias $_ ";
            
            if(!(Test-Path -PathType Leaf "$aliasPath\aliases.func.ps1"))
            {
                out-file -filepath "$aliasPath\aliases.func.ps1" -InputObject ""
            }   
            
            $function = Build-FunctionDeclaration $_.Name $_.Value
            $alias = 'set-alias -Name ' + $_.Name + ' -Value ' + $_.Name  + '_Func -Force -Scope Global -Option AllScope;';
            
            out-file -filepath "$aliasPath\aliases.func.ps1" -InputObject $function -Append
            out-file -filepath "$aliasPath\aliases.func.ps1" -InputObject $alias -Append   
        }
    }
}

<#
.SYNOPSIS
Helper function used for code generation of user-defined helper content.
#>
function Build-FunctionDeclaration
{
	param(
		[string]$name,
		[string]$command
	)
	
	$decl = "function $name`_Func { `n`t" + '$cmd = "' + $command + '"' + "`n`t"
	$decl = $decl + 'if ($args -ne $null) {' + "`n`t`t"
	$decl = $decl + '$args | foreach -process {' + "`n`t`t`t"
	$decl = $decl + '$cmd = $cmd + " " + $_' + "`n`t`t"
	$decl = $decl + "}`n`t"
	$decl = $decl + "}`n`t"
	
	$decl = $decl + 'iex $cmd' + "`n"
	
	$decl = $decl + "}"
	
	return $decl	
}



