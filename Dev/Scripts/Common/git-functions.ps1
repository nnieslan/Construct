﻿#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

<#
git-functions.ps1 - A set of Git helper functions used to ease the interaction with Git 
during the following:
- Developer enlistment processes
- Developer daily use in PowerShell environment window
- Automated build/tagging in a Build Lab
#>

<#
Wire up the git environment by interrogating for a current build 
subfolder containing Git as well as the current user's PATH.
#>
if(Test-Path $env:ENLISTROOT\build\git)
{
    $gitver = 'v1.7.6'
    $gitver = (get-childitem $env:ENLISTROOT\build\git).Name

    # Add Git to the PATH and create aliases for necessary exes.
    set-alias -name git -value "$env:ENLISTROOT\build\git\$gitver\bin\git.exe" -Force -Scope Global -Option AllScope -Description "git.exe Alias"
    set-alias -name ssh-keygen -value "$env:ENLISTROOT\build\git\$gitver\bin\ssh-keygen.exe" -Force -Scope Global -Option AllScope -Description "ssh-keygen.exe Alias"
    set-content "env:PATH" "$env:ENLISTROOT\build\git\$gitver\bin;$env:ENLISTROOT\build\git\$gitver\cmd;$env:ENLISTROOT\build\git\$gitver\mingw\bin;$env:PATH"
}
else
{
    #attempt to find git on the PATH
    get-command git* | ?{$_.Name -eq 'git.exe' -and $_.CommandType -eq 'Application'} | foreach-object -process {
		#create aliases for necessary exes.
        set-alias -name git -value $_.Definition -Force -Scope Global -Option AllScope -Description "git.exe Alias"
        $gitPath = split-path($_.Definition)
        set-alias -name ssh-keygen -value "$gitPath\ssh-keygen.exe" -Force -Scope Global -Option AllScope -Description "ssh-keygen.exe Alias"
    }
}

#assigning the current Line of Developement (LOD) branch based on local git branches
$currentLod = git branch | ?{$_.startswith("*") -eq $true} #use the * to determine the active branch.
$currentLod = $currentLod.TrimStart('*').Trim()

<#
.SYNOPSIS
Sets the CURRENT_LOD environment variable for the build system environment.
.PARAMETER lod
The line of develop to use.
#>
function set-lod
{
	param ([string]$lod)
	Write-Host -Foreground Green "- Determined current LOD as $lod...";
	if(!(test-path env:\CURRENT_LOD))
	{
	    new-item -path env:. -name CURRENT_LOD -value $lod
	}
	else
	{
	    set-item -path env:\CURRENT_LOD -value $lod
	}
}
set-lod -lod $currentLod

<#
.SYNOPSIS
A wrapper function to quickly diff using the currently configured git difftool 
#>
function diff-code
{
	write-header -message "Diffing unstaged code"
    git difftool --no-prompt
}
set-alias -name diff -value diff-code -Force -Scope Global -Option AllScope -Description "git difftool alias"


<#
.SYNOPSIS
A wrapper function to quickly perform a 'git pull origin' (or some other remote spec)
.PARAMETER remote
The git remote to pull from, defaults to 'origin'
.PARAMTER branch 
The git branch to pull into, defaults to the environment variable CURRENT_LOD
#>
function get-latest-code 
{ 
    param
    (
        [string]$remote = "origin",
        [string]$branch = "$env:CURRENT_LOD"
        
    )
    write-header -message "Fetching latest changes from $remote and merging them locally into $branch ..."
    git pull $remote $branch; #consider using fetch and merge seperately to eliminate issues
    
}
set-alias pull get-latest-code -Force -Scope Global -Option AllScope -Description "github remote pull latest alias";

<#
.SYNOPSIS
Simple wrapper around 'git fetch'
#>
function fetch-branch
{
	write-header -message "Fetching latest branch metadata..."
	git fetch
}
set-alias fetch fetch-branch -Force -Scope Global -Option AllScope -Description "github remote fetch alias";

<#
.SYNOPSIS
A wrapper function to quickly perform a 'git push origin <branch>' (or some other remote and refspec)
.PARAMETER remote 
The git remote to push to, defaults to 'origin'
.PARAMETER branch 
The git branch to push from, defaults to the HEAD of the branch indicated by the environment variable CURRENT_LOD
#>
function push-remote 
{ 
    param
    (
    [string]$remote = "origin",
    [string]$refspec = "HEAD:$env:CURRENT_LOD"
    )
    write-header -message "Pushing latest changes to $remote from local $refspec..."
    git push $remote $refspec; 
}
set-alias push push-remote -Force -Scope Global -Option AllScope -Description "GitHub remote push latest alias";

