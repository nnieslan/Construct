﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#####################################################
#enlist-workspace.ps1
# Creates an enlistment TFS workspace in the current
# local directory using the parameters passed in.
#####################################################
param(
    [parameter(Mandatory=$true,
        Position=0,
        HelpMessage="A name for the TFS Workspace to be created is required")]
        [alias("WN")]
        [string] $workspaceName,
    [parameter(Mandatory=$true,
        Position=1,
        HelpMessage="The name of the TFS TeamProject to connect to is required")]
        [alias("TPN")]
        [string] $teamProjectName,
    [parameter(Mandatory=$true,
        Position=2,
        HelpMessage="The name of the TFS branch to map to is required")]
        [alias("B")]
        [string] $branch,
     [parameter(Mandatory=$false,
        Position=0,
        HelpMessage="A TFS TeamCollection URL is required")]
        [alias("TFS")]
        [string] $TfsServerUrl
)

begin
{
    # load the required dll
    [void][System.Reflection.Assembly]::LoadWithPartialName("Microsoft.TeamFoundation.Client")

    $ScriptDirectory = Split-Path $MyInvocation.MyCommand.Path    
	
    #include the related scripts necessary to perform this cmdlet task.
    . (Join-Path $ScriptDirectory common-functions.ps1)
    . (Join-Path $ScriptDirectory tfs-functions.ps1)
    . (Join-Path $ScriptDirectory create-shortcut.ps1)    
}

process
{
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor White "Creating an enlistment for branch $branch of '$teamProjectName' in the current directory..."
    Write-Host -ForegroundColor White "$TfsServerUrl"
    Write-Host -ForegroundColor Yellow "-------------------------------------------------------------------"
    
    #set the TFS server url
    [psobject] $tfs = get-tfs -serverName $TfsServerUrl
    
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor White "- Using the following local and server paths for mapping:"
    #get the local path
    $local = get-location
    Write-Host -ForegroundColor Green  "-   Local path : $local"
    
    $branchRegex = '(^\$/[\w+\.?]+/(Releases/v[0-9]+|Development)/'+$branch+'$|^\$/[\w+\.?]+/'+$branch+'$)';
    #get the server branch path
    $server = $tfs.vcs.GetItems('$/' + $teamProjectName + '/*', [Microsoft.TeamFoundation.VersionControl.Client.RecursionType]::Full).Items | where {$_.serveritem -match $branchRegex }
    
    $serverPath = $server.ServerItem
    Write-Host -ForegroundColor Green "-   Server path : $serverPath"
    Write-Host -ForegroundColor White ""
    
    #create the workspace and get the code
    $wkspc = create-wkspc -tfs $tfs -wkspcName "$env:USERNAME-$env:COMPUTERNAME-$workspaceName" -localPath $local.Path -serverPath $server.serveritem
    
    #get latest
    get-latest-code -wkspc $wkspc
    
    #create an enlistment shortcut
    ces -enlistmentDir $local.Path -name $workspaceName
    
    Write-Host -ForegroundColor Yellow "-------------------------------------------------------------------"
    
}
