# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

TrayAppGtk_CSFILES = $(srcdir)/TrayApp.cs $(srcdir)/SharingPage.cs $(srcdir)/FileBrowser.cs $(srcdir)/DirectoryEntry.cs $(srcdir)/PropertiesDialog.cs $(srcdir)/CollectionBrowser.cs $(srcdir)/InvitationWizard.cs $(srcdir)/NodePropertyPage.cs $(srcdir)/IntroDialog.cs $(srcdir)/SettingsPage.cs $(srcdir)/CrashReport.cs
TrayAppGtk_CSFILES_CSC := $(subst /,$(SEP),$(TrayAppGtk_CSFILES))
TrayAppGtk_INCLUDES = 
TrayAppGtk_RESOURCES =
TrayAppGtk_FLAGS = $(CSC_EXEFLAG)
TrayAppGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll $(SYSTEM_XML) Common.dll StoreProvider.dll CollectionStore.dll AddressBook.dll iFolder.dll TrayIcon.dll CBLib.dll Sync.dll SyncCommon.dll Invitation.dll SyncManager.dll Common.dll CommonGtkWindows.dll SyncLogicLite.dll Simias.Service.dll
TrayAppGtk_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin $(srcdir)/../TrayIcon $(ADDRESSBOOK_INSTALL_PATH)/bin ../iFolder


InviteAppGtk_CSFILES = $(srcdir)/InviteApp.cs $(srcdir)/InvitationWizard.cs
InviteAppGtk_CSFILES_CSC := $(subst /,$(SEP),$(InviteAppGtk_CSFILES))
InviteAppGtk_INCLUDES = 
InviteAppGtk_RESOURCES =
InviteAppGtk_FLAGS = $(CSC_EXEFLAG)
InviteAppGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll $(SYSTEM_XML) Common.dll SyncCommon.dll Invitation.dll iFolder.dll
InviteAppGtk_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin $(ADDRESSBOOK_INSTALL_PATH)/bin ../iFolder


CollectionBrowserGtk_CSFILES = $(srcdir)/CBApp.cs $(srcdir)/CollectionBrowser.cs $(srcdir)/NodePropertyPage.cs $(srcdir)/SharingPage.cs $(srcdir)/PropertiesDialog.cs $(srcdir)/SettingsPage.cs
CollectionBrowserGtk_CSFILES_CSC := $(subst /,$(SEP),$(CollectionBrowserGtk_CSFILES))
CollectionBrowserGtk_INCLUDES = 
CollectionBrowserGtk_RESOURCES =
CollectionBrowserGtk_FLAGS = $(CSC_EXEFLAG)
CollectionBrowserGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gtk-sharp.dll gnome-sharp.dll glib-sharp.dll glade-sharp.dll gdk-sharp.dll $(SYSTEM_XML) Common.dll StoreProvider.dll CollectionStore.dll AddressBook.dll SyncCommon.dll iFolder.dll CBLib.dll
CollectionBrowserGtk_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin $(ADDRESSBOOK_INSTALL_PATH)/bin ../iFolder

INSTALL_FILES = $(srcdir)/invitationwizard $(srcdir)/collectionbrowser $(srcdir)/ifolder $(srcdir)/setup-autostart

DATA_FILES = $(srcdir)/ifolder.glade $(srcdir)/collection-browser.glade $(srcdir)/ifolder-confirmation.glade $(srcdir)/properties-dialog.glade $(srcdir)/crash-report.glade  $(srcdir)/collection.png $(srcdir)/Node.png $(srcdir)/Tombstone.png $(srcdir)/BaseContact.png $(srcdir)/folder.png $(srcdir)/ifolder.png $(srcdir)/ifolderfolder.png $(srcdir)/invitewiz.png $(srcdir)/file.xpm $(srcdir)/ifolder.gif $(srcdir)/property.png $(srcdir)/nuke.png

EXTRA_DIST = $(TrayAppGtk_CSFILES) $(InviteAppGtk_CSFILES) $(CollectionBrowserGtk_CSFILES) $(DATA_FILES) $(INSTALL_FILES)

all: iFolderApp.exe InvitationWizard.exe CollectionBrowser.exe

iFolderApp.exe: $(TrayAppGtk_CSFILES) $(TrayAppGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(TrayAppGtk_FLAGS) $(TrayAppGtk_LIBS:%=/r:%) $(TrayAppGtk_LIBPATH:%=/lib:%) $(TrayAppGtk_RESOURCES:%=$(ICON_FLAG)%) $(TrayAppGtk_CSFILES_CSC) /doc:iFolderApp.doc.xml

InvitationWizard.exe: $(InviteAppGtk_CSFILES) $(InviteAppGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InviteAppGtk_FLAGS) $(InviteAppGtk_LIBS:%=/r:%) $(InviteAppGtk_LIBPATH:%=/lib:%) $(InviteAppGtk_RESOURCES:%=$(ICON_FLAG)%) $(InviteAppGtk_CSFILES_CSC) /doc:InvitationWizard.doc.xml

CollectionBrowser.exe: $(CollectionBrowserGtk_CSFILES) $(CollectionBrowserGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionBrowserGtk_FLAGS) $(CollectionBrowserGtk_LIBS:%=/r:%) $(CollectionBrowserGtk_LIBPATH:%=/lib:%) $(CollectionBrowserGtk_RESOURCES:%=$(ICON_FLAG)%) $(CollectionBrowserGtk_CSFILES_CSC) /doc:CollectionBrowser.doc.xml

install-exec-local: iFolderApp.exe InvitationWizard.exe CollectionBrowser.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderApp.exe InvitationWizard.exe CollectionBrowser.exe $(DATA_FILES) $(INSTALL_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderApp.exe InvitationWizard.exe CollectionBrowser.exe $(notdir $(DATA_FILES)) $(notdir $(INSTALL_FILES))
    
installcheck-local: install
	
CLEAN_FILES = iFolderApp.exe iFolderApp.doc.xml InvitationWizard.exe InvitationWizard.doc.xml CollectionBrowser.exe CollectionBrowser.doc.xml

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -f ifolder
	rm -f invitationwizard
	rm -f collectionbrowser

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

