#=============================================================================
# Module:	TrayAppGtk Makefile
#
#=============================================================================

# configure makefile
include ../config.mk

#=============================================================================
# TrayAppGtk variables
#=============================================================================
TrayAppGtk_SOURCES = TrayApp.cs SharingPage.cs FileBrowser.cs DirectoryEntry.cs PropertiesDialog.cs CollectionBrowser.cs InvitationWizard.cs NodePropertyPage.cs
TrayAppGtk_INCLUDES = 
TrayAppGtk_RESOURCES = addressbook.glade
TrayAppGtk_FLAGS = /t:exe
TrayAppGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll $(SYSTEM_XML) StoreProvider.dll Identity.dll CollectionStore.dll AddressBook.dll Agent.dll iFolder.dll egg-sharp.dll BookLib.dll Sync.dll SyncCommon.dll SyncManager.dll Configuration.dll Common.dll CommonGtkWindows.dll SyncLogicLite.dll
TrayAppGtk_LIBPATH = ../iFolder $(SIMIAS_ROOT)/src/stage $(ADDRESSBOOK_ROOT)/src/stage  
TrayAppGtk_LDFLAGS = 

#=============================================================================
# InviteApp variables
#=============================================================================
InviteApp_SOURCES = InviteApp.cs InvitationWizard.cs
InviteApp_INCLUDES = 
InviteApp_RESOURCES = 
InviteApp_FLAGS = /t:exe
InviteApp_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll Agent.dll iFolder.dll $(SYSTEM_XML)
InviteApp_LIBPATH = $(SIMIAS_ROOT)/src/stage $(ADDRESSBOOK_ROOT)/src/stage ../iFolder  
InviteApp_LDFLAGS = 
InviteApp_STAGE_FILES = invitewiz.png

#=============================================================================
# CollectionBrowser variables
#=============================================================================
CBApp_SOURCES = CBApp.cs CollectionBrowser.cs NodePropertyPage.cs SharingPage.cs PropertiesDialog.cs 
CBApp_INCLUDES = 
CBApp_RESOURCES = 
CBApp_FLAGS = /t:exe
CBApp_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll $(SYSTEM_XML) StoreProvider.dll Identity.dll CollectionStore.dll AddressBook.dll Agent.dll iFolder.dll egg-sharp.dll BookLib.dll
CBApp_LIBPATH = $(SIMIAS_ROOT)/src/stage $(ADDRESSBOOK_ROOT)/src/stage ../iFolder  
CBApp_LDFLAGS = 
CBApp_STAGE_FILES =

#=============================================================================
# globals
#=============================================================================

#TARGETS = InvitationWizard.exe
TARGETS = InvitationWizard.exe iFolderApp.exe CollectionBrowser.exe
CLEAN_FILES = Check.xml iFolder.xml Check.xml
NUNIT_TESTS =
# for now we are including libegg.so and egg-sharp.dll until this gets
# distributed with gtk-sharp
STAGE_FILES = $(TARGETS) $(InviteApp_STAGE_FILES) $(CBApp_STAGE_FILES) ifolder.glade contact.png contact_me.png property.xpm file.xpm ifolderfolder.png folder.png ifolder.png node.png leafnode.png ifolder.gif libegg.so egg-sharp.dll
STAGE_FILES := $(addprefix $(STAGE_DIR)$(SEP), $(STAGE_FILES))

#=============================================================================
# custom makefile
#=============================================================================
ifneq "$(wildcard custom.mk)" ""
	include custom.mk
endif

#=============================================================================
# global targets
#=============================================================================

# fix path seperator
CLEAN_FILES := $(subst /,$(SEP),$(CLEAN_FILES))
STAGE_FILES := $(subst /,$(SEP),$(STAGE_FILES))

# all
all: $(TARGETS) $(STAGE_FILES)

# clean
clean:
	$(RM) $(TARGETS) $(CLEAN_FILES) $(STAGE_FILES)

# test
test:
	@echo No tests available
#	$(NUNIT) $(NUNIT_FLAGS) /xml:iFolder.xml $(NUNIT_TESTS)

# stage files
$(STAGE_FILES): $(TARGETS)
	$(CP) $(subst $(STAGE_DIR)$(SEP),,$@) $(STAGE_DIR)

#=============================================================================
# TrayAppGtk targets
#=============================================================================

# fix path seperator
TrayAppGtk_SOURCES := $(subst /,$(SEP), $(TrayAppGtk_SOURCES))
TrayAppGtk_INCLUDES := $(subst /,$(SEP), $(TrayAppGtk_INCLUDES))
TrayAppGtk_LIBPATH := $(subst /,$(SEP), $(TrayAppGtk_LIBPATH))

# update VPATH
VPATH := $(VPATH) $(TrayAppGtk_LIBPATH) $(TrayAppGtk_INCLUDES) $(InviteApp_LIBPATH) $(InviteApp_INCLUDES)

iFolderApp.exe: $(TrayAppGtk_SOURCES) $(TrayAppGtk_RESOURCE) $(TrayAppGtk_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:TrayAppGtk.xml $(CSCFLAGS) $(TrayAppGtk_FLAGS) $(TrayAppGtk_LIBS:%=/r:%) $(TrayAppGtk_LIBPATH:%=/lib:%) $(TrayAppGtk_SOURCES)

InvitationWizard.exe: $(InviteApp_SOURCES) $(InviteApp_RESOURCE) $(InviteApp_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:TrayAppGtk.xml $(CSCFLAGS) $(InviteApp_FLAGS) $(InviteApp_LIBS:%=/r:%) $(InviteApp_LIBPATH:%=/lib:%) $(InviteApp_SOURCES)

CollectionBrowser.exe: $(CBApp_SOURCES) $(CBApp_RESOURCE) $(CBApp_LIBS) ../config.mk
	$(CSC) /out:$@ /doc:TrayAppGtk.xml $(CSCFLAGS) $(CBApp_FLAGS) $(CBApp_LIBS:%=/r:%) $(CBApp_LIBPATH:%=/lib:%) $(CBApp_SOURCES)