<#
.SYNOPSIS
Aliasing for 'git status', simpler wrapper
#>
function get-status { git status }
set-alias status get-status -Force -Scope Global -Option AllScope -Description "GitHub status alias";

<#
.SYNOPSIS
A wrapper around 'git commit' to ensure file adds are included.
.PARAMETER a 
Indicator denoting if even unstaged items should be included. Defaults to false.
#>
function checkin-all 
{ 
    param([switch]$a)
    if(-not $a)
    {
        write-header -message "Committing changes ...";
        git commit 
    }
    else
    {
        write-header -message "Committing all changes ...";
        git commit -a 
    }
}
set-alias commit checkin-all -Force -Scope Global -Option AllScope -Description "GitHub commit all alias";
set-alias ci checkin-all -Force -Scope Global -Option AllScope -Description "GitHub commit all alias";

<#
.SYNOPSIS
A wrapper function around git init, creating an empty repo in the folder indicated.
.PARAMETER localpath 
The folder to create a repo in, defaults to the envionment variable ENLISTROOT
#>
function init-repo
{
    param( [string]$localpath = $env:ENLISTROOT )
    
    write-header -message "Initializing git repo at $localpath ..."
    git init -q $env:ENLISTROOT
}

<#
.SYNOPSIS
A wrapper function around creating an SSH key pair to use in GitHub.
.PARAMETER email 
The email address to use or key generation.
#>
function add-ssh-key
{
    param
	(
        [string]$email = $(throw 'A valid email address is required')
    )
    
    write-header -message "Initializing an SSH Key Pair ..."
    
	#attempt to find .ssh in the HOME environment dir (this would indicate an existing key)
    $sshdir = Join-Path $env:HOME "\.ssh"
    if(!(Test-Path -Path $sshdir -PathType Container))
    {
       write-host -foregroundcolor green "- Creating $sshdir..."
       md $sshdir | out-null
    }
    else
    {
		#prompt the user to see if we should use the existing key pair or create a new one.
       $yn = read-host "An existing rsa key pair exists, would you like to back it up and create a new pair (y/n - default is y)"
       if($yn -eq '' -or $yn -match 'y')
       {
	   	   #back up the existing key pair
           write-host -foregroundcolor green "- Moving existing rsa key pair to $sshdir\key_backup ..."
           md $sshdir\key_backup | out-null
           cp -Path $sshdir\id_rsa* -Destination $sshdir\key_backup | out-null
           rm -Path $sshdir\id_rsa* 
       } else {
         return #return since we use the existing key pair
       }
    }
    $sshkeyfile = "$sshdir\id_rsa"
    ssh-keygen -q -t rsa -C $email  #create a keypair based on the current user email.
    
    #launch the RSA file and notify the user they should add the key to GitHub
    $message = "You must add the new SSH public key to your github account to continue. `r`nOnce the keyfile opens, copy and paste the contents exactly into github.com at `r`nAccount Settings > SSH Public Keys > Add another public key "
    write-header -message $message
    read-host "Press enter to open the SSH key file"
    notepad "$sshkeyfile.pub"
    read-host "Press Enter once you've added the SSH public key to your github account to test the connection"

	#test the connection to git using the new ssh key pair.
    test-git-connection
}

<#
.SYNOPSIS
A wrapper function for testing connections to github.
#>
function test-git-connection
{
    write-host -foregroundcolor green "- Testing connection to github..."
    ssh -T git@github.com
}

<#
.SYNOPSIS
A wrapper function around initialization of git config --global settings for a user.
.PARAMETER username 
The github user name for the person.
.PARAMETER friendlyname 
The first and last name of the person.
.PARAMETER email 
The email address of the person.
.PARAMETER githubtoken 
The API token for the github user.
#>
function set-config-values
{
    param(
        [string]$username,
        [string]$friendlyname,
        [string]$email,
        [string]$githubtoken
        )
        
        write-header -message  "Setting user's global config values..."
        if($friendlyname -ne $null)
        {
            git config --global user.name $friendlyname
        }
        
        if($email -ne $null)
        {
            git config --global user.email $email
        }
        
        if($username -ne $null)
        {
            git config --global github.user $username
        }
        if($githubtoken -ne $null)
        {
            git config --global github.token $githubtoken
        }
}

