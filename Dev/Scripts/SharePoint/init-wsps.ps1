﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#init-addons - initializes WSPs for the current SP Farm.
param 
(
    [string]$InputFile = $(throw '- Need parameter input file (e.g. "c:\SP2010\Scripted\Inputs-WSPs.xml")'),
    [string]$buildFolder = $(throw '- Need a build folder location for the built WSPs.'),
    [switch]$enableSessionState
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
    . (Join-Path $scriptDir sharepoint-functions.ps1) 
}

process
{    

    if($enableSessionState)
    {
        # Check and enable session state
        Get-SPSessionStateService
        Enable-SPSessionStateService –DefaultProvision
    }
    
    Write-Host -ForegroundColor Green "- Reading Input File $InputFile..."
    $xmlinput = [xml] (get-content $InputFile)
    $item = $xmlinput.SharePointSolutions

    #adding 3rd party WSPs to farm 
    Write-Host -ForegroundColor Green "- Adding Solutions to Farm"
	$item.wsp | foreach-object -process {
        $wsp = $_        
        if($wsp.OverwriteExisting -eq '1')
        {
            remove-sharepoint-solution -wsp $wsp.Name
        }
        
        $wspPath = join-path -Path $buildFolder -ChildPath $wsp.Name

        add-sharepoint-solution -wsp $wspPath -scope $wsp.Scope
        
        write-host -ForegroundColor Green "- Waiting for wsp installation to finish..."
        $installedWsp = get-spsolution | where{$_.Name -eq $wsp.Name}
        $i = 1
        while($installedWsp.Deployed -eq $false -and $i -lt 900) #15 min time limit
        {
            Write-Host -ForegroundColor Green ".`a" -NoNewline
            sleep 1
            $i++
		    $installedWsp = get-spsolution | where{$_.Name -eq $wsp.Name}        				
        }
        Write-Host -ForegroundColor Green ""
        
        if($installedWsp.Deployed)
        {
            Write-Host -ForegroundColor Green "- WSP Deployed..." 
            if($wsp.Feature -ne $null)
            {
            
                $wsp.Feature | foreach-object -process {
                    $featureName = $_.InnerText
                    $featureScope = $_.Scope
                    if($featureScope -eq 'Site')
                    {
                        
                        get-spsite | foreach-object -process { 
                            $site = $_
                            $url = $site.Url
                            Write-Host -ForegroundColor Green "- Enabling the $featureName feature to $url..." 
                            Enable-SPFeature -Identity $featureName -URL $url 
                        }
                    }
                    elseif($featureScope -eq 'WebApplication')
                    {
                        get-spwebapplication | foreach-object -process { 
                            $app = $_
                            $url = $app.Url
                            Write-Host -ForegroundColor Green "- Enabling the $featureName feature to $url ..." 
                            Write-Host -ForegroundColor Green ""
                            Enable-SPFeature -Identity $featureName -URL $url 
                       }
                    }
                    elseif($featureScope -eq 'Farm')
                    { 
                        get-spfeature -Farm | foreach-object -process {
                            if($_.DisplayName -eq $featureName -and $_.Status -ne 'Online')
                            { 
                                Write-Host -ForegroundColor Green "- Enabling the $featureName feature at the Farm scope..." 
                                Write-Host -ForegroundColor Green ""
                                Enable-SPFeature -Identity $_ 
                            }
                        }
                    }
                }
            }
        }
        else
        {
            write-host -ForegroundColor Red "- WSP Deployment timed out..."
        }   
    }
}