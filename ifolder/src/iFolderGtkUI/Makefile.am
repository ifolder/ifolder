# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

UILIB_CSFILES = $(srcdir)/ServerInfoDialog.cs $(srcdir)/ConflictResolver.cs $(srcdir)/ApplicationProperties.cs $(srcdir)/CollectionBrowser.cs $(srcdir)/IntroDialog.cs $(srcdir)/SettingsPage.cs $(srcdir)/CrashReport.cs $(srcdir)/ReunionBrowser.cs
UILIB_CSFILES_CSC := $(subst /,$(SEP),$(UILIB_CSFILES))
UILIB_INCLUDES = 
UILIB_RESOURCES =
UILIB_FLAGS = $(CSC_LIBFLAG)
UILIB_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.iFolder.dll
UILIB_LIBPATH = $(srcdir)/../iFolder

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

INSTALL_BIN_FILES = $(srcdir)/collectionbrowser

GLADE_FILES = server-info.glade conflict-resolver.glade application-properties.glade ifolder-banner.png ifolder.glade collection-browser.glade ifolder-confirmation.glade crash-report.glade reunion-browser.glade collection.png folder.png ifolder-collision.png ifolder.png ifolderfolder.png nuke.png

IMAGE_FILES = collection.png Node.png Tombstone.png BaseContact.png folder.png ifolder.png ifolder-collision.png ifolder-shutdown.png ifolder-startup.png ifolderfolder.png file.png ifolder.gif property.png 

EXTRA_DIST = $(UILIB_CSFILES) $(CollectionBrowserGtk_CSFILES) $(ReunionBrowserGtk_CSFILES) $(GLADE_FILES) $(IMAGE_FILES) $(INSTALL_BIN_FILES) 

all: Novell.iFolder.UI.gtk.dll CollectionBrowser.exe ReunionBrowser.exe 

Novell.iFolder.UI.gtk.dll: $(UILIB_CSFILES) $(UILIB_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(UILIB_FLAGS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(SIMIAS_LIBS) $(ADDRESSBOOK_UI_LIBS) $(UILIB_LIBS:%=/r:%) $(UILIB_LIBPATH:%=/lib:%) $(UILIB_RESOURCES:%=$(ICON_FLAG)%) $(UILIB_CSFILES_CSC) Util.cs /doc:Novell.iFolder.UI.gtk.doc.xml

CollectionBrowser.exe: $(CollectionBrowserGtk_CSFILES) $(CollectionBrowserGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(CollectionBrowserGtk_FLAGS) $(SIMIAS_LIBS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(CollectionBrowserGtk_LIBS:%=/r:%) $(CollectionBrowserGtk_LIBPATH:%=/lib:%) $(CollectionBrowserGtk_RESOURCES:%=$(ICON_FLAG)%) $(CollectionBrowserGtk_CSFILES_CSC) Util.cs /doc:CollectionBrowser.doc.xml

ReunionBrowser.exe: $(ReunionBrowserGtk_CSFILES) $(ReunionBrowserGtk_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(ReunionBrowserGtk_FLAGS) $(SIMIAS_LIBS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(ReunionBrowserGtk_LIBS:%=/r:%) $(ReunionBrowserGtk_LIBPATH:%=/lib:%) $(ReunionBrowserGtk_RESOURCES:%=$(ICON_FLAG)%) $(ReunionBrowserGtk_CSFILES_CSC) Util.cs /doc:ReunionBrowser.doc.xml

install-exec-local: all $(GLADE_FILES) $(IMAGE_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir) $(DESTDIR)$(libdir) $(DESTDIR)$(datadir)/ifolder/glade $(DESTDIR)$(datadir)/ifolder/images $(DESTDIR)$(datadir)/applications $(DESTDIR)$(datadir)/pixmaps $(DESTDIR)$(datadir)/mime-info $(DESTDIR)$(datadir)/application-registry
	$(INSTALL_PROGRAM) Novell.iFolder.UI.gtk.dll $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(GLADE_FILES) $(DESTDIR)$(datadir)/ifolder/glade
	$(INSTALL_PROGRAM) $(IMAGE_FILES) $(DESTDIR)$(datadir)/ifolder/images
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) CollectionBrowser.exe ReunionBrowser.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.iFolder.UI.gtk.dll 
	cd $(DESTDIR)$(datadir)/ifolder/glade; rm -f $(GLADE_FILES)
	cd $(DESTDIR)$(datadir)/ifolder/images; rm -f $(IMAGE_FILES)
	cd $(DESTDIR)$(bindir); rm -f CollectionBrowser.exe ReunionBrowser.exe $(notdir $(INSTALL_BIN_FILES))
	cd $(DESTDIR)$(datadir)/applications; rm -f ifolder.desktop
	cd $(DESTDIR)$(datadir)/pixmaps; rm -f ifolder.png
    
CLEAN_FILES = Novell.iFolder.UI.gtk.dll Novell.iFolder.UI.gtk.doc.xml CollectionBrowser.exe ReunionBrowser.exe CollectionBrowser.doc.xml

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -f collectionbrowser

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