<#
.SYNOPSIS
A wrapper function around enlisting in a remote repro on github.
.PARAMETER name
The name for the remote. Defaults to 'origin'.
.PARAMETER reponame	
The remote repo.  
#>
function add-remote
{
    param
    (
        [string]$name = "origin",
        [string]$reponame = $(throw 'A remote repo name in the format of "Owner/Repo" is required.')
    )
    
    write-header -message "Adding the github remote repro $reponame with local name of $name ..."
    git remote add $name git@github.com:$reponame.git
}

<#
.SYNOPSIS 
Aliasing for 'git add', simple wrapper
.PARAMETER file 
The filespec to add.
#>
function get-stage 
{ 
	 param
	 (
	 	[string]$file = $(throw 'A filespec must be provided.')
	 )
	 write-header -message "Staging $file"
	 git add $file
	
}
set-alias stage get-stage -Force -Scope Global -Option AllScope -Description "Git add alias (staging)";

<#
.SYNOPSIS
A function that fetches the list of branches for the current project that match the standard line of development naming convention.
.PARAMETER lodPrefix 
The LOD prefix to use for branch filtering. Defaults 'lod-'
#>
function list-lods
{
	param
	(
		[string] $lodPrefix = "lod-"
	)
	write-header -message "Listing LODs for the current project..."
	
	$lods = @()
	git branch -r | ?{$_ -match $lodPrefix } | foreach {
			$lod = $_.split("/")[1]
			$lods += $lod 
		}
	
	return $lods
}

<#
.SYNOPSIS
Checks out the line of development branch indicated and pulls it down locally to the current enlistment.
.PARAMETER lod 
The line of development to checkout.
#>
function checkout-lod
{
	param
	(
		[string]$lod = $(throw "A line of development is required")
	)
	
	$checkoutRemoteCmd = "git checkout -b $lod origin/$lod"
    $checkoutLocalCmd = "git checkout $lod"
    $gitFetchCmd = "git fetch"
    
	$islocal = ((git branch) -match $lod)
	
	if(-not $islocal)
	{
		write-header -message  "$lod isn't checked out locally. Trying remote checkout..."
		Invoke-Expression $gitFetchCmd
		Invoke-Expression $checkoutRemoteCmd
	}
	Invoke-Expression $gitFetchCmd
	Invoke-Expression $checkoutLocalCmd
	
	#set the CURRENT_LOD env var 
	set-lod -lod $lod
	#get the latest code for the branch
	get-latest-code -branch $lod
}

<#
.SYNOPSIS
Creates a new line of development branch based on an existing one.
.PARAMETER srclod 
The current LOD to copy.
.PARAMETER newlod 
The name to give the new LOD.
#>
function create-lod
{
	param
	(
	[string] $srclod = $env:CURRENT_LOD, 
	[string] $newlod
	)
    
    checkout-lod -lod $srclod
	$createLodCmd = "git push origin origin/$srclod:refs/heads/$newlod"
    
	$exists = ((git branch -r) -match $newlod)
    if($exists) {
        throw "LOD $newlod is already in github. Exiting lod creation process"
    }
    else {
        write-host -ForegroundColor Green "LOD $newlod not in github. Ready to proceed...."
    }
    write-header -message "Creating new lod $newlod...."
    Invoke-Expression $createLodCmd
}

<#
.SYNOPSIS
Deletes a line of development branch from github.
.PARAMETER lod 
The name of the LOD to delete. Due to the nature of the command, this has no default value.
#>
function delete-lod
{
	param( [string]$lod = $(throw "You must specify a LOD such as $env:CURRENT_LOD") ) 
        
    $deleteLodCmd = "git push origin :$lod"
	write-header -message "Deleting lod $lod...."
    Invoke-Expression $deleteLodCmd
}

<#
.SYNOPSIS
Reads in the tags for the current LOD and determines the latest build number.
.PARAMETER lod 
The LOD to get snapshots for.
#>
function get-latestsnapshotname
{
	param( [string]$lod = $env:CURRENT_LOD ) 
    
	write-header -message "Getting the latest snapshot name for the lod $lod...."
    $allsnapshots = @()
	$lodsnapshots = @()
	$buildNumbers = @()
	#run command to get snapshots for lod
	$allsnapshots += git tag -l "$lod*" 
    $lodsnapshots += $allsnapshots | ?{$_ -ne $null -and $_.startswith($lod) -eq $true} 
	
	$lodsnapshots | foreach {
		$buildNumbers += [int]$_.Replace($lod, '').Trim('_')
	}
	 
    if ($buildNumbers.Count -gt 0 ) {
		$orderedBuildNumbers += $buildNumbers | Sort-Object
		$latestSnapshotName = $lodsnapshots | ? { $_.EndsWith($orderedBuildNumbers[$orderedBuildNumbers.Count - 1]) -eq $true }
        return $latestSnapshotName
    }
    else {
        return ''
    }
}

