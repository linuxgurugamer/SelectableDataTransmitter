
set H=R:\KSP_1.3.1_dev
echo %H%

set d=%H%
if exist %d% goto one
mkdir %d%
:one
set d=%H%\Gamedata
if exist %d% goto two
mkdir %d%
:two


copy /y bin\Debug\SelectableDataTransmitter.dll ..\GameData\SelectableDataTransmitter\Plugins
copy  /y SelectableDataTransmitter.version ..\GameData\SelectableDataTransmitter\SelectableDataTransmitter.version

xcopy /Y /E /I ..\GameData\SelectableDataTransmitter %H%\GameData\SelectableDataTransmitter
