@echo off
@echo git-enlistme.cmd - GitHub source control enlistment script


IF '%1'=='/?' (
goto USAGE
)ELSE IF '%1'=='' (
goto USAGE
) ELSE (
goto ENLIST
)

:ENLIST
@echo Are you sure you want to create an enlistment in %cd%...?
SET /P CONFIRM=[y or n, then press ENTER]
IF '%CONFIRM%'=='y' (
@call %WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe -executionpolicy bypass -File %~dp0/enlist-gitrepo.ps1 -N %1 -REPO %2
) ELSE (
@echo NOTE - Run enlistme.cmd from the folder where you want to create the enlistment
)
goto END

:USAGE
@echo Usage: git-enlistme.cmd [Name] [GitHubRepoName]
@echo --------------------------------------------------------------------------------------------------------------
@echo   [Name]            - The desired local name for your enlistment
@echo   [GitHubRepoName]  - The GitHub Repro to connect to (e.g. "CanoeVentures/alph_asset_crtr")


:END

