# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

UILIB_CSFILES = $(srcdir)/ConflictResolver.cs $(srcdir)/iFolderProperties.cs $(srcdir)/ApplicationProperties.cs $(srcdir)/DirectoryEntry.cs $(srcdir)/CollectionBrowser.cs $(srcdir)/iFolderBrowser.cs $(srcdir)/IntroDialog.cs $(srcdir)/SettingsPage.cs $(srcdir)/CrashReport.cs $(srcdir)/ReunionBrowser.cs
UILIB_CSFILES_CSC := $(subst /,$(SEP),$(UILIB_CSFILES))
UILIB_INCLUDES = 
UILIB_RESOURCES =
UILIB_FLAGS = $(CSC_LIBFLAG)
UILIB_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.iFolder.dll
UILIB_LIBPATH = $(srcdir)/../iFolder


TrayAppGtk_CSFILES = $(srcdir)/TrayApp.cs
TrayAppGtk_CSFILES_CSC := $(subst /,$(SEP),$(TrayAppGtk_CSFILES))
TrayAppGtk_INCLUDES = 
TrayAppGtk_RESOURCES =
TrayAppGtk_FLAGS = $(CSC_EXEFLAG)
TrayAppGtk_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.iFolder.dll TrayIcon.dll Novell.iFolder.UI.gtk.dll
TrayAppGtk_LIBPATH = $(srcdir)/../TrayIcon ../iFolder ../TrayIcon


InviteAppGtk_CSFILES = $(srcdir)/InviteApp.cs
InviteAppGtk_CSFILES_CSC := $(subst /,$(SEP),$(InviteAppGtk_CSFILES))
InviteAppGtk_INCLUDES = 
InviteAppGtk_RESOURCES =
InviteAppGtk_FLAGS = $(CSC_EXEFLAG)
InviteAppGtk_LIBS = System.dll System.Drawing.dll System.Data.dll $(SYSTEM_XML) Novell.iFolder.dll Novell.iFolder.UI.gtk.dll
InviteAppGtk_LIBPATH = ../iFolder


CollectionBrowserGtk_CSFILES = $(srcdir)/CBApp.cs
CollectionBrowserGtk_CSFILES_CSC := $(subst /,$(SEP),$(CollectionBrowserGtk_CSFILES))
CollectionBrowserGtk_INCLUDES = 
CollectionBrowserGtk_RESOURCES =
CollectionBrowserGtk_FLAGS = $(CSC_EXEFLAG)
CollectionBrowserGtk_LIBS = System.dll System.Drawing.dll System.Data.dll $(SYSTEM_XML) Novell.iFolder.dll Novell.iFolder.UI.gtk.dll
CollectionBrowserGtk_LIBPATH = ../iFolder


ReunionBrowserGtk_CSFILES = $(srcdir)/RBApp.cs
ReunionBrowserGtk_CSFILES_CSC := $(subst /,$(SEP),$(ReunionBrowserGtk_CSFILES))
ReunionBrowserGtk_INCLUDES = 
ReunionBrowserGtk_RESOURCES =
ReunionBrowserGtk_FLAGS = $(CSC_EXEFLAG)
ReunionBrowserGtk_LIBS = System.dll System.Drawing.dll System.Data.dll $(SYSTEM_XML) Novell.iFolder.dll Simias.dll Novell.iFolder.UI.gtk.dll
ReunionBrowserGtk_LIBPATH = ../iFolder


iFolderBrowserGtk_CSFILES = $(srcdir)/FBApp.cs
iFolderBrowserGtk_CSFILES_CSC := $(subst /,$(SEP),$(iFolderBrowserGtk_CSFILES))
iFolderBrowserGtk_INCLUDES = 
iFolderBrowserGtk_RESOURCES =
iFolderBrowserGtk_FLAGS = $(CSC_EXEFLAG)
iFolderBrowserGtk_LIBS = System.dll System.Drawing.dll System.Data.dll $(SYSTEM_XML) Novell.iFolder.dll Novell.AddressBook.UI.gtk.dll Novell.iFolder.UI.gtk.dll
iFolderBrowserGtk_LIBPATH = ../iFolder


