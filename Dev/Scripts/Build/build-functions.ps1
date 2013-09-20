#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#build-functions.ps1 - contains the msbuild & test functions for the build environment.

write-message "- Creating aliases for msbuild, mstest, statlight"
if($env:PROCESSOR_ARCHITECTURE  -eq 'AMD64') {
	$pfx86 = get-content "env:programfiles(x86)"
} else {
	$pfx86 = get-content "env:programfiles"
}
#find MSTest.exe and create an alias to it.
#if(test-path HKLM:\SOFTWARE\Wow6432Node\Microsoft\VisualStudio\10.0)
#{
#    $vsInstallDir = (get-itemproperty -path HKLM:\SOFTWARE\Wow6432Node\Microsoft\VisualStudio\10.0).InstallDir
#} else {
#	if(test-path HKLM:\SOFTWARE\Microsoft\VisualStudio\10.0) {
#	    $vsInstallDir = (get-itemproperty -path HKLM:\SOFTWARE\Microsoft\VisualStudio\10.0).InstallDir
#	}
#}
#
#if($vsInstallDir -ne '' -and $vsInstallDir -ne $null) {
#	set-alias -Name mstest -Value "$vsInstallDir\MSTest.exe"
#}

#alias statlight based on the current enlistment
set-alias -Name statlight -Value "$env:ENLISTROOT\build\StatLight\v1.5.4260.39423\statlight.exe"
#alias msbuild.exe and sn.exe from standard locations
#set-alias -Name msbuild -Value "$env:windir\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"

#configure FXCop functionality if it's installed on the machine.
if(test-path "$pfx86\Microsoft Fxcop 10.0")
{
    set-alias -Name fxcop -Value "$pfx86\Microsoft Fxcop 10.0\fxcopcmd.exe"
    
	<#
	.SYNOPSIS
	Allows the execution of FxCOP static code analysis via manual invocation.
	Used in the case where Code Analysis isn't available as part of Visual Studio 2010.
	.PARAMETER config  
	The configuration to analyze (i.e. Debug, Release)
	.PARAMETER bindir  
	The location of the root bin directory
	.PARAMETER filter  
	An array of optional wildcard expressions used to filter analysis files (i.e. *.MyDomain.dll)
	.PARAMETER exclude 
	An array of optional wildcard expressions used to exclude binaries from analysis
	.PARAMETER dictionary 
	A code analysis dictionary to use during analysis
	#>
    function run-fxcop
    {
        param
        (
            [string] $config = 'Debug',
            [string] $bindir = '$env:ENLISTROOT\bin',
            [string[]] $filter = @("*.dll","*.exe"),
            [string[]] $exclude = @("*Test*.dll","*Test*.exe"),
			[string] $dictionary = "$env:ENLISTROOT\CustomDictionary.xml"
        )
        $outdir = Join-Path -path $bindir -childpath "$config\\"
        
        $binaries = gci -path $outdir -include $filter
        $binaries | foreach {
            $cmd = "fxcop /f:$outdir\$_ /o:$outdir\fxcop.$_.xml /summary /dictionary:$dictionary"
            write-header -message "Running FxCop 10.0 on $_ ... `r`n FxCop command: `r`n $cmd"
            invoke-expression $cmd
            
			$cmd = "fxcop /f:$outdir\$_ /o:$outdir\fxcop.$_.html /applyoutXsl /summary /dictionary:$dictionary"
            write-header -message "Running FxCop 10.0 on $_ to create readable HTML report... `r`n FxCop command: `r`n $cmd"
            invoke-expression $cmd
        }
    }
}

