#!/bin/sh -x

#This script removes iFolder from an OS X System.  It must be run as root

rm -rf /opt/novell/ifolder3
rm -rf "/Applications/iFolder 3.app"
rm -rf ~/.local/share/simias
rm -rf /Library/Receipts/iFolderClient.pkg
rm -rf /Library/Receipts/Simias.pkg

