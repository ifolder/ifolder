@echo off

echo "--- setup"
rd /q /s tmpServerData tmpClientData
mkdir tmpServerData
mkdir tmpServerData\testFolder
mkdir tmpClientData
xcopy *.cs tmpServerData\testFolder
xcopy \*.mp3 tmpServerData\testFolder

echo "--- invite"
SyncCmd.exe -s tmpServerData invite tmpServerData\testFolder unusedUser tmpClientData\invitation
if errorlevel 1 exit /B 1

echo "--- accept"
SyncCmd.exe -s tmpClientData accept tmpClientData\invitation tmpClientData
if errorlevel 1 exit /B 1

echo "--- localsync 1"
:: "c:\Program Files\Microsoft.NET\SDK\v1.1\GuiDebug\DbgCLR.exe" "SyncCmd.exe" "-s tmpClientData localsync tmpClientData\testFolder tmpServerData"
SyncCmd.exe -s tmpClientData localsync tmpClientData\testFolder tmpServerData
if errorlevel 1 exit /B 1

c:\msys\bin\diff tmpClientData\testFolder tmpServerData\testFolder
if errorlevel 1 exit /B 1

echo "---  test deletes: delete a file from each end, and cause deletion collision
echo "---  sync and make sure everything got sorted out

@echo on
del tmpClientData\testFolder\SyncPoint.cs
del tmpClientData\testFolder\SyncOps.cs
del tmpServerData\testFolder\SyncPass.cs
del tmpServerData\testFolder\SyncOps.cs
@echo off

echo "--- localsync 2"
SyncCmd.exe -s tmpClientData localsync tmpClientData\testFolder tmpServerData
if errorlevel 1 exit /B 1

c:\msys\bin\diff tmpClientData\testFolder tmpServerData\testFolder
if errorlevel 1 exit /B 1

@echo on
if EXIST tmpClientData\testFolder\SyncPoint.cs exit /B 1
if EXIST tmpServerData\testFolder\SyncPoint.cs exit /B 2
if EXIST tmpClientData\testFolder\SyncPass.cs exit /B 3
if EXIST tmpServerData\testFolder\SyncPass.cs exit /B 4
if EXIST tmpClientData\testFolder\SyncOps.cs exit /B 5
if EXIST tmpServerData\testFolder\SyncOps.cs exit /B 6
@echo off

echo "--- succeeded"
