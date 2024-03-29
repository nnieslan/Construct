﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

function add-sharepoint-feature
{
    param(
    [string]$featureName,
    [string]$scope,
    [string]$filter
    )
    
    $gc = Start-SPAssignment	
    
    $feature = $gc | get-spfeature -Identity $featureName
    if($scope -eq 'Site')
    {
        $sites = $gc | get-spsite
        if($filter -ne $null)
        {
            $sites = $sites | where {$_.Url -match $filter}
        }
        
        $sites | foreach-object -process { 
            $site = $_
            $url = $site.Url
            $exists = $gc | get-spfeature -Site $_ | ?{ $_.DisplayName -eq $featureName -and $_.Status -eq 'Online' }
            if($exists -eq $null)
            {
                Write-Host -ForegroundColor Green "- Enabling the $featureName feature to $url..." 
                Write-Host -ForegroundColor Green ""
                $gc | Enable-SPFeature -Identity $feature -URL $url 
            }
        }
    }
    if($scope -eq 'Web')
    {
        if($filter -ne $null)
        {
            Write-Host -ForegroundColor Green "- Enabling the $featureName feature to $filter..." 
            Write-Host -ForegroundColor Green ""
            $gc | Enable-SPFeature -Identity $feature -URL $filter
        }
        else
        {
           $gc | get-spsite | foreach-object -process {
                $site = $_
                $url = $site.Url
                
                write-host -ForegroundColor Green "- Getting spwebs for spsite at $url"
                $site.AllWebs | foreach-object -process {                
                    $webUrl = $_.Url
                    $exists = $gc | get-spfeature -Identity $feature -Web $_ | ?{ $_.Status -eq 'Online' }
                    if($exists -eq $null)
                    {
                        Write-Host -ForegroundColor Green "- Enabling the $featureName feature to $webUrl..." 
                        Write-Host -ForegroundColor Green ""
                        $gc | Enable-SPFeature -Identity $feature -URL $webUrl 
                    }
                }
            }
        }
    }
    elseif($scope -eq 'WebApplication')
    {        
        $apps = $gc | get-spwebapplication
        if($filter -ne $null)
        {
            $apps = $apps | where {$_.Url -match $filter}
        }
        
        $apps | foreach-object -process { 
            $app = $_
            $url = $app.Url
            $exists = get-spfeature -Identity $feature -WebApplication $app | ?{ $_.Status -eq 'Online' }
            if($exists -eq $null)
            {
                Write-Host -ForegroundColor Green "- Enabling the $featureName feature to $url ..." 
                Write-Host -ForegroundColor Green ""
                $gc | Enable-SPFeature -Identity $feature -URL $url 
            }
       }
    }
    elseif($scope -eq 'Farm')
    { 
        $gc | get-spfeature -Farm | foreach-object -process {
            if($_.DisplayName -eq $featureName -and $_.Status -ne 'Online')
            { 
                Write-Host -ForegroundColor Green "- Enabling the $featureName feature at the Farm scope..." 
                Write-Host -ForegroundColor Green ""
                $gc | Enable-SPFeature -Identity $_ 
            }
        }
    }
    
    Stop-SPAssignment -SemiGlobal $gc
}

function add-sharepoint-solution
{
   param
    (
        [string] $wsp = $(throw 'The full path to a wsp is required.'),
        [string] $scope = ("Farm"),
        [string] $name
    )
    
    $exists = Test-Path -Path $wsp -PathType Leaf
    
    if($exists -eq $false)
    {
        Write-Host -ForegroundColor Red "The wsp $wsp does not exist!"
    }
    else
    {
        Write-Host -Foreground Green "- Found WSP at $wsp"
        Write-Host -Foreground Green "- WSP scope : $scope"
        Write-Host -Foreground Green "- WSP Name  : $name"
        
        $sln = Add-SPSolution -LiteralPath $wsp 
        if($scope -eq "Farm")
        {
            $sln | Install-SPSolution -GACDeployment -Force
        }
        elseif($scope -eq "WebApplication")
        {
            $sln | Install-SPSolution -GACDeployment -AllWebApplications -Force
        }  
        
        $installedWsp = get-spsolution |?{$_.Name -eq $name}
        Write-Host -ForegroundColor Green ""   
        write-host -ForegroundColor Green "- Waiting for $installedWsp installation to finish"
        Write-Host -ForegroundColor Green ""   
        
        $i = 1
        while(($installedWsp.Deployed -eq $false) -and ($i -lt 900)) #15 min time limit
        {
            Write-Host -ForegroundColor Green ".`a" -NoNewline
            sleep 1
            $i++
		    $installedWsp = (get-spsolution |?{$_.Name -eq $name})   				
        }
        Write-Host -ForegroundColor Green ""   
        write-host -ForegroundColor Green "- installation finished for $installedWsp"
        Write-Host -ForegroundColor Green ""   
        return $installedWsp
    }
}

