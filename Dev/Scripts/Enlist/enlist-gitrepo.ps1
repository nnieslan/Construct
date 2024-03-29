﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#####################################################
#enlist-gitrepo.ps1
# Creates an enlistment to a github repo in the current
# local directory using the parameters passed in.
#####################################################
param(
    [parameter(Mandatory=$true,
        Position=0,
        HelpMessage="A name for the local enlistment to be created is required")]
        [alias("N")]
        [string] $name,
    [parameter(Mandatory=$true,
        Position=1,
        HelpMessage="The name of the GitHub Repo to connect to is required")]
        [alias("REPO")]
        [string] $gitRepoName
)

begin
{

    # load the helper function dlls
    $scriptDir = Split-Path $MyInvocation.MyCommand.Path
    #get the local path
    $local = get-location

	
	if(!(test-path env:\ENLISTROOT))
	{
		Write-Host -Foreground Green "- Adding environment variable ENLISTROOT = $local...";
		new-item -path env:. -name ENLISTROOT -value $local | out-null
	}
	else
	{
		Write-Host -Foreground Green "- Resetting environment variable ENLISTROOT = $local...";
		set-item -path env:\ENLISTROOT -value $local | out-null
	}
    
	if(!(test-path env:\HOME))
	{
		Write-Host -Foreground Green "- Adding environment variable HOME = $local...";
		new-item -path env:. -name HOME -value $local | out-null
	}
	else
	{
		Write-Host -Foreground Green "- Resetting environment variable ENLISTROOT = $local...";
		set-item -path env:\HOME -value $local | out-null
	}
    
    #include the related scripts necessary to perform this cmdlet task.
    . (Join-Path $scriptDir common-functions.ps1)
    . (Join-Path $scriptDir git-functions.ps1)
    . (Join-Path $scriptDir create-shortcut.ps1)    
}

process
{
    Write-Host -ForegroundColor Yellow "-------------------------------------------------------------------"
    Write-Host -ForegroundColor White " Creating an enlistment for '$gitRepoName' in the current directory..."
    Write-Host -ForegroundColor White " "
    Write-Host -ForegroundColor Yellow " You'll need to provide the following information to enlist so please have it ready:"
    Write-Host -ForegroundColor Yellow "    - your name"
    Write-Host -ForegroundColor Yellow "    - your github account username"
    Write-Host -ForegroundColor Yellow "    - your email address"
    Write-Host -ForegroundColor Yellow "    - your github API token (found on github.com at Account Settings > Account Admin)"
    Write-Host -ForegroundColor Yellow "-------------------------------------------------------------------"
    
    Write-Host -ForegroundColor White " "
    read-host "Press Enter to continue"
    Write-Host -ForegroundColor White " "
    
    write-header -message " Copying Git binaries to enlistment..."
    set-alias robocopy $env:windir\system32\robocopy.exe
    #path to git bits
    $gitbits = (Join-Path -Path $scriptDir -ChildPath "Git")
    $gitver = cat $gitbits\gitver.txt
    $gitbits = (Join-Path -Path $scriptDir -ChildPath $gitver)
    if(!(Test-Path -Path $gitbits -PathType Container))
    {
        throw "Unable to find the Git binaries. Cancelling enlistment process."
    }
    
    robocopy $gitbits "$local\build\git\$gitver" /MIR /NS /NC /NFL /ETA /NJH
    
    write-host ""
    write-header -message "Gathering necessary information for Git Repo creation"
    write-host ""
    #prompt for necessary information
    $userfullname = read-host "Enter your full name                      "
    $username = read-host "Enter your github username                "
    $email = read-host "Enter your github-associated email address"
    $token = read-host "Enter your github API token               "
    
    #create an SSH key for the remote repo
    add-ssh-key -email $email
    
    #set user's global information
    set-config-values -friendlyname $userfullname -username $username -email $email -githubtoken $token
    
    #init the local repo
    init-repo
    
    #create the git remote and get the code
    add-remote -name origin -reponame $gitRepoName
    
    #get latest
    pull -remote origin -branch master
    
    #create an enlistment shortcut
    ces -enlistmentDir $env:ENLISTROOT -name $name
    
    Write-Host -ForegroundColor Yellow "-------------------------------------------------------------------"
    
}
