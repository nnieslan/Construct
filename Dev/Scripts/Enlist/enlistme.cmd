@echo off
@echo enlistme.cmd - TFS source control enlistment script


IF '%1'=='/?' (
goto USAGE
)ELSE IF '%1'=='' (
goto USAGE
) ELSE IF '%1'=='/TEAMPROJECTS' (
goto TEAMPROJECTS
) ELSE IF '%1'=='/BRANCHES' (
goto BRANCHES
) ELSE (
goto ENLIST
)

:ENLIST
@echo Are you sure you want to create an enlistment in %cd%...?
SET /P CONFIRM=[y or n, then press ENTER]
IF '%CONFIRM%'=='y' (
@call %WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe -executionpolicy bypass -File %~dp0/enlist-workspace.ps1 -WN %1 -TPN %2 -B %3
) ELSE (
@echo NOTE - Run enlistme.cmd from the folder where you want to create the enlistment
)
goto END

:BRANCHES
@call %WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe -executionpolicy bypass -File %~dp0/get-branches.ps1 -TPN %2
goto END

:TEAMPROJECTS
@call %WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe -executionpolicy bypass -File %~dp0/get-teamprojects.ps1
goto END

:USAGE
@echo Usage: enlistme.cmd [workspaceName] [teamProjectName] [branch]
@echo -----------------------------------------------------------------------------
@echo   [workspaceName]   - The desired TFS Workspace name for your enlistment
@echo   [teamProjectName] - The TFS Team Project to connect to.
@echo                        To get the list of Team Projects, 
@echo                        run this command with the /TEAMPROJECTS flag
@echo                        EXAMPLE : enlistme.cmd /TEAMPROJECTS
@echo   [branch]          - The branch to enlist in and map to your workspace 
@echo                       (e.g. "Dev", "Main", etc.) 
@echo                       To get the list of branches for a Team Project, 
@echo                       run this command with the /BRANCHES flag
@echo                        EXAMPLE : enlistme.cmd /BRANCHES [teamProjectName] 

:END

