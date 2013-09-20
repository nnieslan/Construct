function fourbuild-project
{
    param
    (
      [string] $project = $(throw 'A MSBuild project file is required'),
      [string] $bindir = $(throw 'A output bin directory for the build is required.'),
      [string] $toolsVersion = $(''),
      [string[]] $targets = $(@('Clean','Rebuild')),
      [string] $config = $('Debug'),
      [string] $platform = $('AnyCPU')
    )
    
    [void][System.Reflection.Assembly]::LoadFrom("$env:windir\Microsoft.Net\Framework64\v4.0.30319\Microsoft.Build.dll") 
    [void][System.Reflection.Assembly]::LoadFrom("$env:windir\Microsoft.Net\Framework64\v4.0.30319\Microsoft.Build.Framework.dll") 
    
    $currentDir = Get-Location
    $outdir = Join-Path -path $bindir -childpath "$config\\"
    
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor White " parameters:"
    Write-Host -ForegroundColor Green "    bindir = $bindir"
    Write-Host -ForegroundColor Green "    config = $config"
    Write-Host -ForegroundColor Green "    platform = $platform"
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    
    
    Write-Host -ForegroundColor Green "Deleting old output directory of ""$outdir"" ..."    
    # if the output folder exists, delete it
    if ([System.IO.Directory]::Exists($outdir))
    {
     [System.IO.Directory]::Delete($outdir, 1)
    }
    
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "Deleting existing log files..."
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    get-childitem * -include *.log,*.err,*.wrn | remove-item


    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "Configuring MSBuild Loggers..."
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    $consoleLogger = New-Object Microsoft.Build.Logging.ConsoleLogger -property @{Verbosity=[Microsoft.Build.Framework.LoggerVerbosity]::Minimal; ShowSummary=$true}
    $fileLogger = New-Object Microsoft.Build.Logging.FileLogger -property @{Verbosity=[Microsoft.Build.Framework.LoggerVerbosity]::Normal; ShowSummary=$true; Parameters="LogFile=$currentDir\build.$config.log"}
    $errLogger = New-Object Microsoft.Build.Logging.FileLogger -property @{ShowSummary=$false; Parameters="LogFile=$currentDir\build.$config.err;errorsonly"}
    $wrnLogger = New-Object Microsoft.Build.Logging.FileLogger -property @{ShowSummary=$false; Parameters="LogFile=$currentDir\build.$config.wrn;warningsonly"}
    
    $loggers = new-object "System.Collections.Generic.List[Microsoft.Build.Framework.ILogger]"
    $loggers.Add($consoleLogger)
    $loggers.Add($fileLogger)
    $loggers.Add($errLogger)
    $loggers.Add($wrnLogger)
    
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "Creating the MSBuild property list..."
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    $props = new-object "System.Collections.Generic.Dictionary``2[[System.String],[System.String]]"
    $props.Add("Configuration",$config)
    $props.Add("Platform",$platform)
    #$props = @{"Configuration" = $config; "Platform" = $platform }
    echo $props
    
    
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "Creating the MSBuild ProjectCollection with defaults..."
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    
    $toolsDefnLoc = [Microsoft.Build.Evaluation.ToolsetDefinitionLocations]::ConfigurationFile 
    #[Microsoft.Build.Evaluation.ToolsetDefinitionLocations]::Registry
    
    $projectCollection = new-object Microsoft.Build.Evaluation.ProjectCollection -argumentList $props,$loggers,"ConfigurationFile,Registry"
    echo $projectCollection
    
    $projectCollection.Toolsets | foreach-object -process { Write-Host -ForegroundColor Green $_.ToolsVersion; }
    
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "Creating the MSBuild BuildRequest with current properties..."
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    $request = new-object Microsoft.Build.Execution.BuildRequestData -argumentlist $project,$props,$null,$targets,$null
    echo $request
    
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "Creating the MSBuild BuildParameters with current projectCollection..."
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    $parameters = new-object Microsoft.Build.Execution.BuildParameters -argumentlist @($projectCollection)
    #$parameters.MaxNodeCount = 1
	$parameters.Loggers = $projectCollection.Loggers
	$parameters.ToolsetDefinitionLocations = "ConfigurationFile,Registry"
	$parameters.DefaultToolsVersion = $toolsVersion 
    
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "Submitting the request to the MSBuild BuildManager..."
    Write-Host -ForegroundColor White ""
    Write-Host -ForegroundColor Yellow "---------------------------------------------------------------------"
    $manager = [Microsoft.Build.Execution.BuildManager]::DefaultBuildManager
 
    $result = $manager.Build($parameters, $request)    
    
    #echo $result
    
    $loggers | foreach-object -process { $_.Shutdown(); }
}
