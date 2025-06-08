setlocal enableextensions enabledelayedexpansion
@echo off
prompt $

set _version=1.0.0-alpha.1

call build.bat %_version% win-x86
call build.bat %_version% win-x64

endlocal