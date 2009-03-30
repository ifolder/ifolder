-- UninstalliFolder.applescript
-- UninstalliFolder

--  Created by Satyam on 17/05/08.
--  Copyright 2008 Novell. All rights reserved.

on clicked theObject
	--Check if Yes button is clicked
	if the name of theObject is equal to "yesButton" then
		--Remove Simias
		do shell script "rm -rf /opt/novell/ifolder3;rm -rf \"/Applications/iFolder 3.app\";rm -rf /Library/Receipts/iFolderClient.pkg;rm -rf /Library/Receipts/Simias.pkg;rm -rf /Applications/UninstalliFolder.app" with administrator privileges
		--Ask for confirmation to remove Data path also
		--display dialog "Do you want to remove Data path also?" buttons {"No", "Yes"}
		--if user confirms then delete, else don't
		--if the button returned of the result is "Yes" then
		--	do shell script "rm -rf ~/.local/share/simias"
		--end if
		--Check if No button is clicked and quit 
		quit
	else if the name of theObject is equal to "noButton" then
		quit
	end if
end clicked
