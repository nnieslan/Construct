#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#env-init - Initializes the current powershell enviornment, setting aliases and importing modules
param( 
		[string]$envRoot = $(Get-Location),
		[string]$pathToConfigFile = $null
	)
	
#check the current execution policy. If the execution policy is no unrestricted set the execution policy to remote signed.
$currentExecutionPolicy = Get-ExecutionPolicy
if($currentExecutionPolicy  -ne "Unrestricted")
{
	Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -ErrorAction:SilentlyContinue | Out-Null
}

#loading common utility functions
Write-Host -Foreground Green "- Adding common utility functions..."
if((Test-Path -PathType Leaf "$PSScriptRoot\common-functions.ps1"))
{
    . (Join-Path -Path $PSScriptRoot -ChildPath "common-functions.ps1")
}
if((Test-Path -PathType Leaf "$PSScriptRoot\create-shortcut.ps1"))
{
    . (Join-Path -Path $PSScriptRoot -ChildPath "create-shortcut.ps1")
}


write-message "Initializing Environment and setting up aliases..."
write-message "Ensuring $envRoot as a Construct-compatible environment root..."
if(!(test-buildpath $envRoot)) {
	write-warning "$envRoot is not a Construct-compatible location, attempting to find a compatible location."
	$envRoot = resolve-path "$PSScriptRoot\.."
	if(!(test-buildpath $envRoot)) {
		[bool]$finished = $false
		while((-not $finished) -and ($Host.Name -eq "ConsoleHost")) {
			$envRoot = Read-Host "Enter the path containing the codebase you'd like Construct to use for this session"
			$finished = (test-buildpath $envRoot)
			if(-not $finished) { write-error "$envRoot is not a Construct-compatible location!" }
		}
	}
}

#define the environment root as the parent of the current build folder.
$root = resolve-path $envRoot;

#check for and update the environment variable for ENLISTROOT
if(!(test-path env:\ENLISTROOT))
{
    write-message "- Adding environment variable ENLISTROOT = $root...";
    new-item -path env:. -name ENLISTROOT -value $root
}
else
{
    write-message "- Resetting environment variable ENLISTROOT = $root...";
    set-item -path env:\ENLISTROOT -value $root
}

if (!(Test-Path -Path:(Join-Path -Path:"$PSScriptRoot" -ChildPath:"ConstructConfig.ps1"))) {
    write-error "Construct configuration module was not found!"
}

. (Join-Path -Path:"$PSScriptRoot" -ChildPath:"ConstructConfig.ps1") -pathToConfigFile:$pathToConfigFile

#create a new HOME env var if necessary, 
#ensure it's pointing to the parent of an existing .ssh folder if at all possible.
if (IsConfigurationModuleEnabled("GitHub")) {
    if(!(test-path env:\HOME))
    {
        $userSsh = join-path $env:USERPROFILE '.ssh'
        if(Test-Path($userSsh))
        {
            new-item -path env:. -name HOME -value $env:USERPROFILE | out-null
        }
        else
        {
            new-item -path env:. -name HOME -value $root | out-null
        }
    }

	$githubAddIn = Resolve-Path "$env:LOCALAPPDATA\GitHub\shell.ps1"
    if($githubAddIn -ne $null -and Test-Path -Path $githubAddIn) {
       . (Resolve-Path "$githubAddIn")
    } else {
	
	}
}

#navigated the command prompt to the root of the environment
set-location $env:ENLISTROOT 

#In the section below, we passively add optional powershell modules for extending functionality

#Pscx - Used if you have deployed the (Powershell Community)Pscx modules on your machine and would like them included
#you can find them on www.codeplex.com
Import-Module Pscx -ErrorAction:SilentlyContinue

if (IsConfigurationModuleEnabled("Microsoft.Azure")) {
    LogModuleLoad "Attempting load of Microsoft.Azure modules..."
	#Sharepoint - Used if you have SharePoint installed for maintenance of a SP farm
	if(((get-module Azure) -eq $null)) 
    {	
		write-message "- Attempting to add the Azure Module"
        #configure powershell with Azure module & functions
		Import-Module Azure -ErrorAction:SilentlyContinue
    }
	
    if((Test-Path -PathType Leaf "$PSScriptRoot\azure-functions.ps1")) {
        . (Join-Path -Path $PSScriptRoot -ChildPath "azure-functions.ps1")
    }
}

if (IsConfigurationModuleEnabled("Microsoft.SharePoint")) {
    LogModuleLoad "Attempting load of Microsoft.SharePoint modules..."
    #Sharepoint - Used if you have SharePoint installed for maintenance of a SP farm
    $spsnapin = $(get-pssnapin | where {$_.Name -eq "Microsoft.SharePoint.PowerShell" })
    if($spsnapin -eq $null) 
    {
        write-message "- Attempting to add the SharePoint PSSnapin"
        Add-PsSnapin Microsoft.SharePoint.PowerShell -ErrorAction:SilentlyContinue
    }
}

if (IsConfigurationModuleEnabled("Microsoft.WebAdministration")) {
    LogModuleLoad "Attempting load of Microsoft.WebAdministration modules..."

    #Web Administration - used if you have IIS 7+ installed to manage IIS hosted web sites, etc.
    $spsnapin = $(get-pssnapin | where {$_.Name -eq "WebAdministration" })
    if($spsnapin -eq $null)
    {
        write-message "- Adding the WebAdministration Module"
        Import-Module WebAdministration -ErrorAction:SilentlyContinue
    }
}

#In the section below, we passively load functions based on their functionality and existence.