<#
.SYNOPSIS
Reads in the tags for the current LOD and determines the next build number available.
.PARAMETER lod 
The LOD to use.  Defaults to CURRENT_LOD.
#>
function get-nextsnapshotname
{
	param( [string]$lod = $env:CURRENT_LOD ) 
    
	write-header -message "Getting the next available snapshot name for the lod $lod...."
    $allsnapshots = @()
	$lodsnapshots  = @()
	$buildNumbers = @()
	#run command to get snapshots for lod
	$allsnapshots += git tag -l "$lod*" 
    $lodsnapshots += $allsnapshots | ?{$_ -ne $null -and $_.startswith($lod) -eq $true} 
	$lodsnapshots | foreach {
		$buildNumbers += [int]$_.Replace($lod, '').Trim('_')
	}
	if ($buildNumbers.Count -gt 0 ) {
		$orderedBuildNumbers += $buildNumbers | Sort-Object
        [int]$latestBuildNumber = $orderedBuildNumbers[$orderedBuildNumbers.Count - 1]
		$nextSnapshotName = $lod + "_" + ($latestBuildNumber + 1)
        return $nextSnapshotName
    }
    else {
        return $lod + "_1"
    }
}

<#
.SYNOPSIS 
Creates a snapshot for the current line of development, which consists of an annotated git tag containing the commit log.
.PARAMETER lod 
The LOD to use. Defaults to CURRENT_LOD.
#>
function create-snapshot
{
	param( [string]$lod = $env:CURRENT_LOD ) 
    
	write-header -message "Creating a new snapshot for $lod..."
	
	$currentSnapshot = get-latestsnapshotname -lod $lod
	$nextSnapshot = get-nextsnapshotname -lod $lod
	
	$gitLogCmd
    #if first snapshot, need to get log history for entire lod
    
	Write-Host -ForegroundColor Green "- Getting git log output for annotated tag: $gitLogCmd"
	if( $nextSnapshot -eq "$lod_1" ) {
        $logContent = "first snapshot"
    }
    else {
        $logContent = Invoke-Expression "git log --reverse --pretty=`"  * %s`" $currentSnapshot..HEAD"
    }
	
	$relnotes = Join-Path -Path (Get-Location) -ChildPath 'RELNOTES.txt'
	$logContent | Out-File -FilePath $relnotes
	
	Write-Host -ForegroundColor Green "- Tagging $lod with $nextSnapshot..."
	#TODO - make this a signed tag
	$tagCmd = "git tag -F $relnotes $nextSnapshot"
	Write-Host -ForegroundColor Green " - Tag command : $tagCmd"
	Invoke-Expression $tagCmd
	
	Write-Host -ForegroundColor Green "- Pushing tags for $lod to remote..."
	git push --tags
	
	if(!(test-path env:\LATEST_SNAPSHOT))
	{
	    Write-Host -Foreground Green "- Adding environment variable LATEST_SNAPSHOT = $nextSnapshot...";
	    new-item -path env:. -name LATEST_SNAPSHOT -value $nextSnapshot
	}
	else
	{
	    Write-Host -Foreground Green "- Resetting environment variable LATEST_SNAPSHOT = $nextSnapshot...";
	    set-item -path env:\LATEST_SNAPSHOT -value $nextSnapshot
	}

	Remove-Item $relnotes -Force
}

<#
.SYNOPSIS
Fetches a user OAuth token based on basic authentication of user/password
.PARAMETER user
The user name to log on with
.PARAMETER password
The password to log on with
#>
function Get-GitHubOAuthToken {
	param(
		[string] $user = $(throw 'A GitHub user id is required.'),
		[string] $password = $(throw 'A GitHub password is required.')
	)
	$json = '{"scopes":["repo","user","gist"]}'
	$creds = $user + ":" + $password
	$encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($creds))
	$authorization = "Basic $encodedCreds"
	$jsonResult = Execute-HttpCommand -target 'https://api.github.com/authorizations' -verb 'POST' -content $json -authHeader $authorization
		
	if($jsonResult -match "`"token`":`"(\w+)`"") {
		$token = $matches[1]
		return $token
	}
	
	return ''
}