<#
.SYNOPSIS
Executes HP Fortify on a single .NET assembly file per the arguments passed in.
#>
function run-fortify 
{
	param(
		[string]$assembly = $(throw 'An assembly to scan is required.'),
		[string]$bindir = $(throw 'A binary folder location is required.'),
		[string]$logfile,
		[string]$results
	)
	
	if((get-command -CommandType Application | ?{$_.Name -eq 'sourceanalyzer.exe'}) -ne $null) {
		write-header "Running Fortify on $assembly ..."
		$startTime = [System.DateTime]::Now.ToShortTimeString()
		write-message "Fortify Execution Start Time : $startTime"

		if((Test-Path (Join-Path $bindir $assembly))) {
			
			$current = gci $bindir $assembly
			$fullAssemblyPath = $current.FullName
			if($logfile -eq $null -or $logfile -eq '') {$logfile = "$fullAssemblyPath.fortify.wrn"}
			if($results -eq $null -or $results -eq '') {$results = "$fullAssemblyPath.results.fpr"}
			
			write-message "Writing Fortify output to $results and logging warnings to $logfile"

			& sourceanalyzer.exe -vsversion 10.0 -quiet -scan $fullAssemblyPath -f $results  2>&1 | Out-File $logfile
		}
		
		$endTime = [System.DateTime]::Now.ToShortTimeString()
		write-message "Fortify Execution End Time : $endTime"
	}
}

