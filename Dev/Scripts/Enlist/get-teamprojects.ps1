﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

param
(
    [parameter(Mandatory=$false,
        Position=0,
        HelpMessage="A TFS TeamCollection URL is required.")]
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

    Write-Host -ForegroundColor Yellow "-------------------------------------------------------------------"
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor White "Listing the available Team Projects on"
    Write-Host -ForegroundColor White "$TfsServerUrl"
    Write-Host -ForegroundColor Yellow "-------------------------------------------------------------------"
    
    #set the TFS server location
    [psobject] $tfs = get-tfs -silent -serverName $TfsServerUrl   
    
    Write-Host -ForegroundColor White "- Fetching available TFS team projects..."
    Write-Host -ForegroundColor White ""
    
    $items = $tfs.vcs.GetAllTeamProjects( 'True' )
    $items | foreach-object -process { Write-Host -ForegroundColor Green $_.Name}
    
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "-------------------------------------------------------------------"
}