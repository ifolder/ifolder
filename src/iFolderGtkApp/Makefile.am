# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderApp_CSFILES = $(srcdir)/iFolderApp.cs
iFolderApp_CSFILES_CSC := $(subst /,$(SEP),$(iFolderApp_CSFILES))
iFolderApp_INCLUDES = 
iFolderApp_RESOURCES =
iFolderApp_FLAGS = $(CSC_EXEFLAG)
iFolderApp_LIBS = System.dll System.Drawing.dll System.Data.dll Novell.iFolder.dll Novell.iFolder.UI.gtk.dll
iFolderApp_LIBPATH = $(srcdir)/../iFolderGtkUI $(srcdir)/../iFolder

INSTALL_BIN_FILES = $(srcdir)/ifolder

EXTRA_DIST = $(iFolderApp_CSFILES) $(INSTALL_BIN_FILES)

all: iFolderApp.exe

iFolderApp.exe: $(iFolderApp_CSFILES) $(iFolderApp_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderApp_FLAGS) $(GTK_SHARP_LIBS) $(GNOME_SHARP_LIBS) $(SIMIAS_LIBS) $(ADDRESSBOOK_UI_LIBS) $(iFolderApp_LIBS:%=/r:%) $(iFolderApp_LIBPATH:%=/lib:%) $(iFolderApp_RESOURCES:%=$(ICON_FLAG)%) $(iFolderApp_CSFILES_CSC) /doc:iFolderApp.doc.xml

install-exec-local: all $(GLADE_FILES) $(IMAGE_FILES)
	$(mkinstalldirs) $(DESTDIR)$(bindir) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(INSTALL_EXE_OPTS) iFolderApp.exe $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(INSTALL_BIN_FILES) $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderApp.exe $(notdir $(INSTALL_BIN_FILES))
    
CLEAN_FILES = iFolderApp.exe

clean-local:
	rm -rf *.dbg *.exe $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -f ifolder

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