write-message "- Adding custom functions and aliases..."
if((Test-Path -PathType Leaf "$PSScriptRoot\env-functions.ps1"))
{
    . (Join-Path -Path $PSScriptRoot -ChildPath "env-functions.ps1")
}

if (IsConfigurationModuleEnabled("Microsoft.VirtualMachineManager")) {
    LogModuleLoad "Attempting load of Microsoft.VirtualMachineManager modules..."

    #SCVMM - Used if you have SCVMM admin console installed to allow management of a virtual machine farm
    $spsnapin = $(get-pssnapin | where {$_.Name -eq "Microsoft.SystemCenter.VirtualMachineManager" })
    if($spsnapin -eq $null)
    {
        write-message "- Attempting to add the SCVMM PSSnapin"
        Add-PsSnapin Microsoft.SystemCenter.VirtualMachineManager -ErrorAction:SilentlyContinue
    }

    if((Test-Path -PathType Leaf "$PSScriptRoot\vm-functions.ps1"))
    {
        . (Join-Path -Path $PSScriptRoot -ChildPath "vm-functions.ps1")
    }
}

if (IsConfigurationModuleEnabled("GitHub")) {
    LogModuleLoad "Attempting load of GitHub modules..."

    if((Test-Path -PathType Container "$PSScriptRoot\git") -and (Test-Path -PathType Leaf "$PSScriptRoot\git-functions.ps1"))
    {
        write-message "- Adding Git source control environment functions..."
        . (Join-Path -Path $PSScriptRoot -ChildPath "git-functions.ps1")
    } else {
        $gitCmds = get-command git*
        if($gitCmds -ne $null)
        {
            write-message "- Adding Git source control environment functions..."
            . (Join-Path -Path $PSScriptRoot -ChildPath "git-functions.ps1")        
        }
    }
}

if (IsConfigurationModuleEnabled("Microsoft.TeamFoundationServer")) {
    LogModuleLoad "Attempting load of Microsoft.TeamFoundationServer modules..."

    #loading TFS source control functionality
    if((Test-Path -PathType Leaf "$PSScriptRoot\tfs-functions.ps1"))
    {
        write-message "- Adding TFS source control environment functions..."
        . (Join-Path -Path $PSScriptRoot -ChildPath "tfs-functions.ps1")
    }
}

#loading the main build functions
write-message "- Adding the build functions..."
if((Test-Path -PathType Leaf "$PSScriptRoot\build-functions.ps1"))
{
    . (Join-Path -Path $PSScriptRoot -ChildPath "build-functions.ps1")
}

#loading the build wrapper functions for ease of use.
if((Test-Path -PathType Leaf "$PSScriptRoot\build.ps1"))
{
    . (Join-Path -Path $PSScriptRoot -ChildPath "build.ps1")
}

#loading all web deployment functions.
if (IsConfigurationModuleEnabled("Microsoft.WebDeploy")) {
    LogModuleLoad "Attempting load of Microsoft.WebDeploy modules..."

    if((Test-Path -PathType Leaf "$PSScriptRoot\deploy-functions.ps1"))
    {
        . (Join-Path -Path $PSScriptRoot -ChildPath "deploy-functions.ps1")
    }
}

#loading navigation and other helpful aliases.  
#If you want to add more aliases to the environment for ease of use, 
#do so by adding the alias and the action it shortcuts as a name, value 
#pair in the aliases file contained in this folder.
if (IsConfigurationModuleEnabled("Construct.GlobalAliases")) {
    LogModuleLoad "Attempting load of the Construct.GlobalAliases module..."

	set-env-aliases
	if((Test-Path -PathType Leaf "$PSScriptRoot\aliases.func.ps1"))
	{
    	. "$PSScriptRoot\aliases.func.ps1" 
	}
}

if (IsConfigurationModuleEnabled("Microsoft.VisualStudioEnvironment")) {
    LogModuleLoad "Attempting load of Microsoft.VisualStudio environment variables..."
    $vsInitString = GetConfigurationSetting "Microsoft.VisualStudioEnvironment" "initString"
    write-message "Initializing Visual Studio with the string: $vsInitString" "Yellow"
    init-visualstudio-envvars $vsInitString
}

if (IsConfigurationModuleEnabled("Construct.UserMappings")) {
    LogModuleLoad "Attempting to configure user tools and folder mappings..."

    $userNameMappingOverride = GetConfigurationSetting "Construct.UserMappings" "versionControlMappingDisabled"
    if (($userNameMappingOverride -eq $null) -or ($userNameMappingOverride -ne "True")) {
        add-user-folder-mapping
    }
	
    $userToolsPath = GetConfigurationSetting "Construct.UserMappings" "alternateUserToolsPath"
    if ($userToolsPath -eq $null) {
        $userToolsPath = "$env:ENLISTROOT\Users\$env:USERNAME"
    }
    
    #store it as an environment variable for other scripts to refer to it by
    new-item -path env:. -name USERTOOLSPATH -value $userToolsPath	
	
    write-message "- Adding user-defined aliases for current environment...";
    set-env-aliases -aliasPath $userToolsPath
    if((Test-Path -PathType Leaf "$userToolsPath\aliases.func.ps1"))
    {
        #this is an auto-generated file based on the existence of an aliases csv file in the user's directory
        #create a CSV file named "aliases" with the same format as $env:ENLISTROOT\build\aliases to use this.
        . "$userToolsPath\aliases.func.ps1" 
    }
    if((Test-Path -PathType Leaf "$userToolsPath\user.func.ps1"))
    {
        #this is a script created by the user within their user directory
        #add functions and other customizations here.
        . "$userToolsPath\user.func.ps1" 
    }

    add-user-paths
}

Export-ModuleMember -Alias * -Function * -Cmdlet *
