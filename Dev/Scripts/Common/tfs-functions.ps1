﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#tfs-functions.ps1 - TFS related functions and aliases
if($env:PROCESSOR_ARCHITECTURE  -eq 'AMD64') {
	$pfx86 = get-content "env:programfiles(x86)"
} else {
	$pfx86 = get-content "env:programfiles"
}
set-alias -Name tf -Value "$pfx86\Microsoft Visual Studio 11.0\Common7\IDE\TF.exe" -Force -Scope Global -Option AllScope -Description "Construct build environment alias"
set-alias -Name tcm -Value "$pfx86\Microsoft Visual Studio 11.0\Common7\IDE\TCM.exe" -Force -Scope Global -Option AllScope -Description "Construct build environment alias"
set-alias -Name tfpt -Value "$pfx86\Microsoft Team Foundation Server 2010 Power Tools\TFPT.EXE" -Force -Scope Global -Option AllScope -Description "Construct build environment alias"

if(!(Test-Path env:\TFSSERVERURL))
{
	$scriptDirectory = Split-Path $MyInvocation.MyCommand.Path
    
	if(($tfsServerUrl -eq '' -or $tfsServerUrl -eq $null) -and (Test-Path $scriptDirectory\tfsserverurl.txt -PathType Leaf))
	{
		$tfsServerUrl =  Get-Content "$scriptDirectory\tfsserverurl.txt"
	}
	elseif((Test-Path $env:ENLISTROOT\build -PathType Container))
	{
		$tfsServerUrl = (tf workspaces | ?{$_ -match "Collection:"}).Replace("Collection:", "").Trim()
	}
	else
	{
		$tfsServerUrl = read-host "Construct was unable to determine your TFS Server. Please enter one: "
	}

	new-item -path env:. -name TFSSERVERURL -value $tfsServerUrl | Out-Null
}
<#
.SYNOPSIS
Executes a forward integration of code branches based on
the current workspace mapping and it's parent branch.
#>
function forward-integrate
{
    param
    (
    [parameter(Mandatory=$false,
        Position=0,
        HelpMessage="The TFS team collection URL. Optional.")]
        [alias("TFS")]
	    [string]$teamCollectionUrl = $env:TFSSERVERURL,
	[parameter(Mandatory=$false,
        Position=1,
        HelpMessage="A switch used to silence confirmation.")]
        [switch]$confirm
    )
    # load the required dll
    [void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.TeamFoundation.Client")
    
    #set the TFS server location
    [psobject] $tfs = get-tfs -serverName $teamCollectionUrl;    
    $wkspc = $tfs.vcs.GetWorkspace($env:ENLISTROOT);    
    $currentBranch = $wkspc.Folders[0].ServerItem;    
    write-header "Current Branch : $currentBranch "    
    
    $itemSpec = new-object Microsoft.TeamFoundation.VersionControl.Client.ItemSpec -ArgumentList @( $currentBranch, [Microsoft.TeamFoundation.VersionControl.Client.RecursionType]::None )
    $history = $tfs.vcs.GetBranchHistory( @($itemSpec), [Microsoft.TeamFoundation.VersionControl.Client.VersionSpec]::Latest )
    
    $history | foreach-object -process {
        $_ | foreach-object -process {
                $relative = $_.Relative; 
                if($relative.BranchToChangeType -match 'Add')
                {
                    $parent = $relative.BranchToItem.ServerItem;
                    write-header "Parent Branch : $parent ";
                            
                    if(-not $confirm)
                    {
                        $yn = Read-Host -Prompt "Are you sure you want to integrate? (y or n)"
                        if($yn -match 'n')
                        {
                            return;
                        }                
                    }
                    write-header -message "Getting Latest on $currentBranch ..."; 
                    tf get /recursive *.*;
                    
                    write-header -message "Performing Forward Integration from $parent to $currentBranch ..."; 
                    tf merge /recursive /noprompt $parent $currentBranch;
                    
                    write-header -message "Resolving version files by taking parent version number..."; 
                    tf resolve AssemblyInfo.* /auto:TakeTheirs /recursive
                    tf resolve Version.cs /auto:TakeTheirs /recursive
                }
            }
        }
}
set-alias fi forward-integrate -Force -Scope Global -Option AllScope -Description "Construct build environment alias"

<#
.SYNOPSIS
Executes a reverse integration of code branches based on
the current workspace mapping and the indicated child branch.
#>
function reverse-integrate
{
     param
    (
    [parameter(Mandatory=$true,
        Position=0,
        HelpMessage="The child branch to RI from.")]
        [alias("C")]
	    [string]$child = $(throw 'A child branch is required'),
    [parameter(Mandatory=$false,
        Position=1,
        HelpMessage="The TFS team collection URL. Optional.")]
        [alias("TFS")]
	    [string]$teamCollectionUrl = $env:TFSSERVERURL,
	[parameter(Mandatory=$false,
        Position=2,
        HelpMessage="A switch used to silence confirmation.")]
        [switch]$confirm
    )
    # load the required dll
    [void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.TeamFoundation.Client")
    
    #set the TFS server location
    [psobject] $tfs = get-tfs -serverName $teamCollectionUrl;    
    $wkspc = $tfs.vcs.GetWorkspace($env:ENLISTROOT);    
    $currentBranch = $wkspc.Folders[0].ServerItem;    
    write-header "Current Branch : $currentBranch "    
        
    $itemSpec = new-object Microsoft.TeamFoundation.VersionControl.Client.ItemSpec -ArgumentList @( $currentBranch, [Microsoft.TeamFoundation.VersionControl.Client.RecursionType]::None )
    $history = $tfs.vcs.GetBranchHistory( @($itemSpec), [Microsoft.TeamFoundation.VersionControl.Client.VersionSpec]::Latest )
    
    $history | foreach-object -process {
        $_ | foreach-object -process {
                $_.Children | ?{$_.Relative.BranchToItem.ServerItem -match $child} | foreach { 
                    $relative = $_.Relative;                 
                    $childBranch = $relative.BranchToItem.ServerItem
                                                
                    if(-not $confirm)
                    {
                        $yn = Read-Host -Prompt "Are you sure you want to integrate the following? `r`n$childBranch -> $currentBranch `r`n(y or n)"
                        if($yn -match 'n')
                        {
                            return;
                        }                
                    }
                    
                    write-header -message "Getting Latest on $currentBranch ..."; 
                    tf get /recursive *.*;
                    
                    write-header -message "Performing Reverse Integration from $childBranch to $currentBranch ..."; 
                    tf merge /recursive /noprompt $childBranch $currentBranch;
                    
                    write-header -message "Resolving version files by taking parent version number..."; 
                    tf resolve AssemblyInfo.* /auto:KeepYours /recursive
                    tf resolve Version.cs /auto:KeepYours /recursive
                    
                }
            }
        }
    
    
}
Set-Alias ri reverse-integrate -Force -Scope Global -Option AllScope -Description "Construct build environment alias"

<#
.SYNOPSIS
Executes TCM testcase for the test dll passed in.
#>
function import-testcases
{
    param
    (
    [parameter(Mandatory=$true,
        Position=0,
        HelpMessage="The Test Container name, which is the Test Dll.")]
        [alias("TC")]
		[string]$testContainer = $(throw 'A test dll is required!'),
	[parameter(Mandatory=$true,
        Position=1,
        HelpMessage="The team project to import tests to.")]
        [alias("TP")]
	    [string]$teamProject = $(throw 'A team project is required.'),
    [parameter(Mandatory=$false,
        Position=2,
        HelpMessage="The TFS team collection Url. Optional.")]
        [alias("TFS")]
	    [string]$teamCollectionUrl = $env:TFSSERVERURL
    )
    
    write-header -message " Adding/Updating test cases in $testContainer for $teamProject..."
    $tcm_cmd = "tcm testcase /collection:$teamCollectionUrl /teamproject:$teamProject /import /storage:$testContainer";
    
    Invoke-Expression $tcm_cmd    
}
set-alias itc import-testcases -Force -Scope Global -Option AllScope -Description "Construct Import Test Cases Alias"

function scorch-enlistment { tfpt scorch /recursive }
set-alias scorch scorch-enlistment -Force -Scope Global -Option AllScope -Description "Construct PowerTools Scorch all alias"

<#
.SYNOPSIS
Creates a TFS workspace on the TFS Server passed in using the name and path mapping denoted.
#>
function create-wkspc
{
    param(
    [psobject] $tfs = $(throw 'The parameter tfs is required'),
    [string] $wkspcName = $(throw 'The parameter wkspcName is required'),
    [string] $localPath = $(throw 'The parameter localPath is required'),
    [string] $serverPath = $(throw 'The parameter serverPath is required')
    )
    # load the required dll
    [void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.TeamFoundation.Client")

    if($tfs -eq $null)
    {
     throw 'TFS version control server was not found.'
    }
        
    [psobject] $existing = $tfs.vcs.QueryWorkspaces($wkspcName, '', '')

    if($existing -ne $null)
    {
        #Write-Host -ForegroundColor Red 'A workspace with the following name already exists -> $wkspcName'
        throw "A workspace with name '$wkspcName' already exists."
    }
    
    write-message ""
    write-message "- Creating a TFS Workspace named '$wkspcName'..."
    
    $wkspc = $tfs.vcs.CreateWorkspace($wkspcName)
    
    $wkspc.Map($serverPath, $localPath)
    
    write-message "-   Workspace created successfully"
    write-message ""
    
    
    return $wkspc   
}

<#
.SYNOPSIS
Gets the latest code for the workspace passed in.
#>
function get-latest-code
{
    param
    (    
	[parameter(Mandatory=$false,
        Position=0,
        HelpMessage="The TFS Workspace to get latest code in.")]
        [alias("W")]
		[psobject]$wkspc = ((get-tfs $env:TFSSERVERURL -silent).VCS.GetWorkspace($env:ENLISTROOT))
    )
    
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor White "- Getting latest code to new Workspace ..."
    
    $status = $wkspc.Get()
    
    if ($status.NumFailures -gt 0)
    {
        $status.GetFailures() | foreach-object -process {Write-Host -ForegroundColor Red $_.Message}
    }
    elseif ($status.NumWarnings -gt 0)
    {
        Write-Host -ForegroundColor Yellow "-   Source Code synced with warnings!"
    }
    else
    {
        Write-Host -ForegroundColor Green "-   Source code synced with no warnings or errors"
    }
    Write-Host -ForegroundColor White ""
        
}


<#
.SYNOPSIS
Utility function for adding the users sandbox folder to the workspace
#>
function add-user-folder-mapping
{
    $tfs = get-tfs $env:TFSSERVERURL -silent
    $wkspc = $tfs.vcs.GetWorkspace($env:ENLISTROOT)
    $project = $wkspc.GetTeamProjectForLocalPath($env:ENLISTROOT)
    
    $userserverpath = $project.ServerItem + "/Users/$env:USERNAME"
    $userlocalpath = "$env:ENLISTROOT\users\$env:USERNAME"
   
    $userMap = $wkspc.Folders | where{$_.LocalItem -eq $userlocalpath}
    
    if($userMap -eq $null)
    {
        write-header -message "Adding workspace mapping $userserverpath -> $userlocalpath"
        $wkspc.Map($userserverpath, $userlocalpath)
    }
}

function get-activebugs
{
	param(
		[parameter(Mandatory=$true, Position=0, HelpMessage="The Name of the Team Project")]
		[alias("TP")]
		[string] $projectName
	)
	$tfs = get-tfs $env:TFSSERVERURL -silent
	$project = $tfs.wit.Projects | ?{ $_.Name -eq $projectName}
	#todo - replace with text for query or file read -- this is deprecated
	$query = $project.StoredQueries | ?{ $_.Name -eq 'Active Bugs' }
	$queryText = $query.QueryText.Replace("@project","'$projectName'")
	
	$results = $tfs.wit.Query($queryText) 
	
	$results | foreach{
		Write-Host "========"
		write-host $_.Id
		Write-Host $_.Uri
		Write-Host $_.Title
		Write-Host $_.CreatedBy
		#$_.Fields | foreach{
		#	if($_.Value -ne ''){
		#	write-host $_.Name
		#	write-host $_.Value
		#	}
		#	
		#	}
		Write-Host "========"
		
	}
	return $results
}
<#
.SYNOPSIS
Gets a TFS Server based on the server name url passed in.
#>
function get-tfs
{
    param(
	[parameter(Mandatory=$false,
        Position=0,
        HelpMessage="The TFS Server Url containing the Team Project Collection, defaults to the TFSSERVERURL env var.")]
        [alias("TFS")]
    	[string] $serverName = $env:TFSSERVERURL,
    [parameter(Mandatory=$false,
        Position=1,
        HelpMessage="A switch used to silence console output.")]
        [switch] $silent
    )
    
    # load the required dll
    [void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.TeamFoundation.Client")

    $propertiesToAdd = (
        ('VCS', 'Microsoft.TeamFoundation.VersionControl.Client', 'Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer'),
        ('WIT', 'Microsoft.TeamFoundation.WorkItemTracking.Client', 'Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemStore'),
        ('CSS', 'Microsoft.TeamFoundation', 'Microsoft.TeamFoundation.Server.ICommonStructureService'),
        ('GSS', 'Microsoft.TeamFoundation', 'Microsoft.TeamFoundation.Server.IGroupSecurityService'),
		('TST', 'Microsoft.TeamFoundation.TestManagement.Client', 'Microsoft.TeamFoundation.TestManagement.Client.ITestManagementService'),
		('LAB', 'Microsoft.TeamFoundation.Lab.Client', 'Microsoft.TeamFoundation.Lab.Client.LabService'),
		('LFS', 'Microsoft.TeamFoundation.Lab.Client', 'Microsoft.TeamFoundation.Lab.Client.LabFrameworkService'),
		('LAS', 'Microsoft.TeamFoundation.Lab.Client', 'Microsoft.TeamFoundation.Lab.Client.LabAdminService')
    )
    
    if(-not $silent)
    {
        write-header -message "Connecting to TFS Server $serverName..."
    }
    
    # fetch the TFS instance, but add some useful properties to make life easier
    # Make sure to "promote" it to a psobject now to make later modification easier
    [psobject] $tfs = [Microsoft.TeamFoundation.Client.TeamFoundationServerFactory]::GetServer($serverName)
    foreach ($entry in $propertiesToAdd) {
        $scriptBlock = '
            [System.Reflection.Assembly]::LoadWithPartialName("{0}") > $null
            $this.GetService([{1}])
        ' -f $entry[1],$entry[2]
        $tfs | add-member scriptproperty $entry[0] $ExecutionContext.InvokeCommand.NewScriptBlock($scriptBlock)
    }
    return $tfs
}

<#
.SYNOPSIS
Finds environments matching the filter indicated and creates a snapshot with the 
name and desc indicated.  Optionally the user can denote a different Team Project.
.PARAMETER envfilter
A string allowing the user to specify a Lab Environment filter
.PARAMETER snapshotName
A string containing the desired snapshot name
.PARAMETER snapshotDesc
A string containing the desired snapshot desc
.PARAMETER teamProject
The team project to query environments for
#>
function create-testenv-snapshot
{
	param
	(
	[parameter(Mandatory=$true,
        Position=0,
        HelpMessage="The filter to use when querying for test environments based on Name.")]
        [alias("EF")]
		[string]$envfilter = $(throw 'An environment name filter is required'),
	[parameter(Mandatory=$true,
        Position=1,
        HelpMessage="The name to use when snapshoting the environment.")]
        [alias("S")]
		[string]$snapshotName = $(throw 'A snapshot name is required'),
	[parameter(Mandatory=$true,
        Position=2,
        HelpMessage="The description to use when snapshoting the environment.")]
        [alias("S")]
		[string]$snapshotDesc = $(throw 'A snapshot description is required'),
	[parameter(Mandatory=$true,
        Position=3,
        HelpMessage="The team project to use when querying for test environments.")]
        [alias("TP")]
		[string]$teamProject
	)

	$tfs = get-tfs
	$testProject = $tfs.TST.GetTeamProject($teamProject)
	
	$testProject.TestEnvironments.Query() | ?{$_.DisplayName -match $envfilter } | foreach { 
		$testenv = $_ 
		write-header -message $testenv.DisplayName 
		$labenv = $tfs.LAB.GetLabEnvironment($testenv.LabEnvironmentUri)
		
		return $labenv.CreateLabEnvironmentSnapshot($snapshotName,$snapshotDesc)
	}
}
set-alias ctes create-test-environment-snapshot -Force -Scope Global -Option AllScope -Description "Construct build environment alias"

<#
.SYNOPSIS
Finds environments matching the filter indicated and removes all snapshots that match
the snapshot filter indicated.  Optionally the user can denote a different Team Project.
.PARAMETER envfilter 
A string allowing the user to specify a Lab Environment filter
.PARAMETER snapshotfilter 
A string allowing the user to specify a snapshot filter
.PARAMETER teamProject 
The team project to query environments for
#>
function delete-testenv-snapshots
{
	param
	(
	[parameter(Mandatory=$true,
        Position=0,
        HelpMessage="The filter to use when querying for test environments based on Name.")]
        [alias("EF")]
		[string]$envfilter = $(throw 'An environment name filter is required'),
	[parameter(Mandatory=$true,
        Position=1,
        HelpMessage="The filter to use when querying the matching test environment for snapshots by Name.")]
        [alias("S")]
		[string]$snapshotfilter = $(throw 'A snapshot name filter is required'),
	[parameter(Mandatory=$true,
        Position=2,
        HelpMessage="The team project to use when querying for test environments.")]
        [alias("TP")]
		[string]$teamProject
	)
	$tfs = get-tfs
	$testProject = $tfs.TST.GetTeamProject($teamProject)
	
	$testProject.TestEnvironments.Query() | ?{$_.DisplayName -match $envfilter } | foreach { 
		$testenv = $_ 
		write-header -message $testenv.DisplayName 
		$labenv = $tfs.LAB.GetLabEnvironment($testenv.LabEnvironmentUri)
		$labenv.QueryLabEnvironmentSnapshots() | ?{$_.Name -match $snapshotfilter}| foreach {			
			write-host -ForegroundColor Green "Removing Snapshot " $_.Name 
			$labenv.DeleteLabEnvironmentSnapshot($_.Id)
			
			$snapshot = $labenv.GetLabEnvironmentSnapshot($_.Id)
			$i = 1
        	while($snapshot -ne $null `
			      -and $snapshot.State -eq [Microsoft.TeamFoundation.Lab.Client.LabEnvironmentSnapshotState]::Deleting `
				  -and ($i -lt 300))
			{
				Write-Host -ForegroundColor Green ".`a" -NoNewline
	            sleep 1
	            $i++
				try{
				$snapshot = $labenv.GetLabEnvironmentSnapshot($_.Id)
				}
				catch {$snapshot = $null}
			}
			Write-Host ""
		}
	}
}
set-alias dtes delete-testenv-snapshots -Force -Scope Global -Option AllScope -Description "Construct build environment alias"

function restore-testenv-snapshot
{
	param
	(
	[parameter(Mandatory=$true,
        Position=0,
        HelpMessage="The filter to use when querying for test environments based on Name.")]
        [alias("EF")]
		[string]$envfilter = $(throw 'An environment name is required'),
	[parameter(Mandatory=$true,
        Position=1,
        HelpMessage="The filter to use when querying the matching test environment for snapshots by Name.")]
        [alias("S")]
		[string]$snapshotfilter = $(throw 'A snapshot name is required'),
	[parameter(Mandatory=$true,
        Position=2,
        HelpMessage="The team project to use when querying for test environments.")]
        [alias("TP")]
		[string]$teamProject
	)
	$tfs = get-tfs
	$testProject = $tfs.TST.GetTeamProject($teamProject)
	
	$testProject.TestEnvironments.Query() | ?{$_.DisplayName -eq $envfilter } | foreach { 
		$testenv = $_ 
		write-header -message $testenv.DisplayName 
		$labenv = $tfs.LAB.GetLabEnvironment($testenv.LabEnvironmentUri)
		$labenv.QueryLabEnvironmentSnapshots() | ?{$_.Name -eq $snapshotfilter}| foreach {			
			write-host -ForegroundColor Green "Restoring to Snapshot " $_.Name 
			$labenv.RestoreLabEnvironmentSnapshot($_.Id)
			break
		}
		$labenv = $tfs.LAB.GetLabEnvironment($testenv.LabEnvironmentUri)
		$i = 1
    	while($labenv -ne $null `
		      -and $labenv.StatusInfo.State -eq [Microsoft.TeamFoundation.Lab.Client.LabEnvironmentState]::RestoringSnapshot `
			  -and ($i -lt 300))
		{
			Write-Host -ForegroundColor Green ".`a" -NoNewline
            sleep 1
            $i++
			try{
				$labenv = $tfs.LAB.GetLabEnvironment($testenv.LabEnvironmentUri)
			}
			catch {$labenv = $null}
		}
		Write-Host ""
	}
	
}
set-alias rtes restore-testenv-snapshot -Force -Scope Global -Option AllScope -Description "Construct build environment alias"

<#
.SYNOPSIS
Finds network adapters for the TFS Lab host specified.
.PARAMETER filter 
A string allowing the user to specify a Lab Host filter
#>
function get-tfs-lab-host-network-adapters
{
	param
	(
	[parameter(Mandatory=$true,
        Position=0,
        HelpMessage="The SCVMM host containing the TFS Lab machines.")]
        [alias("F")]
		[string]$filter
	)
	
	
	$tfs = get-tfs -silent
	get-tfs-lab-hosts -tfs $tfs | ?{$_.Name -match $filter} | foreach {
		write-header -message "Getting the VM network adapters for " $_.FullyQualifiedDomainName
		$tfs.LFS.GetHostNetworkAdapters($_.FullyQualifiedDomainName)
	}
}


<#
.SYNOPSIS
Finds TFS Lab hosts matching the filter indicate.
Optionally the user can pass in a TFS object.
.PARAMETER filter 
A string allowing the user to specify a Lab Host filter
.PARAMETER tfs  
The TFS server object to use.
#>
function get-tfs-lab-hosts
{
	param
	(
	[parameter(Mandatory=$true,
        Position=0,
        HelpMessage="A filter to apply when iterating over team project collections.")]
        [alias("F")]
		[string]$filter,
	[parameter(Mandatory=$false,
        Position=1,
        HelpMessage="A tfs instance object to use for communication.")]
        [alias("T")]
		[psobject]$tfs = (get-tfs -silent)
			
	)
	
	write-header -message "Getting the VM hosts available for $filter..."
	
	$tfs.LAS.ListAvailableHostGroups() | ?{ $_.Path -match $filter} | foreach {
		return $tfs.LAS.ListAvailableHosts($_.Path)
	}
	
}
