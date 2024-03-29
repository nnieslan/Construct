﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

######################################################################
#init-addons 
#  initializes any 3rd Party SharePoint Add-ons denoted by the 
#  configuration file for the current SP Farm.  This includes
#  Office Web Application if necessary.
######################################################################
param 
(
    [string]$InputFile = $(throw '- Need parameter input file (e.g. "c:\SP2010\Scripted\Inputs-Addons.xml")')
)

begin
{
    $spsnapin = get-pssnapin | where { $_.Name -eq "Microsoft.SharePoint.PowerShell" }

    if($spsnapin -eq $null)
    {
        Add-PSSnapin Microsoft.SharePoint.PowerShell -ErrorAction SilentlyContinue | Out-Null
    }

     # load the helper function dlls
    $scriptDir = Split-Path $MyInvocation.MyCommand.Path
    . (Join-Path $scriptDir office-web-app-functions.ps1)
    . (Join-Path $scriptDir sharepoint-functions.ps1) 
}

process
{    

    Write-Host -ForegroundColor Green "- Reading Input File $InputFile..."
    $xmlinput = [xml] (get-content $InputFile)
    $item = $xmlinput.SP2010ExtraConfig

    #set up Office Web Apps services and features as appropriate
    if($item.OfficeWebApps.ConfigureSvcApps -eq 1)
    {
    
        Write-Host -ForegroundColor Green "- Activating Office Web App Services..."
        activate-office-services -machines @( $env:COMPUTERNAME )
        Write-Host -ForegroundColor Green "- Creating Office Web App Service Applications and Proxies..."
        create-office-svcapps -appPoolName $item.OfficeWebApps.ServiceApplicationAppPool -appPoolUserName $item.OfficeWebApps.ServiceApplicationUserName -appPoolPassword $item.OfficeWebApps.ServiceApplicationPassword
    }
    
    if($item.OfficeWebApps.ActivateFeatures -eq 1)
    {
        Write-Host -ForegroundColor Green "- Activating Office Web App Features on all SiteCollections..."
        activate-office-feature-all-sitecollections
    }    
    

    #adding 3rd party WSPs to farm 
    Write-Host -ForegroundColor Green "- Adding Solutions to Farm"
	foreach ($wsp in $item.SolutionsToAdd.wsp)
	{
        if($wsp.OverwriteExisting -eq '1')
        {
            remove-sharepoint-solution -wsp $wsp.Name
        }
        
        add-sharepoint-solution -wsp $wsp.Path -scope $wsp.Scope
    }
    
}