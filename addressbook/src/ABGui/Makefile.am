# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

abgui_CSFILES = $(srcdir)/ABGui.cs $(srcdir)/AssemblyInfo.cs
abgui_CSFILES_CSC := $(subst /,$(SEP),$(abgui_CSFILES))
abgui_INCLUDES = 
abgui_RESOURCES = $(srcdir)/App$(ICON_EXT)
abgui_FLAGS = $(CSC_WINEXEFLAG)
abgui_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) Common.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll AddressBook.dll
abgui_LIBPATH =

EXTRA_DIST = $(abgui_CSFILES)

#TARGETS = abgui.exe
# do not build
TARGETS = 

all: $(TARGETS)

abgui.exe: $(abgui_CSFILES) $(abgui_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(abgui_FLAGS) $(SIMIAS_LIBS) $(abgui_LIBS:%=/r:%) $(abgui_LIBPATH:%=/lib:%) $(abgui_RESOURCES:%=$(ICON_FLAG)%) $(abgui_CSFILES_CSC) /doc:abgui.doc.xml

install-exec-local: abgui.exe
#	$(mkinstalldirs) $(DESTDIR)$(bindir)
#	$(INSTALL_PROGRAM) $(TARGETS) $(DESTDIR)$(bindir)
    
uninstall-local:
#	cd $(DESTDIR)$(bindir); rm -f abgui.exe
    
installcheck-local: install
	
CLEAN_FILES = $(TARGETS) abgui.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

