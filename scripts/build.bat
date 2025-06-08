setlocal enableextensions enabledelayedexpansion
@echo off
prompt $

echo Version: %~1
echo Platform RID: %~2

call publish.bat %~2

set _archive_path=..\artifacts\publish\ExtractVPK\release_%~2

copy ..\LICENSE "%_archive_path%\LICENSE.txt"
copy ..\CHANGELOG.md "%_archive_path%\CHANGELOG.txt"

pushd "%_archive_path%"
zip -r "..\ExtractVPK-v%~1-%~2.zip" .
popd

endlocal