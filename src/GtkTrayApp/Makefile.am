TrayAppGtk_CSFILES = $(srcdir)/TrayApp.cs $(srcdir)/SharingPage.cs $(srcdir)/FileBrowser.cs $(srcdir)/DirectoryEntry.cs $(srcdir)/PropertiesDialog.cs $(srcdir)/CollectionBrowser.cs $(srcdir)/InvitationWizard.cs $(srcdir)/NodePropertyPage.cs
TrayAppGtk_CSFILES_CSC := $(subst /,$(SEP),$(TrayAppGtk_CSFILES))
TrayAppGtk_INCLUDES = 
TrayAppGtk_RESOURCES =
TrayAppGtk_FLAGS = $(CSC_EXEFLAG)
TrayAppGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll $(SYSTEM_XML) StoreProvider.dll Identity.dll CollectionStore.dll AddressBook.dll Agent.dll iFolder.dll egg-sharp.dll BookLib.dll Sync.dll SyncCommon.dll SyncManager.dll Configuration.dll Common.dll CommonGtkWindows.dll SyncLogicLite.dll
TrayAppGtk_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin ../iFolder


InviteAppGtk_CSFILES = $(srcdir)/InviteApp.cs $(srcdir)/InvitationWizard.cs
InviteAppGtk_CSFILES_CSC := $(subst /,$(SEP),$(InviteAppGtk_CSFILES))
InviteAppGtk_INCLUDES = 
InviteAppGtk_RESOURCES =
InviteAppGtk_FLAGS = $(CSC_EXEFLAG)
InviteAppGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll $(SYSTEM_XML) Agent.dll iFolder.dll
InviteAppGtk_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin ../iFolder


CollectionBrowserGtk_CSFILES = $(srcdir)/CBApp.cs $(srcdir)/CollectionBrowser.cs $(srcdir)/NodePropertyPage.cs $(srcdir)/SharingPage.cs $(srcdir)/PropertiesDialog.cs
CollectionBrowserGtk_CSFILES_CSC := $(subst /,$(SEP),$(CollectionBrowserGtk_CSFILES))
CollectionBrowserGtk_INCLUDES = 
CollectionBrowserGtk_RESOURCES =
CollectionBrowserGtk_FLAGS = $(CSC_EXEFLAG)
CollectionBrowserGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll $(SYSTEM_XML) StoreProvider.dll Identity.dll CollectionStore.dll Configuration.dll AddressBook.dll Agent.dll iFolder.dll BookLib.dll
CollectionBrowserGtk_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin ../iFolder


DATA_FILES = ifolder.glade contact.png contact_me.png folder.png ifolder.png ifolderfolder.png invitewiz.png leafnode.png node.png ifolder.gif property.png libegg.so egg-sharp.dll

EXTRA_DIST = $(TrayAppGtk_CSFILES) $(InviteAppGtk_CSFILES) $(CollectionBrowserGtk_CSFILES) $(DATA_FILES)

all: iFolderApp.exe InvitationWizard.exe CollectionBrowser.exe

iFolderApp.exe: $(TrayAppGtk_CSFILES) $(TrayAppGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(TrayAppGtk_FLAGS) $(TrayAppGtk_LIBS:%=/r:%) $(TrayAppGtk_LIBPATH:%=/lib:%) $(TrayAppGtk_RESOURCES:%=$(ICON_FLAG)%) $(TrayAppGtk_CSFILES_CSC) /doc:iFolderApp.xml

InvitationWizard.exe: $(InviteAppGtk_CSFILES) $(InviteAppGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InviteAppGtk_FLAGS) $(InviteAppGtk_LIBS:%=/r:%) $(InviteAppGtk_LIBPATH:%=/lib:%) $(InviteAppGtk_RESOURCES:%=$(ICON_FLAG)%) $(InviteAppGtk_CSFILES_CSC) /doc:InvitationWizard.xml

CollectionBrowser.exe: $(CollectionBrowserGtk_CSFILES) $(CollectionBrowserGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionBrowserGtk_FLAGS) $(CollectionBrowserGtk_LIBS:%=/r:%) $(CollectionBrowserGtk_LIBPATH:%=/lib:%) $(CollectionBrowserGtk_RESOURCES:%=$(ICON_FLAG)%) $(CollectionBrowserGtk_CSFILES_CSC) /doc:CollectionBrowser.xml

install-exec-local: iFolderApp.exe InvitationWizard.exe CollectionBrowser.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderApp.exe InvitationWizard.exe CollectionBrowser.exe $(DATA_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderApp.exe InvitationWizard.exe CollectionBrowser.exe $(DATA_FILES)
    
installcheck-local: install
	
CLEAN_FILES = iFolderApp.exe iFolderApp.xml InvitationWizard.exe InvitationWizard.xml CollectionBrowser.exe CollectionBrowser.xml

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