<#
.SYNOPSIS
Executes HP Fortify on an entire build output folder per the arguments passed in.
#>
function run-fortify-for-build
{
	param(
		[string]$bindir = $(throw 'A binary folder location is required.'),
		[string]$libdirs,
		[string]$sessionName = $env:COMPUTERNAME,
		[string]$logFile,
		[string]$results,
        	[string]$exclusionList,
        	[string]$vmSize = "1000M"
	)
	
	if((get-command -CommandType Application | ?{$_.Name -eq 'sourceanalyzer.exe'}) -ne $null) {
		write-header "Running Fortify on the folder $bindir..."
		$startTime = [System.DateTime]::Now.ToShortTimeString()
		write-message "Fortify Execution Start Time : $startTime"

		if($sessionName -eq $null -or $sessionName -eq '') { $sessionName = "$env:COMPUTERNAME" }

		if((Test-Path ($bindir))) {
			if($logFile -eq $null -or $logFile -eq '') {
				$logFile = Join-Path $bindir "$sessionName.fortify.wrn"
			}
			
			if($results -eq $null -or $results -eq '') {
				$results = Join-Path $bindir "$sessionName.results.fpr"
			}
			
            if ($bindir.EndsWith("\") -ne $true){
                $bindir = $bindir + "\"
            }

			write-message "Writing Fortify output to $results and logging warnings to $logfile"
			write-message "Using the following exclusion masks on output DLL's: $exclusionList"			
			
			$command = "sourceanalyzer -quiet -b $sessionName -Xmx$vmSize -clean 2>&1 | Out-File $logFile -Encoding Unicode" 
			write-message $command -color:"Yellow"
			iex "$command"
			
            if ($exclusionList -ne $null) {
                $exclusions = $exclusionList.Split(";")
            }

			Get-ChildItem "$bindir*.dll", "$bindir*.exe" -Exclude $exclusions | % {

            	$startTime = [System.DateTime]::Now.ToShortTimeString()
		        write-message "$_ Model build start time : $startTime" -color:"Cyan"
				$command = "sourceanalyzer -quiet -b $sessionName -Xmx$vmSize -vsversion 10.0 -libdirs `"$bindir`;$libdirs`" $_ 2>&1 | Add-Content $logFile -Encoding Unicode"
				write-message $command -color:"Yellow"
				iex "$command"

                $endTime = [System.DateTime]::Now.ToShortTimeString()
		        write-message "$_ Model build end time : $endTime" -color:"Cyan"
			}
			
            $command = "sourceanalyzer -quiet -b $sessionName -Xmx$vmSize -show-build-warnings 2>&1 | Add-Content $logfile -Encoding Unicode"
			write-message $command -color:"Yellow"
			iex "$command"

			$command = "sourceanalyzer -quiet -b $sessionName -Xmx$vmSize -scan -f $results 2>&1 | Add-Content $logfile -Encoding Unicode"
			write-message $command -color:"Yellow"
			iex "$command"
		}
		
		$endTime = [System.DateTime]::Now.ToShortTimeString()
		write-message "Fortify Execution End Time : $endTime"
	}
}

<#
.SYNOPSIS
Executes the MSBUILD project file passed in with the 
targets and properties defined.
.PARAMETER project
The project file to process
.PARAMETER targets
The Target to execute on the project
.PARAMETER config
The configuration to assign as a property
.PARAMETER bindir
The bin location to place output under in configuration subfolder
.PARAMETER toolsVersion
The .NET tools version to use when building
.PARAMETER consoleLogLevel
The MSBuild log level to use
.PARAMETER forceCodeAnalyis
Switch allowing enforcement of Code Analysis    
.PARAMETER skipCodeAnalyis
Switch allowing forcible omission of Code Analysis    
#>
function build-project
{
    param
    (
      [string] $project = $(throw 'A MSBuild project file is required'),
      [string] $targets = $(throw 'A target(s) such as Clean or Rebuild is required'),
      [string] $config = $(throw 'A configuration to build is required'),
      [string] $bindir = $(throw 'A output bin directory for the build is required.'),
      [string] $toolsVersion = $('4.0'),
      [string] $consoleLogLevel = 'normal',
      [switch] $forceCodeAnalysis,
      [switch] $skipCodeAnalysis,
      [switch] $buildReferences,
      [switch] $binplaceIntoConfig,
      [string] $runFortify
    )
    
	$outdir = $bindir
	if($binplaceIntoConfig) { $outdir = Join-Path -path $outdir -childpath "$config\\" }
    
    # if the output folder exists and we are doing a clean, delete it
    #if ($targets -match 'Clean' -and [System.IO.Directory]::Exists($outdir))
    #{
    #    write-header -message  "Deleting old output directory of ""$outdir"" ..."    
    #    [System.IO.Directory]::Delete($outdir, 1)
    #}
	
	if($skipCodeAnalysis -and $forceCodeAnalysis) {
		write-error "The -SkipCA and -ForceCA flags cannot be used together, please select one and re-run the command."
		return;
	}
    
    #kill log files.
    write-header -message  "Deleting existing log files..."
    get-childitem * -include *.log,*.err,*.wrn | remove-item

    #from here down, we are concatenating a large set of switches to pass MSBuild.
    $fileloglevel = $consoleLogLevel;
    if($fileloglevel -eq 'minimal'){ $fileloglevel = 'normal'}

    $console_logging_switch = " /clp:Summary"";""Verbosity=$consoleLogLevel"
    $file_logging_switch = " /flp:Summary"";""Append"";""LogFile=build.$config.log"";""verbosity=$fileloglevel"
    $file_errlog_switch = " /flp1:Append"";""LogFile=build.$config.err"";""errorsonly"
    $file_wrnlog_switch = " /flp2:Append"";""LogFile=build.$config.wrn"";""warningsonly"
    $logging_switch = $console_logging_switch + $file_logging_switch + $file_errlog_switch + $file_wrnlog_switch
    $targets_switch = " /t:$targets"
    $outdir_switch = " /p:OutDir=""$outdir"""
    $config_switch = " /p:Configuration=""$config"""
	
    #optionally set some flags based on incoming switches.
    if($forceCodeAnalysis){ $config_switch = " $config_switch /p:RunCodeAnalysis=true " }
    if($skipCodeAnalysis){ $config_switch = " $config_switch /p:RunCodeAnalysis=false " }
    if(-not $buildReferences){ $config_switch = " $config_switch /p:BuildProjectReferences=false " }
    if($filter -ne $null){ $config_switch = " $config_switch /p:ProjectFilter=$filter " }
	
    if ($runFortify -ne $null -and $runFortify -ne "") {$config_switch = " $config_switch /p:RunFortifyScan=$runFortify "}
	
    #write out the built switches for debugging purposes.
    write-header -message " Logging parameters: `r`n $logging_switch"
    write-header -message " Targets parameters: `r`n $targets_switch"
    write-header -message " OutDir parameters: `r`n $outdir_switch"
    write-header -message " Configuration parameters: `r`n $config_switch"
    
    $options = $outdir_switch + $config_switch + $logging_switch + $targets_switch
    
    write-header -message " MSBuild command: `r`n $options"    
    write-header -message " Building $project ...";
    
    #build final command and execute it.
    $cmd = "msbuild $project $options"
	Invoke-Expression $cmd
}

<#
.SYNOPSIS
 Executes MSTEST for all test dlls found in the directory
 for the given configuration and bin root.
.PARAMETER trxFile
The output TRX file to report to
.PARAMETER testSettingsFile
The MSTest test settings file to use
.PARAMETER config
The configuration to test
.PARAMETER bindir
The output folder to look in
.PARAMETER filter
A wildcard filter to use for file include
#>
function run-tests
{
    param
    (
      [string] $trxFile = "local.trx",
      [string] $testSettingsFile = "$env:ENLISTROOT\local.testsettings",
      [string] $config = "Debug",
      [string] $bindir = "$env:ENLISTROOT\bin",
      [string] $filter = "*Test*",
      [switch] $binplaceIntoConfig
    )
    
    $testdir = $bindir
    if($binplaceIntoConfig) {$testdir = Join-Path -path $testdir -childpath "$config\\" }
    
    if ([System.IO.Directory]::Exists($testdir))
    {    
	write-header -message " Deleting existing test result (*.trx) files..."
	get-childitem $testdir *.trx | foreach { remove-item $_.FullName }
    }
    #build base mstest commandline 
    $trxFile = join-path -path $testdir -childpath $trxFile
    $mstest_cmd="mstest /nologo /resultsfile:""$trxFile"" /testsettings:""$testSettingsFile"" /detail:errormessage"; 
   
    #foreach test binary, append it to test commandline invocation.
    $projs = get-childitem $testdir* -include "$filter.dll" -exclude Microsoft.*
    $projs | foreach-object -process { 
    
        #omit test dlls with an accompanying xap file since they are Silverlight
        if(($_ -ne $null) -and (-not (test-path $_.FullName.Replace($_.Extension, '.xap'))))
        {
	   $fulln = $_.FullName
           write-header -message "Running tests for $fulln ..."; 
           $mstest_cmd = "$mstest_cmd /testcontainer:""$_"""
        }
   }
   Invoke-Expression $mstest_cmd   
}

<#
.SYNOPSIS
Executes STATLIGHT for all test XAPs found in the directory
for the given configuration and bin root.
.PARAMETER config
The configuration to test
.PARAMETER bindir
The output folder to look in
.PARAMETER filter 
A wildcard filter to use for file include
.PARAMETER outputFormat
The output format for statlight data (i.e. NUnit, MSGenericTest)
.PARAMETER outputFile
The output file to write to
#>
function run-silverlight-tests
{
    param
    (
      [string] $config = "Debug",
      [string] $bindir = "$env:ENLISTROOT\bin",
      [string] $filter = "*Test*.xap",
      [string] $outputFormat = 'NUnit',
      [string] $outputFile = "local.statlight",
      [switch] $binplaceIntoConfig
      )
	  
    $testdir = $bindir
    if($binplaceIntoConfig) {$testdir = Join-Path -path $testdir -childpath "$config\\" }
    
	if ([System.IO.Directory]::Exists($testdir))
    {
	    write-header -message "Deleting existing test result (*.statlight) files in $testdir..."
	    get-childitem $testdir -include *.statlight | remove-item
    }
	
    #build base mstest commandline 
    $outputFile = join-path -path $testdir -childpath $outputFile
    $statlight_cmd="statlight --ReportOutputFile=""$outputFile"" --ReportOutputFileType=$outputFormat --ShowTestingBrowserHost"; 
    $projs = get-childitem $testdir* -include $filter
    $projs | foreach-object -process { 
        if($_ -ne $null){
            write-header -message "Running tests for $_ ..."; 
            $statlight_cmd = "$statlight_cmd --XapPath=""$_"""
            }
        }
        
    if($projs -ne $null) { Invoke-Expression $statlight_cmd }
}

<#
.SYNOPSIS
Parses the build number from the LOD tag and applies it to
the AssemblyFileVersion attribute as well as injecting data
into the BuildInformation attribute on all applicable 
AssemblyInfo.cs files.
.PARAMETER updateAssemblyVersion 
A switch used to optionally increment the AssemblyVersion in addition to the AssemblyFileVersion
#>
function assign-build-number
{
	param ( [switch]$updateAssemblyVersion )
	
	if(!(Test-Path env:\LATEST_SNAPSHOT)) { return }
	
	#consts
	$lodPrefix = 'br_'
	$fileVersionPattern = 'AssemblyFileVersion'
    $assemblyVersionPattern = 'AssemblyVersion'   
    $buildInfoPattern = 'BuildInformation'
	
	#parse the build number from the latest snapshot
	$buildNumber = $env:LATEST_SNAPSHOT.replace($lodPrefix, '').replace('_', '.')
	
	#get the build time
	$time = (Get-Date).ToString()
	#build a string containing the build info
	$buildInfoValue = "(Branch = `"$env:CURRENT_LOD`", Tag = `"$env:LATEST_SNAPSHOT`", BuildTime = `"$time`")"
	
	#find all AssemblyInfo files and update them with the data from above.
    write-header -message "Updating AssemblyInfo.cs files with build number '$buildNumber'..."
    $assemblyInfos = gci -path $env:ENLISTROOT -include AssemblyInfo.cs -Recurse
    $assemblyInfos | foreach-object -process {
		
        $file = $_
        write-host -ForegroundColor Green "- Updating build number in $file"
        #kill tmp file if it exists.
		if(test-path "$file.tmp" -PathType Leaf)
        {
            remove-item "$file.tmp"
        }
		#read in the assemblyinfo file line by line
        get-content $file | foreach-object -process {
            $line = $_
            if($line -match $fileVersionPattern)
            {
                #replace the version number in the file version to match this build number.
                $line = $line -replace '[\d+\.?]+"', "$buildNumber`""
            }
			elseif ($updateAssemblyVersion -and $line -match $assemblyVersionPattern)
			{
				#replace the version number in the file version to match this build number.
                $line = $line -replace '[\d+\.?]+"', "$buildNumber`""
			}
            elseif($line -match $buildInfoPattern)
			{
				$line = $line -replace '\([(Branch|Tag|BuildTime) = ""\w+"",?]+\)', $buildInfoValue
			}
            $line | add-content "$file.tmp"

        }
        remove-item $file #remove the old version and replace with the rewritten version.
        rename-item "$file.tmp" $file -Force -Confirm:$false
   }
}

<#
.SYNOPSIS
Helper function used to delete all subfolders matching the pattern specified of the paretn folder passed in.
.PARAMETER folder
The Folder for which we'll delete subfolders
.PARAMETER subfolder
A subfolder filter to use when recursing for sub-folders to purge.
#>
function delete-subfolders
{
	param
	(
		[string]$folder,
		[string]$subfolder
	)
	
	$subs = @()
	$subs += gci $folder $subfolder -Recurse 
	$subs | foreach {	
			if($_ -ne $null)
			{
				$name = $_.FullName
				write-message "Deleting $name ..."
				$_.Delete($true)
			}
		}
}

<#
.SYNOPSIS
Helper function used to kill all obj/bin output folders from environment.
#>
function scorch-output
{
	# we target specific areas to avoid the build folders since they are vital 
	# and other items that aren't pertinant to builds
	
	#clean dev outputs
	delete-subfolders -folder $env:ENLISTROOT\Dev -subfolder bin
	delete-subfolders -folder $env:ENLISTROOT\Dev -subfolder obj
	delete-subfolders -folder $env:ENLISTROOT\Dev -subfolder TestResults
	
	#clean test outputs
	delete-subfolders -folder $env:ENLISTROOT\Test -subfolder bin
	delete-subfolders -folder $env:ENLISTROOT\Test -subfolder obj
	delete-subfolders -folder $env:ENLISTROOT\Test -subfolder TestResults
	
	#clean deployment outputs
	delete-subfolders -folder $env:ENLISTROOT\Deploy -subfolder obj
	delete-subfolders -folder $env:ENLISTROOT\Deploy -subfolder bin
	
	#clean root outputs
	if(Test-Path $env:ENLISTROOT\bin) { del $env:ENLISTROOT\bin -Recurse }
	if(Test-Path $env:ENLISTROOT\obj) { del $env:ENLISTROOT\obj -Recurse }
	if(Test-Path $env:ENLISTROOT\TestResults) { del $env:ENLISTROOT\TestResults -Recurse }
	
}
