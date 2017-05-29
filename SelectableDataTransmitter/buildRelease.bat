
@echo off

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"


set VERSIONFILE=SelectableDataTransmitter.version
rem The following requires the JQ program, available here: https://stedolan.github.io/jq/download/
c:\local\jq-win64  ".VERSION.MAJOR" %VERSIONFILE% >tmpfile
set /P major=<tmpfile

c:\local\jq-win64  ".VERSION.MINOR"  %VERSIONFILE% >tmpfile
set /P minor=<tmpfile

c:\local\jq-win64  ".VERSION.PATCH"  %VERSIONFILE% >tmpfile
set /P patch=<tmpfile

c:\local\jq-win64  ".VERSION.BUILD"  %VERSIONFILE% >tmpfile
set /P build=<tmpfile
del tmpfile
set VERSION=%major%.%minor%.%patch%
if "%build%" NEQ "0"  set VERSION=%VERSION%.%build%


copy bin\Release\SelectableDataTransmitter.dll ..\GameData\SelectableDataTransmitter\Plugins
copy  SelectableDataTransmitter.version ..\GameData\SelectableDataTransmitter\SelectableDataTransmitter.version

copy /y ..\LICENSE ..\Gamedata\SelectableDataTransmitter

cd ..
set FILE="%RELEASEDIR%\SelectableDataTransmitter-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% GameData\SelectableDataTransmitter 

pause