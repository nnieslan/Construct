#==============================================================================
# Copyright (c) NT Prime LLC. All Rights Reserved.
#==============================================================================

#build.ps1 - wrapper methods around MSBuild invocation to make the process easier and to ensure tests execute.

set-alias build start-build -Force -Scope Global -Option AllScope -Description "An incremental msbuild alias"
set-alias rbc clean-rebuild -Force -Scope Global -Option AllScope -Description "A clean-rebuild msbuild alias"

<#
.SYNOPSIS
A wrapper function around start-build that forces the Clean and Build targets to run back to back, ensuring a clean build.
.PARAMETER config 
The config to build such as 'Debug'
.PARAMETER loglevel
The MSBuild logging level to use (Minimal, Normal, Detailed or Diag)
.PARAMETER forceCodeAnalysis
A switch to force static analysis if you need the build to run FxCop on all projects
.PARAMETER skipCodeAnalysis
A switch to skip static analysis if you want to forcibly omit FxCop on all projects
.PARAMETER skipTests
A switch to skip automated tests
.PARAMETER buildReferences
A switch to turn on implicit reference rebuild (not recommended for build speed)
.PARAMETER enableCodeCoverage
A switch to turn on code coverage during test execution
#>
function clean-rebuild 
{ 
    param(
        [parameter(Mandatory=$false,
            Position=0,
            HelpMessage="The configuration (i.e. Debug or Release) to build")]
            [alias("CFG")]
            [string] $config = 'Debug',
         [parameter(Mandatory=$false,
            Position=1,
            HelpMessage="The MSBuild logging level for the console")]
            [alias("LL")]
            [string] $logLevel = 'Minimal',
		[parameter(Mandatory=$false,
            Position=2,
            HelpMessage="A switch to force code analysis execution.")]
            [alias("FORCECA")]
            [switch] $forceCodeAnalysis,
		[parameter(Mandatory=$false,
            Position=3,
            HelpMessage="A switch to skip unit test execution.")]
            [alias("SKIPT")]
            [switch] $skipTests,
	    [parameter(Mandatory=$false,
            Position=4,
            HelpMessage="A switch to skip code analysis execution.")]
            [alias("SKIPCA")]
            [switch] $skipCodeAnalysis,
	    [parameter(Mandatory=$false,
            Position=5,
            HelpMessage="A switch to determine the build status of project references.")]
            [alias("BR")]
            [switch] $buildReferences,
	    [parameter(Mandatory=$false,
            Position=6,
            HelpMessage="A switch to determine if code coverage should execute during tests.")]
            [alias("CC")]
            [switch] $enableCodeCoverage,
	    [parameter(Mandatory=$false,
            Position=7,
            HelpMessage="A switch to determine if HP Fortify should be run.  Possible options are ASSEMBLY and BUILD.")]
            [string] $runFortify="NONE"
         )
            
    start-build -CFG $config -T 'Clean";"Build' -FORCECA:$forceCodeAnalysis -SKIPT:$skipTests -SKIPCA:$skipCodeAnalysis -BR:$buildReferences -CC:$enableCodeCoverage -LL:$logLevel -runFortify:$runFortify
}

