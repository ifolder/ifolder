# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderNautilus_CSFILES = $(srcdir)/NautilusExtension.cs $(srcdir)/../GtkTrayApp/SharingPage.cs $(srcdir)/../GtkTrayApp/NodePropertyPage.cs
iFolderNautilus_CSFILES_CSC := $(subst /,$(SEP),$(iFolderNautilus_CSFILES))
iFolderNautilus_INCLUDES = 
iFolderNautilus_RESOURCES =
iFolderNautilus_FLAGS = $(CSC_LIBFLAG)
iFolderNautilus_LIBS = System.dll System.Drawing.dll System.Data.dll glib-sharp.dll gtk-sharp.dll gnome-sharp.dll glade-sharp.dll gdk-sharp.dll nautilus-sharp.dll $(SYSTEM_XML) AddressBook.dll iFolder.dll BookLib.dll CollectionStore.dll
iFolderNautilus_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin ../iFolder ../GtkTrayApp

INSTALL_FILES =

DATA_FILES = ../GtkTrayApp/ifolder.glade 

EXTRA_DIST = $(iFolderNautilus_CSFILES) $(DATA_FILES) $(INSTALL_FILES)

all: iFolderNautilus.dll

iFolderNautilus.dll: $(iFolderNautilus_CSFILES) $(iFolderNautilus_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderNautilus_FLAGS) $(iFolderNautilus_LIBS:%=/r:%) $(iFolderNautilus_LIBPATH:%=/lib:%) $(iFolderNautilus_RESOURCES:%=$(ICON_FLAG)%) $(iFolderNautilus_CSFILES_CSC) /doc:iFolderNautilus.doc.xml

install-exec-local: iFolderNautilus.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderNautilus.dll $(DATA_FILES) $(INSTALL_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderNautilus.dll $(DATA_FILES) $(notdir $(INSTALL_FILES))
    
installcheck-local: install
	
CLEAN_FILES = iFolderNautilus.dll iFolderNautilus.doc.xml

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean
	rm -f ifolder

maintainer-clean-local:
	rm -f Makefile.in

