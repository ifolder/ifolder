# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

TrayAppGtk_CSFILES = $(srcdir)/TrayApp.cs $(srcdir)/SharingPage.cs $(srcdir)/FileBrowser.cs $(srcdir)/DirectoryEntry.cs $(srcdir)/PropertiesDialog.cs $(srcdir)/CollectionBrowser.cs $(srcdir)/iFolderBrowser.cs $(srcdir)/InvitationWizard.cs $(srcdir)/NodePropertyPage.cs $(srcdir)/IntroDialog.cs $(srcdir)/SettingsPage.cs $(srcdir)/CrashReport.cs $(srcdir)/ReunionBrowser.cs

TrayAppGtk_CSFILES_CSC := $(subst /,$(SEP),$(TrayAppGtk_CSFILES))
TrayAppGtk_INCLUDES = 
TrayAppGtk_RESOURCES =
TrayAppGtk_FLAGS = $(CSC_EXEFLAG)
TrayAppGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gnome-sharp.dll glade-sharp.dll Novell.iFolder.dll TrayIcon.dll
TrayAppGtk_LIBPATH = $(SIMIAS_INSTALL_PATH)/lib $(srcdir)/../TrayIcon $(ADDRESSBOOK_INSTALL_PATH)/lib ../iFolder ../TrayIcon


InviteAppGtk_CSFILES = $(srcdir)/InviteApp.cs $(srcdir)/InvitationWizard.cs
InviteAppGtk_CSFILES_CSC := $(subst /,$(SEP),$(InviteAppGtk_CSFILES))
InviteAppGtk_INCLUDES = 
InviteAppGtk_RESOURCES =
InviteAppGtk_FLAGS = $(CSC_EXEFLAG)
InviteAppGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gnome-sharp.dll glade-sharp.dll $(SYSTEM_XML) Novell.iFolder.dll
InviteAppGtk_LIBPATH = $(SIMIAS_INSTALL_PATH)/lib $(ADDRESSBOOK_INSTALL_PATH)/lib ../iFolder


CollectionBrowserGtk_CSFILES = $(srcdir)/CBApp.cs $(srcdir)/CollectionBrowser.cs
CollectionBrowserGtk_CSFILES_CSC := $(subst /,$(SEP),$(CollectionBrowserGtk_CSFILES))
CollectionBrowserGtk_INCLUDES = 
CollectionBrowserGtk_RESOURCES =
CollectionBrowserGtk_FLAGS = $(CSC_EXEFLAG)
CollectionBrowserGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gnome-sharp.dll glade-sharp.dll $(SYSTEM_XML) Novell.iFolder.dll
CollectionBrowserGtk_LIBPATH = $(SIMIAS_INSTALL_PATH)/lib $(ADDRESSBOOK_INSTALL_PATH)/lib ../iFolder

ReunionBrowserGtk_CSFILES = $(srcdir)/RBApp.cs $(srcdir)/ReunionBrowser.cs
ReunionBrowserGtk_CSFILES_CSC := $(subst /,$(SEP),$(ReunionBrowserGtk_CSFILES))
ReunionBrowserGtk_INCLUDES = 
ReunionBrowserGtk_RESOURCES =
ReunionBrowserGtk_FLAGS = $(CSC_EXEFLAG)
ReunionBrowserGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gnome-sharp.dll glade-sharp.dll $(SYSTEM_XML) Novell.iFolder.dll Simias.dll
ReunionBrowserGtk_LIBPATH = $(SIMIAS_INSTALL_PATH)/lib $(ADDRESSBOOK_INSTALL_PATH)/lib ../iFolder

iFolderBrowserGtk_CSFILES = $(srcdir)/FBApp.cs $(srcdir)/iFolderBrowser.cs $(srcdir)/SharingPage.cs $(srcdir)/PropertiesDialog.cs $(srcdir)/NodePropertyPage.cs $(srcdir)/SettingsPage.cs $(srcdir)/DirectoryEntry.cs
iFolderBrowserGtk_CSFILES_CSC := $(subst /,$(SEP),$(iFolderBrowserGtk_CSFILES))
iFolderBrowserGtk_INCLUDES = 
iFolderBrowserGtk_RESOURCES =
iFolderBrowserGtk_FLAGS = $(CSC_EXEFLAG)
iFolderBrowserGtk_LIBS = System.dll System.Drawing.dll System.Data.dll gnome-sharp.dll glade-sharp.dll $(SYSTEM_XML) Novell.iFolder.dll Novell.AddressBook.UI.gtk.dll
iFolderBrowserGtk_LIBPATH = $(SIMIAS_INSTALL_PATH)/lib $(ADDRESSBOOK_INSTALL_PATH)/lib ../iFolder

INSTALL_BIN_FILES = $(srcdir)/invitationwizard $(srcdir)/collectionbrowser $(srcdir)/ifolder
INSTALL_LIB_FILES = $(srcdir)/setup-autostart