<#
.SYNOPSIS
Invokes MSBuild and builds each of the proj files in the current folder and then invokes Test execution.
.PARAMETER config
The config to build such as 'Debug'
.PARAMETER targets
The targets to build such as 'Build', 'Clean' or 'Rebuild'
.PARAMETER loglevel
The MSBuild logging level to use (Minimal, Normal, Detailed or Diag)
.PARAMETER forceCodeAnalysis
A switch to force static analysis if you need the build to run FxCop on all projects
.PARAMETER skipCodeAnalysis
A switch to skip static analysis if you want to forcibly skip FxCop on all projects
.PARAMETER buildReferences
A switch to turn on implicit reference rebuild (not recommended for build speed)
.PARAMETER enableCodeCoverage
A switch to turn on code coverage during test execution
.PARAMETER filter
A filter string to use when building projects, omitting projects that don't match the filter RegEx
.PARAMETER skipTests
A switch to skip automated tests
#>
function start-build
{
    param(
        [parameter(Mandatory=$false,
            Position=0,
            HelpMessage="The configuration (i.e. Debug or Release) to build")]
            [alias("CFG")]
            [string] $config = 'Debug',
        [parameter(Mandatory=$false,
            Position=1,
            HelpMessage="The build target (i.e. Build or Clean or Rebuild) to execute")]
            [alias("T")]
            [string] $targets = 'Build',
        [parameter(Mandatory=$false,
            Position=2,
            HelpMessage="The MSBuild logging level for the console")]
            [alias("LL")]
            [string] $logLevel = 'Minimal',
		[parameter(Mandatory=$false,
            Position=3,
            HelpMessage="A switch to force code analysis execution.")]
            [alias("FORCECA")]
            [switch] $forceCodeAnalysis,
		[parameter(Mandatory=$false,
            Position=4,
            HelpMessage="A switch to skip code analysis execution.")]
            [alias("SKIPCA")]
            [switch] $skipCodeAnalysis,
		[parameter(Mandatory=$false,
            Position=5,
            HelpMessage="A switch to determine the build status of project references.")]
            [alias("BR")]
            [switch] $buildReferences,
		[parameter(Mandatory=$false,
			Position=6,
			HelpMessage="A filter to use when building projects, allowing the system to build only projects that match the filter using simple Regex")]
			[alias("F")]
			[string]$filter,
	    [parameter(Mandatory=$false,
		   Position=7,
		   HelpMessage="A flag to skip test execution.")]
	       [alias("SKIPT")]
	       [switch]$skipTests,
		[parameter(Mandatory=$false,
            Position=8,
            HelpMessage="A switch to determine if code coverage should execute during tests.")]
            [alias("CC")]
            [switch] $enableCodeCoverage,
	    [parameter(Mandatory=$false,
            Position=9,
            HelpMessage="A switch to determine if HP Fortify should be run.  Possible options are ASSEMBLY and BUILD.")]
            [string] $runFortify="NONE"
    )
		
    #gets the current directory and creates a bin folder path off of it for output binaries
    $currentdir = Get-Location

	if($skipCodeAnalysis -and $forceCodeAnalysis) {
		write-error "The -SkipCA and -ForceCA flags cannot be used together, please select one and re-run the command."
		return;
	}

    if($env:ENLISTROOT -ne $null)
    {
        $bindir = Join-Path -path $env:ENLISTROOT -childpath "\bin\$config\"
    }
    else
    {        
        $bindir = Join-Path -path $currentdir -childpath "\bin\$config\"
    }
	
    $buildStartTime = [System.DateTime]::Now   
    #finds project files in this folder and builds them with the settings defined.
    $projs = get-childitem * -include *.*proj 
    $projs | foreach-object -process { 
			build-project -project $_ `
				-config $config `
				-bindir $bindir `
				-targets $targets `
				-filter $filter `
				-consoleLogLevel $logLevel `
				-forceCodeAnalysis:$forceCodeAnalysis `
				-skipCodeAnalysis:$skipCodeAnalysis `
				-buildReferences:$buildReferences `
				-runFortify:$runFortify
		}

	$buildEndTime = [System.DateTime]::Now
	$elapsedBuildTime = $buildEndTime.Subtract($buildStartTime).ToString()
    write-header -message "Build Execution Time : $elapsedBuildTime"
   
    if((gci $currentdir build.$config.err).Length -eq 0)
    {
       if (-not $skipTests) {
	   	  $testSettingsFile = "$env:ENLISTROOT\local.testsettings"
		  if($enableCodeCoverage) {$testSettingsFile = "$env:ENLISTROOT\local.coverage.testsettings"}
		  
		  if((Test-Path $bindir\tests)) {
	          $testStartTime = [System.DateTime]::Now
	   		  run-tests -bindir $bindir\tests\ -config $config -testSettingsFile $testSettingsFile
	          #NOTE - Uncomment in the event silverlight tests are needed.
	          #run-silverlight-tests -bindir $bindir -config $config
			  $testEndTime = [System.DateTime]::Now
	   		  $elapsedTestTime = $testEndTime.Subtract($testStartTime).ToString()
	   		  write-header -message "Test Execution Time : $elapsedTestTime"
			  		
			  $elapsedTotalTime = $testEndTime.Subtract($buildStartTime).ToString()
	   		  write-header -message "Total Execution Time (build and test): $elapsedTotalTime"
			  }
		} else {
			write-header -message "No automated tests were found to execute"
			  
		}
	}
    else
    {
        write-error "Tests were not executed due to a build error. See build.$config.err for complete error list."        
    }
}
