#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#git-tfs-functions.ps1

#wire up the git-tfs environment
if(Test-Path $env:ENLISTROOT\build\git-tfs)
{
    set-alias -name git-tfs -value "$env:ENLISTROOT\build\git-tfs\git-tfs.exe" -Force -Scope Global -Option AllScope -Description "git.exe Alias"
    set-content "env:PATH" "$env:ENLISTROOT\build\git-tfs;$env:PATH"
}
else
{
    get-command git-tfs | ?{$_.Name -eq 'git-tfs.exe' -and $_.CommandType -eq 'Application'} | foreach-object -process {
        set-alias -name git-tfs -value $_.Definition -Force -Scope Global -Option AllScope -Description "git-tfs.exe Alias"
    }
}

<# 
tfs-clone - Creates a local git clone of the tfs team project specified.
#>
function tfs-clone
{
	param
	(
		[string]$teamProject,
		[string]$tfsUrl,
		[string]$enlistDir,
		[string]$filter,
		[switch]$quick
	)

	$action = "quick-clone"
	if(-not $quick) { $action= "clone" }
	$cmd = "git-tfs $action $tfsurl $teamProject $enlistDir"

	write-header -message "git-tfs clone cmd : $cmd"
	Invoke-Expression $cmd;
}