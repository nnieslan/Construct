﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

param(
    [parameter(Mandatory=$true,
        Position=0,
        HelpMessage="A TFS TeamProject name to connect to is required")]
        [alias("TPN")]
        [string] $TeamProjectName,
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

    # load the helper function dlls
    $ScriptDirectory = Split-Path $MyInvocation.MyCommand.Path
	
	. (Join-Path $ScriptDirectory common-functions.ps1)
    . (Join-Path $ScriptDirectory tfs-functions.ps1)
}

process
{
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor White "Listing the available branches for '$TeamProjectName'"
    Write-Host -ForegroundColor White "$TfsServerUrl"
    Write-Host -ForegroundColor Yellow "-------------------------------------------------------------------"
    
    
    #set the TFS server location
    [psobject] $tfs = get-tfs -silent -serverName $TfsServerUrl
    
    Write-Host -ForegroundColor White "- Fetching TFS team project $TeamProjectName ..."
    
    $project = $tfs.vcs.GetTeamProject($TeamProjectName)    
    $items = $tfs.vcs.GetItems($project.ServerItem, [Microsoft.TeamFoundation.VersionControl.Client.RecursionType]::Full).Items
    
    Write-Host -ForegroundColor White "- Fetching available code branches ..."
    Write-Host -ForegroundColor White ""
    
    $items | where{$_.serveritem -match '^\$/[\w+\.?]+/(Releases/v[0-9]+|Development)/\w+$'} | foreach-object -process { Write-Host -ForegroundColor Green $_.serveritem.Replace($project.ServerItem, "").Replace("/Releases/", "").Replace("/Development/", "") }
    $items | where{$_.serveritem -match '^\$/[\w+\.?]+/Main$'} | foreach-object -process { Write-Host -ForegroundColor Green $_.serveritem.Replace($project.ServerItem+"/", "") }
    
    Write-Host -ForegroundColor Green ""
    Write-Host -ForegroundColor Yellow "-------------------------------------------------------------------"
    
}