INSTALL_BIN_FILES = $(srcdir)/invitationwizard $(srcdir)/collectionbrowser $(srcdir)/ifolder $(srcdir)/ifolderbrowser

INSTALL_LIB_FILES = $(srcdir)/setup-autostart

GLADE_FILES = ifolder-properties.glade conflict-resolver.glade application-properties.glade ifolder-banner.png ifolder.glade collection-browser.glade ifolder-confirmation.glade properties-dialog.glade crash-report.glade reunion-browser.glade ifolder-browser.glade collection.png folder.png ifolder-collision.png ifolder.png ifolderfolder.png invitewiz.png nuke.png

IMAGE_FILES = invited-contact.png collection.png Node.png Tombstone.png BaseContact.png folder.png ifolder.png ifolder-collision.png ifolder-shutdown.png ifolder-startup.png ifolderfolder.png file.png ifolder.gif property.png 

MIME_FILES = $(srcdir)/ifolder.mime $(srcdir)/ifolder.keys
APPLICATIONS_FILES = $(srcdir)/ifolder.applications

EXTRA_DIST = $(UILIB_CSFILES) $(TrayAppGtk_CSFILES) $(InviteAppGtk_CSFILES) $(CollectionBrowserGtk_CSFILES) $(ReunionBrowserGtk_CSFILES) $(iFolderBrowserGtk_CSFILES) $(GLADE_FILES) $(IMAGE_FILES) $(INSTALL_BIN_FILES) $(INSTALL_LIB_FILES) ifolder.desktop.in $(MIME_FILES) $(APPLICATIONS_FILES)

all: Novell.iFolder.UI.gtk.dll iFolderGtkApp.exe CollectionBrowser.exe ReunionBrowser.exe iFolderBrowser.exe