##############################################################
# add-sharepoint-solutions
#  Adds a list of wsps to the farm
##############################################################
function add-sharepoint-solutions
{
    param
    (
        [string[]] $wsps = $(throw 'A list of wsps to add to sharepoint is required.')
    )
    
    $wsps | foreach-object -process{
        add-sharepoint-solution -wsp $_ 
    }
}

##############################################################
# remove-sharepoint-solution
#  Forces the removal of a solution
##############################################################
function remove-sharepoint-solution
{
    param
    (
        [string] $wsp = $(throw 'The name of the wsp to remove is required.')
    )
    
    get-spsolution | where {$_.Name -eq $wsp } | remove-spsolution -Force -Confirm:$false | Out-Null
    
}

function add-sharepoint-sitecollection
{
    param(
        [string]$subPath  = $(throw "A site collection path is required."),
        [psobject]$webApplication = $(throw 'A parent web application is required'),
        [string]$contentDb = $(throw 'A content database name is required'),
        [string]$title,
        [string]$description,
        [string]$owner,
        [string]$lcid = '1033',
        [string]$template = 'STS#1'
        )
    
    $siteUrl = $webApplication.Url + $subPath
    write-host -ForegroundColor Green "- Adding SPSite at $siteUrl"        
    
    write-host -Foreground Yellow "- fetching template $template"
    $sptemplate = get-spwebtemplate | where{$_.Name -eq $template}
    
    Write-Host "- Creating Content DB..."
	New-SPContentDatabase $contentDb -WebApplication $webApplication
			
	#Creating site collection
	$gc = Start-SPAssignment
	$site = $gc | New-SPSite -Url $siteUrl -OwnerAlias $owner -ContentDatabase $contentDB -Description $description -Name $title -Language $lcid -Template $sptemplate
    Stop-SPAssignment -SemiGlobal $gc
    
    write-host -ForegroundColor Green "- Added SPSite at $siteUrl"
    return $siteUrl
}

function add-sharepoint-site
{
    param(
        [string]$subPath = $(throw "A relative URL such as '/sample/site' is required"),
        [string]$parent = $(throw "A parent is required"),
        [string]$title,
        [string]$description,
        [string]$template = 'STS#1'
    )
    
    $siteUrl = $parent + $subPath
    write-host -ForegroundColor Green "- Adding SPWeb at $siteUrl"
    
    if($template -eq $null)
    {
        write-host -Foreground Yellow "- No template provided, defaulting to a Blank Site"
        $template = get-spwebtemplate | where{$_.Name -eq 'STS#1'}
    }
    else
    {
        write-host -Foreground Yellow "- fetching template $template"
        $sptemplate = get-spwebtemplate | where{$_.Name -eq $template}
    }
    
    $gc = Start-SPAssignment
	$site = $gc | new-spweb -Url $siteUrl -Name $title -Description $description -Template $sptemplate
    Stop-SPAssignment -SemiGlobal $gc

    return $siteUrl
}

function add-sharepoint-site
{
    param(
        [string]$subPath = $(throw "A relative URL such as '/sample/site' is required"),
        [psobject]$siteCollection = $(throw "A parent site collection is required"),
        [psobject]$template
    )
    
    $siteUrl = $siteCollection.Url + $subPath
    write-host -ForegroundColor Green "- Adding SPWeb at $siteUrl"
    
    if($template -eq $null)
    {
        write-host -Foreground Yellow "- No template provided, defaulting to a Blank Site"
        $template = get-spwebtemplate | where{$_.Name -eq 'STS#1'}
    }
    
    $site = new-spweb -Url $siteUrl -Template $template
    
    $site
}

function add-domain-service-account
{

    param
    (
        [string]$UserName = $(throw 'UserName required.'),
        [string]$Password = $(throw 'Password required.'),
        [string]$ldapUri = $(throw 'An LDAP Uri such as LDAP://cn=Users,dc=domain,dc=local is required.')
    )

    $container = [ADSI] $ldapUri
    $newUser = $container.Create("User", "cn=" + $UserName)
    $newUser.Put("sAMAccountName", $UserName)
    $newUser.SetInfo()
    $newUser.psbase.InvokeSet('AccountDisabled', $false)
    $newUser.SetInfo()
    $newUser.SetPassword($Password)
}


function write-section-header
{
    param([string]$message)
    
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    Write-Host -ForegroundColor White ""
    write-host -ForegroundColor Green "$message"
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
}