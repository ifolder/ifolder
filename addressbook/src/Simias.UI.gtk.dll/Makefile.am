# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

UILIB_CSFILES = $(srcdir)/CollectionGeneralPage.cs $(srcdir)/ContactCollectionSharingPage.cs $(srcdir)/CollectionPropertiesPage.cs $(srcdir)/CollectionProperties.cs
UILIB_CSFILES_CSC := $(subst /,$(SEP),$(UILIB_CSFILES))
UILIB_INCLUDES = 
UILIB_RESOURCES =
UILIB_FLAGS = $(CSC_LIBFLAG)
UILIB_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.AddressBook.dll Novell.AddressBook.UI.gtk.dll
UILIB_LIBPATH = $(srcdir)/../AddressBook $(srcdir)/../GtkAddrBook


INSTALL_BIN_FILES = 

INSTALL_LIB_FILES = 

GLADE_FILES = collection-properties.glade

IMAGE_FILES = invited-contact.png

EXTRA_DIST = $(UILIB_CSFILES) $(GLADE_FILES) $(IMAGE_FILES) $(INSTALL_BIN_FILES) $(INSTALL_LIB_FILES)

all: Simias.UI.gtk.dll

Simias.UI.gtk.dll: $(UILIB_CSFILES) $(UILIB_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(UILIB_FLAGS) $(GLADE_SHARP_LIBS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(UILIB_LIBS:%=/r:%) $(UILIB_LIBPATH:%=/lib:%) $(UILIB_RESOURCES:%=$(ICON_FLAG)%) $(SIMIAS_LIBS) $(UILIB_CSFILES_CSC) Util.cs /doc:Simias.UI.gtk.doc.xml

install-exec-local: all $(GLADE_FILES) $(IMAGE_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir) $(DESTDIR)$(libdir) $(DESTDIR)$(datadir)/ifolder/glade $(DESTDIR)$(datadir)/ifolder/images $(DESTDIR)$(datadir)/applications $(DESTDIR)$(datadir)/pixmaps $(DESTDIR)$(datadir)/mime-info $(DESTDIR)$(datadir)/application-registry
	$(INSTALL_PROGRAM) Simias.UI.gtk.dll $(INSTALL_LIB_FILES) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(GLADE_FILES) $(DESTDIR)$(datadir)/ifolder/glade
	$(INSTALL_PROGRAM) $(IMAGE_FILES) $(DESTDIR)$(datadir)/ifolder/images
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Simias.UI.gtk.dll $(notdir $(INSTALL_LIB_FILES))
	cd $(DESTDIR)$(datadir)/ifolder/glade; rm -f $(GLADE_FILES)
	cd $(DESTDIR)$(datadir)/ifolder/images; rm -f $(IMAGE_FILES)
    
installcheck-local: install
	
CLEAN_FILES = Simias.UI.gtk.dll Simias.UI.gtk.doc.xml

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

