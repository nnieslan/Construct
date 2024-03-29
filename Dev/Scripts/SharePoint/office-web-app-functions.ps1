﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

###########################################################
#activate office web apps
###########################################################
function activate-office-services
{
    param
    (
        [string[]] $machines = $(throw 'An array of computers to start the services on is required')
    )
    
    $serviceInstanceNames = @("Word Viewing Service","PowerPoint Service","Excel Calculation Services")
    
    foreach ($machine in $machines)
    {
      foreach ($serviceInstance in $serviceInstanceNames)
      {
         $serviceID = $(Get-SPServiceInstance | where {$_.TypeName -match $serviceInstance } | where {$_.Status -ne 'Online' } | where {$_.Server -match "SPServer Name="+$machine}).ID
         if($serviceID -ne $null) { Start-SPServiceInstance -Identity $serviceID }
      }
    }
}

###########################################################
#turns off the office services if they are running.
###########################################################
function deactivate-office-services
{
    param
    (
        [string[]] $machines = $(throw 'An array of computers to start the services on is required')
    )
    
    $serviceInstanceNames = @("Word Viewing Service","PowerPoint Service","Excel Calculation Services")
    
    foreach ($machine in $machines)
    {
      foreach ($serviceInstance in $serviceInstanceNames)
      {
         $serviceID = $(Get-SPServiceInstance | where {$_.TypeName -match $serviceInstance } | where {$_.Status -eq 'Online' } | where {$_.Server -match "SPServer Name="+$machine}).ID
         if($serviceID -ne $null) { Stop-SPServiceInstance -Identity $serviceID -Confirm:$false }
      }
    }
}

###########################################################
#activate the office web apps feature for an SPSite
###########################################################
function activate-office-feature
{
    param
    (
        [string] $siteUrl = $(throw 'A Site URL is required')
    )
    $webAppsFeatureId = $(Get-SPFeature -limit all | where {$_.displayname -eq "OfficeWebApps"}).ID
    $singleSiteCollection = Get-SPSite -Identity $siteUrl 
    Enable-SPFeature $webAppsFeatureId -Url $singleSiteCollection.URL
}

###########################################################
#activate the office web apps feature for all spsites
###########################################################
function activate-office-feature-all-sitecollections
{
    $webAppsFeatureId = $(Get-SPFeature -limit all | where {$_.displayname -eq "OfficeWebApps"}).ID
    Get-SPSite -limit ALL | foreach-object -process { 
        if($(Get-SPFeature -Site $_ | where {$_.ID -eq $webAppsFeatureId}) -eq $null) 
        {
            Write-Host -ForegroundColor Green "- Activating OfficeWebApps on $_"
            Enable-SPFeature $webAppsFeatureId -url $_.URL 
        }
    }
}

###########################################################
#create service applications for powerpoint and word doc viewing
###########################################################
function create-office-svcapps
{
    param
    (
        [string] $appPoolName,
        [string] $appPoolUserName,
        [string] $appPoolPassword,
        [string] $wordSvcName = ("WdView"),
        [string] $wordProxyName = ("WdProxy"),
        [string] $pptSvcName = ("PPT"),
        [string] $pptProxyName = ("PPTProxy"),        
        [string] $excelSvcName = ("Excel")
    )
    
    $existingSvcApps = get-spserviceapplication
    
    $securePw = ConvertTo-SecureString $appPoolPassword -AsPlainText -force    
    $officeServiceAppPoolAcct = New-Object System.Management.Automation.PsCredential $appPoolUserName,$securePw
    
    ## Add Managed Account for Office Web Apps Service Application Account
    $ManagedAccountOfficeWebAppsService = Get-SPManagedAccount | Where-Object {$_.UserName -eq $appPoolUserName}
	if ($ManagedAccountOfficeWebAppsService -eq $NULL) 
	{ 
	  Write-Host -ForegroundColor White "- Registering managed account" $appPoolUserName
	  New-SPManagedAccount -Credential $officeServiceAppPoolAcct | Out-Null 
	}
	else {Write-Host -ForegroundColor White "- Managed account $appPoolUserName already exists, continuing."}
		
	$appPool = Get-SPServiceApplicationPool "$appPoolName" -ea SilentlyContinue
	if ($appPool -eq $null)
	{ 
	   $appPool = New-SPServiceApplicationPool $appPoolName -account $appPoolUserName 
	   if (-not $?) { throw " - Failed to create the application pool" }
	}
    
    if($($existingSvcApps | where {$_.Name -eq $wordSvcName}) -eq $null)
    {
        New-SPWordViewingServiceApplication -Name $wordSvcName -ApplicationPool $appPool | New-SPWordViewingServiceApplicationProxy -Name $wordProxyName
    }
    
    if($($existingSvcApps | where {$_.Name -eq $pptSvcName}) -eq $null)
    {
        New-SPPowerPointServiceApplication -Name $pptSvcName -ApplicationPool $appPool | New-SPPowerPointServiceApplicationProxy -Name $pptProxyName -AddToDefaultGroup
    }
    
    if($($existingSvcApps | where {$_.Name -eq $excelSvcName}) -eq $null)
    {
        New-SPExcelServiceApplication -Name $excelSvcName -ApplicationPool $appPool
    }
}