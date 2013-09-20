#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#####################################################
#enlist-tfsgitrepo.ps1
# Creates an local git repo enlistment to a TFS project in the current
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
        HelpMessage="The name of the TFS Team Project to connect to is required")]
        [alias("TP")]
        [string] $teamProject,
    [parameter(Mandatory=$true,
        Position=2,
        HelpMessage="The URL of the TFS Team Project Collection to connect to is required")]
        [alias("TFS")]
        [string] $tfsServerUrl
)

begin
{

    # load the helper function dlls
    $scriptDir = Split-Path $MyInvocation.MyCommand.Path
    . (Join-Path $scriptDir common-functions.ps1)
	
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
		Write-Host -Foreground Green "- Resetting environment variable HOME = $local...";
		set-item -path env:\HOME -value $local | out-null
	}
	
	write-header -message " Copying git-tfs binaries to enlistment..."
    set-alias robocopy $env:windir\system32\robocopy.exe
    #path to git bits
    $gittfsbits = (Join-Path -Path $scriptDir -ChildPath "git-tfs")
    if(!(Test-Path -Path $gittfsbits -PathType Container))
    {
        throw "Unable to find the Git-tfs binaries. Cancelling enlistment process."
    }
    
    robocopy $gittfsbits "$env:TEMP\git-tfs" /MIR /NS /NC /NFL /ETA /NJH
    Set-Item -path env:\PATH -value "$env:TEMP\git-tfs;$env:PATH"

    #include the related scripts necessary to perform this cmdlet task.
    . (Join-Path $scriptDir tfs-functions.ps1)
    . (Join-Path $scriptDir gittfs-functions.ps1)
    . (Join-Path $scriptDir create-shortcut.ps1)    
}
process
{
    #write-header -message " Copying Git binaries to enlistment..."
    #set-alias robocopy $env:windir\system32\robocopy.exe
    #path to git bits
    #$gitbits = (Join-Path -Path $scriptDir -ChildPath "Git")
    #$gitver = cat $gitbits\gitver.txt
    #$gitbits = (Join-Path -Path $gitbits -ChildPath $gitver)

	#write-header -message "Git binaries found at $gitbits"
    #if(!(Test-Path -Path $gitbits -PathType Container))
    #{
    #    throw "Unable to find the Git binaries. Cancelling enlistment process."
    #}
    
    #robocopy $gitbits "$local\build\git\$gitver" /MIR /NS /NC /NFL /ETA /NJH
    
    #write-header -message " Copying git-tfs binaries to enlistment..."
    #set-alias robocopy $env:windir\system32\robocopy.exe
    #path to git bits
    #$gittfsbits = (Join-Path -Path $scriptDir -ChildPath "git-tfs")
    #if(!(Test-Path -Path $gittfsbits -PathType Container))
    #{
    #    throw "Unable to find the Git-tfs binaries. Cancelling enlistment process."
    #}
	
    tfs-clone -teamProject $teamProject -tfsUrl $tfsServerUrl -enlistDir $env:ENLISTROOT -quick
	
    #create an enlistment shortcut
    ces -enlistmentDir $env:ENLISTROOT -name $name
    
}