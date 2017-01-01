@echo off
set DEFHOMEDRIVE=d:
set DEFHOMEDIR=%DEFHOMEDRIVE%%HOMEPATH%
set HOMEDIR=
set HOMEDRIVE=%CD:~0,2%

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"
echo Default homedir: %DEFHOMEDIR%

rem set /p HOMEDIR= "Enter Home directory, or <CR> for default: "

if "%HOMEDIR%" == "" (
set HOMEDIR=%DEFHOMEDIR%
)
echo %HOMEDIR%

SET _test=%HOMEDIR:~1,1%
if "%_test%" == ":" (
set HOMEDRIVE=%HOMEDIR:~0,2%
)

type SelectableDataTransmitter.version
set /p VERSION= "Enter version: "

set d=%HOMEDIR\install
if exist %d% goto one
mkdir %d%
:one
set d=%HOMEDIR%\install\Gamedata
if exist %d% goto two
mkdir %d%
:two

rmdir /s /q %HOMEDIR%\install\Gamedata\SelectableDataTransmitter

copy bin\Release\SelectableDataTransmitter.dll ..\GameData\SelectableDataTransmitter\Plugins
copy  SelectableDataTransmitter.version ..\GameData\SelectableDataTransmitter\SelectableDataTransmitter.version

xcopy /Y /E ..\GameData\SelectableDataTransmitter  %HOMEDIR%\install\Gamedata\SelectableDataTransmitter\
copy /y ..\LICENSE %HOMEDIR%\install\Gamedata\SelectableDataTransmitter

%HOMEDRIVE%
cd %HOMEDIR%\install

set FILE="%RELEASEDIR%\SelectableDataTransmitter-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% GameData\SelectableDataTransmitter 
