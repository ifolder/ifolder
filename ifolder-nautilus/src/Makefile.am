# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderNautilus_CSFILES = $(srcdir)/NautilusExtension.cs $(srcdir)/SharingPage.cs $(srcdir)/NodePropertyPage.cs
#$(srcdir)/../GtkTrayApp/SharingPage.cs $(srcdir)/../GtkTrayApp/NodePropertyPage.cs
iFolderNautilus_CSFILES_CSC := $(subst /,$(SEP),$(iFolderNautilus_CSFILES))
iFolderNautilus_INCLUDES = 
iFolderNautilus_RESOURCES =
iFolderNautilus_FLAGS = $(CSC_LIBFLAG)
iFolderNautilus_LIBS = System.dll System.Drawing.dll System.Data.dll glib-sharp.dll gtk-sharp.dll gnome-sharp.dll glade-sharp.dll gdk-sharp.dll nautilus-sharp.dll $(SYSTEM_XML) Novell.iFolder.dll Novell.AddressBook.UI.gtk.dll Simias.dll
iFolderNautilus_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin ../iFolder ../GtkTrayApp

INSTALL_FILES =

DATA_FILES = ifolder.glade contact.png contact_me.png property.png

EXTRA_DIST = $(iFolderNautilus_CSFILES) $(DATA_FILES) $(INSTALL_FILES)

all: iFolderNautilus.dll

iFolderNautilus.dll: $(iFolderNautilus_CSFILES) $(iFolderNautilus_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SIMIAS_LIBS) $(IFOLDER_LIBS) $(NAUTILUS_MONO_LIBS) $(iFolderNautilus_FLAGS) $(iFolderNautilus_LIBS:%=/r:%) $(iFolderNautilus_LIBPATH:%=/lib:%) $(iFolderNautilus_RESOURCES:%=$(ICON_FLAG)%) $(iFolderNautilus_CSFILES_CSC) /doc:iFolderNautilus.doc.xml

install-exec-local: iFolderNautilus.dll $(DATA_FILES)
	$(mkinstalldirs) $(DESTDIR)$(libdir)/nautilus/extensions-1.0/mono
	$(INSTALL_PROGRAM) iFolderNautilus.dll $(DATA_FILES) $(DESTDIR)$(libdir)/nautilus/extensions-1.0/mono
    
uninstall-local:
	cd $(DESTDIR)$(libdir)/nautilus/extensions-1.0/mono; rm -f iFolderNautilus.dll $(DATA_FILES)
    
installcheck-local: install
	
CLEAN_FILES = iFolderNautilus.dll iFolderNautilus.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

