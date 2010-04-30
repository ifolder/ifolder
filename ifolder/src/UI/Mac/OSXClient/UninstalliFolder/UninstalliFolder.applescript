-- UninstalliFolder.applescript
-- UninstalliFolder

--  Created by Satyam on 17/05/08.
--  Copyright 2008 Novell. All rights reserved.

on clicked theObject
	--Check if Yes button is clicked
	if the name of theObject is equal to "yesButton" then
		if not iFolderIsRunning() then
			removeInstalledFiles()
		else
			-- TODO : Fix strings, Dialog type, one button
			display dialog "Uninstall failed because iFolder is running. Exit iFolder and launch the uninstaller again." buttons ["Ok"] with icon 1
		end if
		quit
	else if the name of theObject is equal to "noButton" then
		-- Uninstall cancelled by user.
		quit
	end if
end clicked

on iFolderIsRunning()
	appIsRunning("iFolder 3")
end iFolderIsRunning

on appIsRunning(appName)
	tell application "System Events" to (name of the processes) contains appName
end appIsRunning

on removeInstalledFiles()
	do shell script "rm -rf /opt/novell/ifolder3;rm -rf \"/Applications/iFolder 3.app\";rm -rf /Library/Receipts/iFolderClient.pkg;rm -rf /Library/Receipts/Simias.pkg;rm -rf /Applications/UninstalliFolder.app" with administrator privileges
end removeiFolderApplicationFiles

on removeClientDataCache()
	--Ask for confirmation to remove Data path also
	display dialog "Do you want to remove Data path also?" buttons {"No", "Yes"}
	--if user confirms then delete, else don't
	if the button returned of the result is "Yes" then
		do shell script "rm -rf ~/.local/share/simias"
	end if
end removeClientDataCache