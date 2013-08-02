@echo off
@echo gittfs-enlistme.cmd - TFS-Git bridged source control enlistment script


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
@call %WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe -executionpolicy bypass -File %~dp0/enlist-tfsgitrepo.ps1 -N %1 -TPN %2
) ELSE (
@echo NOTE - Run gittfs-enlistme.cmd from the folder where you want to create the enlistment
)
goto END

:USAGE
@echo Usage: gittfs-enlistme.cmd [Name] [TeamProject]
@echo --------------------------------------------------------------------------------------------------------------
@echo   [Name]            - The desired local name for your enlistment
@echo   [TeamProject]     - The TFS Team Project to connect to 

:END