DATA_FILES = $(srcdir)/ifolder.glade $(srcdir)/collection-browser.glade $(srcdir)/ifolder-confirmation.glade $(srcdir)/properties-dialog.glade $(srcdir)/crash-report.glade $(srcdir)/reunion-browser.glade $(srcdir)/ifolder-browser.glade $(srcdir)/collection.png $(srcdir)/Node.png $(srcdir)/Tombstone.png $(srcdir)/BaseContact.png $(srcdir)/folder.png $(srcdir)/ifolder.png $(srcdir)/ifolderfolder.png $(srcdir)/invitewiz.png $(srcdir)/file.png $(srcdir)/ifolder.gif $(srcdir)/property.png $(srcdir)/nuke.png

EXTRA_DIST = $(TrayAppGtk_CSFILES) $(InviteAppGtk_CSFILES) $(CollectionBrowserGtk_CSFILES) $(ReunionBrowserGtk_CSFILES) $(iFolderBrowserGtk_CSFILES) $(DATA_FILES) $(INSTALL_BIN_FILES) $(INSTALL_LIB_FILES)

all: iFolderGtkApp.exe InvitationWizard.exe CollectionBrowser.exe ReunionBrowser.exe iFolderBrowser.exe

iFolderGtkApp.exe: $(TrayAppGtk_CSFILES) $(TrayAppGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(TrayAppGtk_FLAGS) $(SIMIAS_LIBS) $(ADDRESSBOOK_LIBS) $(TrayAppGtk_LIBS:%=/r:%) $(TrayAppGtk_LIBPATH:%=/lib:%) $(TrayAppGtk_RESOURCES:%=$(ICON_FLAG)%) $(TrayAppGtk_CSFILES_CSC) /doc:iFolderGtkApp.doc.xml

InvitationWizard.exe: $(InviteAppGtk_CSFILES) $(InviteAppGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InviteAppGtk_FLAGS) $(SIMIAS_LIBS) $(GTK_SHARP_LIBS) $(InviteAppGtk_LIBS:%=/r:%) $(InviteAppGtk_LIBPATH:%=/lib:%) $(InviteAppGtk_RESOURCES:%=$(ICON_FLAG)%) $(InviteAppGtk_CSFILES_CSC) /doc:InvitationWizard.doc.xml

CollectionBrowser.exe: $(CollectionBrowserGtk_CSFILES) $(CollectionBrowserGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionBrowserGtk_FLAGS) $(SIMIAS_LIBS) $(GTK_SHARP_LIBS) $(CollectionBrowserGtk_LIBS:%=/r:%) $(CollectionBrowserGtk_LIBPATH:%=/lib:%) $(CollectionBrowserGtk_RESOURCES:%=$(ICON_FLAG)%) $(CollectionBrowserGtk_CSFILES_CSC) /doc:CollectionBrowser.doc.xml

ReunionBrowser.exe: $(ReunionBrowserGtk_CSFILES) $(ReunionBrowserGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ReunionBrowserGtk_FLAGS) $(SIMIAS_LIBS) $(GTK_SHARP_LIBS) $(ReunionBrowserGtk_LIBS:%=/r:%) $(ReunionBrowserGtk_LIBPATH:%=/lib:%) $(ReunionBrowserGtk_RESOURCES:%=$(ICON_FLAG)%) $(ReunionBrowserGtk_CSFILES_CSC) /doc:ReunionBrowser.doc.xml

iFolderBrowser.exe: $(iFolderBrowserGtk_CSFILES) $(iFolderBrowserGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderBrowserGtk_FLAGS) $(SIMIAS_LIBS) $(GTK_SHARP_LIBS) $(iFolderBrowserGtk_LIBS:%=/r:%) $(iFolderBrowserGtk_LIBPATH:%=/lib:%) $(iFolderBrowserGtk_RESOURCES:%=$(ICON_FLAG)%) $(iFolderBrowserGtk_CSFILES_CSC) /doc:iFolderBrowser.doc.xml

install-exec-local: iFolderGtkApp.exe InvitationWizard.exe CollectionBrowser.exe ReunionBrowser.exe iFolderBrowser.exe $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) iFolderGtkApp.exe InvitationWizard.exe CollectionBrowser.exe ReunionBrowser.exe iFolderBrowser.exe $(DATA_FILES) $(INSTALL_LIB_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f iFolderGtkApp.exe InvitationWizard.exe CollectionBrowser.exe ReunionBrowser.exe iFolderBrowser.exe $(notdir $(DATA_FILES)) $(notdir $(INSTALL_LIB_FILES))
	cd $(DESTDIR)$(bindir); rm -f $(notdir $(INSTALL_BIN_FILES))
    
installcheck-local: install
	
CLEAN_FILES = iFolderGtkApp.exe iFolderGtkApp.doc.xml InvitationWizard.exe InvitationWizard.doc.xml CollectionBrowser.exe ReunionBrowser.exe iFolderBrowser.exe CollectionBrowser.doc.xml

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -f ifolder
	rm -f invitationwizard
	rm -f collectionbrowser

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

