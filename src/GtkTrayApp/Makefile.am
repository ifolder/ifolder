SUBDIRS = po

DIST_SUBDIRS = $(SUBDIRS)

# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

UILIB_CSFILES = $(srcdir)/iFolderConflictDialog.cs $(srcdir)/iFolderAcceptDialog.cs $(srcdir)/iFolderUserSelector.cs $(srcdir)/iFolderLoginDialog.cs $(srcdir)/iFolderPropSettingsPage.cs $(srcdir)/iFolderPropSharingPage.cs $(srcdir)/iFolderPropertiesDialog.cs $(srcdir)/iFolderCrashDialog.cs $(srcdir)/iFolderMsgDialog.cs $(srcdir)/iFolderWindow.cs $(srcdir)/iFolderCreationDialog.cs $(srcdir)/iFolderExceptionDialog.cs $(srcdir)/iFolderAccessDialog.cs $(srcdir)/iFolderTreeView.cs $(srcdir)/NotifyWindow.cs $(srcdir)/CompatFileChooserDialog.cs $(srcdir)/ClientConfig.cs
UILIB_CSFILES_CSC := $(subst /,$(SEP),$(UILIB_CSFILES))
UILIB_INCLUDES = 
UILIB_RESOURCES =
UILIB_FLAGS = $(CSC_LIBFLAG)
UILIB_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.iFolder.dll Mono.Posix.dll
UILIB_LIBPATH = $(srcdir)/../iFolder


TrayAppGtk_CSFILES = $(srcdir)/iFolderApplication.cs $(srcdir)/SimiasEventBroker.cs
TrayAppGtk_CSFILES_CSC := $(subst /,$(SEP),$(TrayAppGtk_CSFILES))
TrayAppGtk_INCLUDES = 
TrayAppGtk_RESOURCES =
TrayAppGtk_FLAGS = $(CSC_EXEFLAG)
TrayAppGtk_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.iFolder.dll TrayIcon.dll Novell.iFolder.UI.gtk.dll Mono.Posix.dll
TrayAppGtk_LIBPATH = $(srcdir)/../TrayIcon ../iFolder ../TrayIcon


INSTALL_BIN_FILES = $(srcdir)/ifolder

INSTALL_LIB_FILES = $(srcdir)/setup-autostart

IMAGE_FILES = ifolder-about.png inviteduser.png currentuser.png ifolderuser.png serverifolder.png ifolder48.png ifolder.png ifolder-banner-scaled.png ifolder-banner.png ifolder-collision.png ifolder-shutdown.png ifolder-startup.png ifolder.gif ifolder-crash.png ifolder-crash.gif

MIME_FILES = $(srcdir)/ifolder.mime $(srcdir)/ifolder.keys
APPLICATIONS_FILES = $(srcdir)/ifolder.applications

EXTRA_DIST = $(UILIB_CSFILES) $(TrayAppGtk_CSFILES) $(IMAGE_FILES) ifolder.in $(INSTALL_LIB_FILES) ifolder.desktop.in $(MIME_FILES) $(APPLICATIONS_FILES)

all: Novell.iFolder.UI.gtk.dll iFolderGtkApp.exe 
#all: Novell.iFolder.UI.gtk.dll iFolderGtkApp.exe CollectionBrowser.exe ReunionBrowser.exe 

Novell.iFolder.UI.gtk.dll: $(UILIB_CSFILES) $(UILIB_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_CLIENT_LIBS) $(UILIB_FLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(UILIB_LIBS:%=/r:%) $(UILIB_LIBPATH:%=/lib:%) $(UILIB_RESOURCES:%=$(ICON_FLAG)%) $(UILIB_CSFILES_CSC) Util.cs /doc:Novell.iFolder.UI.gtk.doc.xml

iFolderGtkApp.exe: $(TrayAppGtk_CSFILES) $(TrayAppGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_CLIENT_LIBS) $(TrayAppGtk_FLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(TrayAppGtk_LIBS:%=/r:%) $(TrayAppGtk_LIBPATH:%=/lib:%) $(TrayAppGtk_RESOURCES:%=$(ICON_FLAG)%) $(TrayAppGtk_CSFILES_CSC) Util.cs /doc:iFolderGtkApp.doc.xml


install-exec-local: all $(IMAGE_FILES) 
	$(mkinstalldirs) $(DESTDIR)$(bindir) $(DESTDIR)$(libdir) $(DESTDIR)$(datadir)/ifolder3/images $(DESTDIR)$(datadir)/applications $(DESTDIR)$(datadir)/pixmaps $(DESTDIR)$(datadir)/mime-info $(DESTDIR)$(datadir)/application-registry
	$(INSTALL_PROGRAM) Novell.iFolder.UI.gtk.dll $(INSTALL_LIB_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(IMAGE_FILES) $(DESTDIR)$(datadir)/ifolder3/images
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) iFolderGtkApp.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_FILES) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) ifolder.desktop $(DESTDIR)$(datadir)/applications
	$(INSTALL_PROGRAM) ifolder.png $(DESTDIR)$(datadir)/pixmaps
	$(INSTALL_PROGRAM) --mode=644 $(MIME_FILES) $(DESTDIR)$(datadir)/mime-info
	$(INSTALL_PROGRAM) --mode=644 $(APPLICATIONS_FILES) $(DESTDIR)$(datadir)/application-registry
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.iFolder.UI.gtk.dll $(notdir $(INSTALL_LIB_FILES))
	cd $(DESTDIR)$(datadir)/ifolder3/images; rm -f $(IMAGE_FILES)
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
