abgui_CSFILES = $(srcdir)/ABGui.cs $(srcdir)/AssemblyInfo.cs
abgui_CSFILES_CSC := $(subst /,$(SEP),$(abgui_CSFILES))
abgui_INCLUDES = 
abgui_RESOURCES = $(srcdir)/App$(ICON_EXT)
abgui_FLAGS = $(CSC_EXEFLAG)
abgui_LIBS = System.dll System.Data.dll System.Drawing.dll System.Windows.Forms.dll $(SYSTEM_XML) Configuration.dll StoreProvider.dll CollectionEvents.dll CollectionStore.dll AddressBook.dll
abgui_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin

EXTRA_DIST = $(abgui_CSFILES)

all: abgui.exe

abgui.exe: $(abgui_CSFILES) $(abgui_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(abgui_FLAGS) $(abgui_LIBS:%=/r:%) $(abgui_LIBPATH:%=/lib:%) $(abgui_RESOURCES:%=$(ICON_FLAG)%) $(abgui_CSFILES_CSC) /doc:abgui.xml

install-exec-local: abgui.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) abgui.exe $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f abgui.exe
    
installcheck-local: install
	
CLEAN_FILES = abgui.exe abgui.xml

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