Novell.iFolder.UI.gtk.dll: $(UILIB_CSFILES) $(UILIB_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(UILIB_FLAGS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(SIMIAS_LIBS) $(ADDRESSBOOK_LIBS) $(UILIB_LIBS:%=/r:%) $(UILIB_LIBPATH:%=/lib:%) $(UILIB_RESOURCES:%=$(ICON_FLAG)%) $(UILIB_CSFILES_CSC) Util.cs /doc:Novell.iFolder.UI.gtk.doc.xml

iFolderGtkApp.exe: $(TrayAppGtk_CSFILES) $(TrayAppGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(TrayAppGtk_FLAGS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(SIMIAS_LIBS) $(ADDRESSBOOK_LIBS) $(TrayAppGtk_LIBS:%=/r:%) $(TrayAppGtk_LIBPATH:%=/lib:%) $(TrayAppGtk_RESOURCES:%=$(ICON_FLAG)%) $(TrayAppGtk_CSFILES_CSC) Util.cs /doc:iFolderGtkApp.doc.xml

InvitationWizard.exe: $(InviteAppGtk_CSFILES) $(InviteAppGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(InviteAppGtk_FLAGS) $(SIMIAS_LIBS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(InviteAppGtk_LIBS:%=/r:%) $(InviteAppGtk_LIBPATH:%=/lib:%) $(InviteAppGtk_RESOURCES:%=$(ICON_FLAG)%) $(InviteAppGtk_CSFILES_CSC) Util.cs /doc:InvitationWizard.doc.xml

CollectionBrowser.exe: $(CollectionBrowserGtk_CSFILES) $(CollectionBrowserGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionBrowserGtk_FLAGS) $(SIMIAS_LIBS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(CollectionBrowserGtk_LIBS:%=/r:%) $(CollectionBrowserGtk_LIBPATH:%=/lib:%) $(CollectionBrowserGtk_RESOURCES:%=$(ICON_FLAG)%) $(CollectionBrowserGtk_CSFILES_CSC) Util.cs /doc:CollectionBrowser.doc.xml

ReunionBrowser.exe: $(ReunionBrowserGtk_CSFILES) $(ReunionBrowserGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ReunionBrowserGtk_FLAGS) $(SIMIAS_LIBS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(ReunionBrowserGtk_LIBS:%=/r:%) $(ReunionBrowserGtk_LIBPATH:%=/lib:%) $(ReunionBrowserGtk_RESOURCES:%=$(ICON_FLAG)%) $(ReunionBrowserGtk_CSFILES_CSC) Util.cs /doc:ReunionBrowser.doc.xml

iFolderBrowser.exe: $(iFolderBrowserGtk_CSFILES) $(iFolderBrowserGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderBrowserGtk_FLAGS) $(SIMIAS_LIBS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(iFolderBrowserGtk_LIBS:%=/r:%) $(iFolderBrowserGtk_LIBPATH:%=/lib:%) $(iFolderBrowserGtk_RESOURCES:%=$(ICON_FLAG)%) $(iFolderBrowserGtk_CSFILES_CSC) Util.cs /doc:iFolderBrowser.doc.xml

install-exec-local: Novell.iFolder.UI.gtk.dll iFolderGtkApp.exe InvitationWizard.exe CollectionBrowser.exe ReunionBrowser.exe iFolderBrowser.exe $(GLADE_FILES) $(IMAGE_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir) $(DESTDIR)$(libdir) $(DESTDIR)$(datadir)/ifolder/glade $(DESTDIR)$(datadir)/ifolder/images $(DESTDIR)$(datadir)/applications $(DESTDIR)$(datadir)/pixmaps $(DESTDIR)$(datadir)/mime-info $(DESTDIR)$(datadir)/application-registry
	$(INSTALL_PROGRAM) Novell.iFolder.UI.gtk.dll $(INSTALL_LIB_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(GLADE_FILES) $(DESTDIR)$(datadir)/ifolder/glade
	$(INSTALL_PROGRAM) $(IMAGE_FILES) $(DESTDIR)$(datadir)/ifolder/images
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) iFolderGtkApp.exe CollectionBrowser.exe ReunionBrowser.exe iFolderBrowser.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder.desktop $(DESTDIR)$(datadir)/applications
	$(INSTALL_PROGRAM) ifolder.png $(DESTDIR)$(datadir)/pixmaps
	$(INSTALL_PROGRAM) --mode=644 $(MIME_FILES) $(DESTDIR)$(datadir)/mime-info
	$(INSTALL_PROGRAM) --mode=644 $(APPLICATIONS_FILES) $(DESTDIR)$(datadir)/application-registry
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.iFolder.UI.gtk.dll $(notdir $(INSTALL_LIB_FILES))
	cd $(DESTDIR)$(datadir)/ifolder/glade; rm -f $(GLADE_FILES)
	cd $(DESTDIR)$(datadir)/ifolder/images; rm -f $(IMAGE_FILES)
	cd $(DESTDIR)$(bindir); rm -f iFolderGtkApp.exe CollectionBrowser.exe ReunionBrowser.exe iFolderBrowser.exe $(notdir $(INSTALL_BIN_FILES))
	cd $(DESTDIR)$(datadir)/applications; rm -f ifolder.desktop
	cd $(DESTDIR)$(datadir)/pixmaps; rm -f ifolder.png
	cd $(DESTDIR)$(datadir)/mime-info; rm -f $(MIME_FILES)
	cd $(DESTDIR)$(datadir)/application-registry; rm -f $(APPLICATIONS_FILES)
    
installcheck-local: install
	
CLEAN_FILES = Novell.iFolder.UI.gtk.dll Novell.iFolder.UI.gtk.doc.xml iFolderGtkApp.exe iFolderGtkApp.doc.xml InvitationWizard.exe InvitationWizard.doc.xml CollectionBrowser.exe ReunionBrowser.exe iFolderBrowser.exe CollectionBrowser.doc.xml

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -f ifolder
	rm -f ifolderbrowser
	rm -f invitationwizard
	rm -f collectionbrowser

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

