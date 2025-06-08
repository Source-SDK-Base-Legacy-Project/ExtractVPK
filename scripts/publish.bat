setlocal enableextensions enabledelayedexpansion
@echo off
prompt $

echo Platform RID: %~1

set _artifacts_path=..\artifacts
set _sln_path=%cd%\..\src\ExtractVPK.sln

dotnet publish -r "%~1" -c Release --artifacts-path "%_artifacts_path%" /p:PublishSingleFile=true /p:DebugType=none /p:DebugSymbols=false --self-contained true "%_sln_path%"

endlocal