rd /s /q tmpRemote
mkdir tmpRemote

\msys\bin\scp olds@mud.provo.novell.com:simias/src/Sync/tmpClientData/invitation tmpRemote

SyncCmd.exe -s tmpRemote accept tmpRemote\invitation tmpRemote

SyncCmd.exe -s tmpRemote sync tmpRemote\testFolder

