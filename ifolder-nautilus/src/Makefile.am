# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderNautilus_CSFILES = $(srcdir)/NautilusExtension.cs
iFolderNautilus_CSFILES_CSC := $(subst /,$(SEP),$(iFolderNautilus_CSFILES))
iFolderNautilus_INCLUDES = 
iFolderNautilus_RESOURCES =
iFolderNautilus_FLAGS = $(CSC_LIBFLAG)
iFolderNautilus_LIBS = System.dll System.Drawing.dll System.Data.dll nautilus-sharp.dll $(SYSTEM_XML) Novell.iFolder.dll Novell.AddressBook.UI.gtk.dll Simias.dll Novell.iFolder.UI.gtk.dll
iFolderNautilus_LIBPATH = 

INSTALL_FILES =

DATA_FILES = 

EXTRA_DIST = $(iFolderNautilus_CSFILES) $(DATA_FILES) $(INSTALL_FILES) emblem-ifolder.png

all: iFolderNautilus.dll

iFolderNautilus.dll: $(iFolderNautilus_CSFILES) $(iFolderNautilus_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(GTK_SHARP_LIBS) $(SIMIAS_LIBS) $(IFOLDER_LIBS) $(NAUTILUS_MONO_LIBS) $(iFolderNautilus_FLAGS) $(iFolderNautilus_LIBS:%=/r:%) $(iFolderNautilus_LIBPATH:%=/lib:%) $(iFolderNautilus_RESOURCES:%=$(ICON_FLAG)%) $(iFolderNautilus_CSFILES_CSC) /doc:iFolderNautilus.doc.xml

install-exec-local: iFolderNautilus.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(libdir)/nautilus/extensions-1.0/mono
	$(mkinstalldirs) $(DESTDIR)$(datadir)/icons/gnome/48x48/emblems
	$(INSTALL_PROGRAM) iFolderNautilus.dll $(DATA_FILES) $(DESTDIR)$(libdir)/nautilus/extensions-1.0/mono
	$(INSTALL_PROGRAM) emblem-ifolder.png  $(DESTDIR)$(datadir)/icons/gnome/48x48/emblems
    
uninstall-local:
	cd $(DESTDIR)$(libdir)/nautilus/extensions-1.0/mono; rm -f iFolderNautilus.dll $(DATA_FILES)
	cd $(DESTDIR)$(datadir)/icons/gnome/48x48/emblems; rm -f emblem-ifolder.png
    
installcheck-local: install
	
CLEAN_FILES = iFolderNautilus.dll iFolderNautilus.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