<#
.SYNOPSIS
Gets a list of Repos for the user.
.PARAMETER token
The OAuth token for the user.
#>
function Get-GitHubRepos{
	param(
		[string]$token
	)
	
	$authorization = "token " + $token
	$jsonResult = Execute-HttpCommand -target "https://api.github.com/user/repos?access_token=$token" -verb 'GET' -authHeader $authorization
	return $jsonResult
}

<#
.SYNOPSIS
Gets the Issues for the owner/repo combination passed in.
.PARAMETER token
The user's OAuth token
.PARAMETER repo
The repo to query
.PARAMETER owner
The owner of the repo
#>
function Get-GitHubIssues{
	param(
		[string]$token,
		[string]$repo,
		[string]$owner
	)
	
	$authorization = "token " + $token
	$jsonResult = Execute-HttpCommand -target "https://api.github.com/repos/$owner/$repo/issues?access_token=$token" -verb 'GET' -authHeader $authorization
	return $jsonResult
		
}

<#
.SYNOPSIS
Updates an issue in GitHub with the values specified.
#>
function Set-GitHubIssue{
	param(
		[string]$token = $(throw "An OAuth access token is required"),
		[string]$repo = $(throw "A repo name is required"),
		[string]$owner = $(throw "A repo owner is required"),
		[string]$id = $(throw "An ID for the issue to update is required."),
		[string]$title,
		[string]$body,
		[string]$assignee,
		[string]$milestone,
		[string]$state,
		[string[]]$labels
	)
	$json  = "{"
	if($title -ne $null -and $title -ne '') {
	
		$json += "`"title`":`"$title`""
	}
	if($body -ne $null -and $body -ne '') {
		if($json.length -gt 1) { $json += "," }
		$json += "`"body`":`"$body`""
	}
	if($milestone -ne $null -and $milestone -ne '') {
		if($json.length -gt 1) { $json += "," }
		$json += "`"milestone`":$milestone"
	}
	if($state -ne $null -and $state -ne '') {
		if($json.length -gt 1) { $json += "," }
		$json += "`"state`":`"$state`""
	}
	if($assignee -ne $null -and $assignee -ne '') {
		if($json.length -gt 1) { $json += "," }
		$json += "`"assignee`":`"$assignee`""
	}
	if($labels -ne $null -and $labels.length -gt 0) {
		if($json.length -gt 1) { $json += "," }
		$json += ",`"labels`": ["
		$count = 0
		$labels | foreach { 
			if($count > 0){ $json += ","  }
			$json += "`"$_`"";
			$count++;
		}
		$json += "]"
	}
	$json += "}"
	
	$authorization = "token " + $token
	$targetUrl = "https://api.github.com/repos/$owner/$repo/issues/$id" + "?access_token=$token"
	$jsonResult = Execute-HttpCommand -target $targetUrl -verb 'PATCH' -authHeader $authorization -content $json
	return $jsonResult

}

<#
.SYNOPSIS
Creates an issue in GitHub with the values specified.
#>
function New-GitHubIssue{
	param(
		[string]$token = $(throw "An OAuth access token is required"),
		[string]$repo = $(throw "A repo name is required"),
		[string]$owner = $(throw "A repo owner is required"),
		[string]$title = $(throw "An issue title is required"),
		[string]$body,
		[string]$assignee,
		[string]$milestone,
		[string[]]$labels
	)
	
	$json = "{ `"title`":`"$title`""
	if($body -ne $null -and $body -ne '') {
		$json += ",`"body`":`"$body`""
	}
	if($milestone -ne $null -and $milestone -ne '') {
		$json += ",`"milestone`":$milestone"
	}
	if($labels -ne $null -and $labels.length -gt 0) {
		$json += ",`"labels`": ["
		$count = 0
		$labels | foreach { 
			if($count > 0){
				$json += ","	
			}
			$json += "`"$_`"";
			
			$count++;
		}
		$json += "]"
	}
	$json += "}"
	
	$authorization = "token " + $token
	$targetUrl = "https://api.github.com/repos/$owner/$repo/issues?access_token=$token"
	$jsonResult = Execute-HttpCommand -target $targetUrl -verb 'POST' -authHeader $authorization -content $json
	return $jsonResult
	
}