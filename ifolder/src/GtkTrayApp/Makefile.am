# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

#UILIB_CSFILES = $(srcdir)/ServerInfoDialog.cs $(srcdir)/ConflictResolver.cs $(srcdir)/ApplicationProperties.cs $(srcdir)/DirectoryEntry.cs $(srcdir)/CollectionBrowser.cs $(srcdir)/IntroDialog.cs $(srcdir)/SettingsPage.cs $(srcdir)/CrashReport.cs $(srcdir)/ReunionBrowser.cs
UILIB_CSFILES = $(srcdir)/iFolderConflictResolver.cs $(srcdir)/iFolderAcceptDialog.cs $(srcdir)/iFolderUserSelector.cs $(srcdir)/iFolderLoginDialog.cs $(srcdir)/iFolderPropSettingsPage.cs $(srcdir)/iFolderPropSharingPage.cs $(srcdir)/iFolderPropertiesDialog.cs $(srcdir)/iFolderCrashDialog.cs $(srcdir)/iFolderMsgDialog.cs $(srcdir)/iFolderWindow.cs $(srcdir)/iFolderCreationDialog.cs $(srcdir)/iFolderExceptionDialog.cs
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


INSTALL_BIN_FILES = $(srcdir)/ifolder

INSTALL_LIB_FILES = $(srcdir)/setup-autostart

GLADE_FILES = conflict-resolver.glade application-properties.glade ifolder-banner.png ifolder.glade collection.png folder.png ifolder-collision.png ifolder.png invitewiz.png

IMAGE_FILES = inviteduser.png ifolderuser.png serverifolder.png collection.png Node.png Tombstone.png BaseContact.png folder.png ifolder.png ifolder-banner.png ifolder-collision.png ifolder-shutdown.png ifolder-startup.png ifolderfolder.png file.png ifolder.gif property.png ifolder-crash.png ifolder-crash.gif

MIME_FILES = $(srcdir)/ifolder.mime $(srcdir)/ifolder.keys
APPLICATIONS_FILES = $(srcdir)/ifolder.applications

EXTRA_DIST = $(UILIB_CSFILES) $(TrayAppGtk_CSFILES) $(GLADE_FILES) $(IMAGE_FILES) ifolder.in $(INSTALL_LIB_FILES) ifolder.desktop.in $(MIME_FILES) $(APPLICATIONS_FILES)

all: Novell.iFolder.UI.gtk.dll iFolderGtkApp.exe 
#all: Novell.iFolder.UI.gtk.dll iFolderGtkApp.exe CollectionBrowser.exe ReunionBrowser.exe 

Novell.iFolder.UI.gtk.dll: $(UILIB_CSFILES) $(UILIB_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(UILIB_FLAGS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(UILIB_LIBS:%=/r:%) $(UILIB_LIBPATH:%=/lib:%) $(UILIB_RESOURCES:%=$(ICON_FLAG)%) $(UILIB_CSFILES_CSC) Util.cs /doc:Novell.iFolder.UI.gtk.doc.xml

iFolderGtkApp.exe: $(TrayAppGtk_CSFILES) $(TrayAppGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(TrayAppGtk_FLAGS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(TrayAppGtk_LIBS:%=/r:%) $(TrayAppGtk_LIBPATH:%=/lib:%) $(TrayAppGtk_RESOURCES:%=$(ICON_FLAG)%) $(TrayAppGtk_CSFILES_CSC) Util.cs /doc:iFolderGtkApp.doc.xml


install-exec-local: all $(GLADE_FILES) $(IMAGE_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir) $(DESTDIR)$(libdir) $(DESTDIR)$(datadir)/ifolder/glade $(DESTDIR)$(datadir)/ifolder/images $(DESTDIR)$(datadir)/applications $(DESTDIR)$(datadir)/pixmaps $(DESTDIR)$(datadir)/mime-info $(DESTDIR)$(datadir)/application-registry
	$(INSTALL_PROGRAM) Novell.iFolder.UI.gtk.dll $(INSTALL_LIB_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(GLADE_FILES) $(DESTDIR)$(datadir)/ifolder/glade
	$(INSTALL_PROGRAM) $(IMAGE_FILES) $(DESTDIR)$(datadir)/ifolder/images
#	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) iFolderGtkApp.exe CollectionBrowser.exe ReunionBrowser.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) iFolderGtkApp.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder.desktop $(DESTDIR)$(datadir)/applications
	$(INSTALL_PROGRAM) ifolder.png $(DESTDIR)$(datadir)/pixmaps
	$(INSTALL_PROGRAM) --mode=644 $(MIME_FILES) $(DESTDIR)$(datadir)/mime-info
	$(INSTALL_PROGRAM) --mode=644 $(APPLICATIONS_FILES) $(DESTDIR)$(datadir)/application-registry
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.iFolder.UI.gtk.dll $(notdir $(INSTALL_LIB_FILES))
	cd $(DESTDIR)$(datadir)/ifolder/glade; rm -f $(GLADE_FILES)
	cd $(DESTDIR)$(datadir)/ifolder/images; rm -f $(IMAGE_FILES)
#	cd $(DESTDIR)$(bindir); rm -f iFolderGtkApp.exe CollectionBrowser.exe ReunionBrowser.exe $(notdir $(INSTALL_BIN_FILES))
	cd $(DESTDIR)$(bindir); rm -f iFolderGtkApp.exe $(notdir $(INSTALL_BIN_FILES))
	cd $(DESTDIR)$(datadir)/applications; rm -f ifolder.desktop
	cd $(DESTDIR)$(datadir)/pixmaps; rm -f ifolder.png
	cd $(DESTDIR)$(datadir)/mime-info; rm -f $(MIME_FILES)
	cd $(DESTDIR)$(datadir)/application-registry; rm -f $(APPLICATIONS_FILES)
    
CLEAN_FILES = Novell.iFolder.UI.gtk.dll Novell.iFolder.UI.gtk.doc.xml iFolderGtkApp.exe iFolderGtkApp.doc.xml

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -f ifolder

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

