function initialise() {
	// Tell joust where to find the various index files it needs
	index1 = 'index.htm';
	
	// Set up parameters to control menu behaviour
	theMenu.autoScrolling = true;	
	theMenu.modalFolders = false;
	theMenu.linkOnExpand = false;
	theMenu.toggleOnLink = false;
	theMenu.showAllAsLinks = false;
	theMenu.savePage = true;
	theMenu.tipText = "status";
	theMenu.selectParents = false;
	theMenu.name = "theMenu";
	theMenu.container = "self.menu";
	theMenu.reverseRef = "parent";
	theMenu.contentFrame = "text";
	theMenu.defaultTarget = "text";
	
	// Initialise all the icons
	initOutlineIcons(theMenu.imgStore);
 
}
           
self.defaultStatus = "";	
