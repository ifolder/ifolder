iFolderComponent_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderAdvanced.cs $(srcdir)/Win32Window.cs $(srcdir)/iFolderComponent.cs
iFolderComponent_CSFILES_CSC := $(subst /,$(SEP),$(iFolderComponent_CSFILES))
iFolderComponent_INCLUDES = 
iFolderComponent_RESOURCES =
iFolderComponent_FLAGS = $(CSC_LIBFLAG)
iFolderComponent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) iFolder.dll CollectionStore.dll System.Drawing.dll System.Windows.Forms.dll AddressBook.dll FormsBookLib.dll
iFolderComponent_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin $(ADDRESSBOOK_INSTALL_PATH)/bin ../iFolder

NUNIT_TESTS := iFolderComponentTests.dll

iFolderComponentTests_CSFILES = $(srcdir)/iFolderComponentTests/AssemblyInfo.cs $(srcdir)/iFolderComponentTests/iFolderComponentTests.cs
iFolderComponentTests_CSFILES_CSC := $(subst /,$(SEP),$(iFolderComponentTests_CSFILES))
iFolderComponentTests_INCLUDES = 
iFolderComponentTests_RESOURCES =
iFolderComponentTests_FLAGS = $(CSC_LIBFLAG)
iFolderComponentTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) iFolderComponent.dll nunit.framework.dll nunit.extensions.dll
iFolderComponentTests_LIBPATH = $(iFolderComponent_LIBPATH) $(srcdir)/../../tools/NUnit/bin

iFolderShell_SRCFILES = $(srcdir)/iFolderShell/ContextMenu.cpp $(srcdir)/iFolderShell/IconOverlay.cpp $(srcdir)/iFolderShell/iFolderShell.cpp $(srcdir)/iFolderShell/PropertySheet.cpp $(srcdir)/iFolderShell/Registry.cpp $(srcdir)/iFolderShell/ShareiFolder.cpp $(srcdir)/iFolderShell/iFolderShell.rc
iFolderShell_OBJFILES = iFolderShell/ContextMenu.obj iFolderShell/IconOverlay.obj iFolderShell/iFolderShell.obj iFolderShell/PropertySheet.obj iFolderShell/Registry.obj iFolderShell/ShareiFolder.obj iFolderShell/iFolderShell$(RES_EXT)
iFolderShell_INCLUDES = iFolderShell
iFolderShell_FLAGS = -D_USRDLL -DIFOLDERSHELL_EXPORTS
iFolderShell_CHARFLAGS = -DUNICODE -D_UNICODE
iFolderShell_CXXFLAGS = $(CXXFLAGS) $(iFolderShell_CHARFLAGS)
iFolderShell_LIBS = comctl32.lib
iFolderShell_LIBPATH = iFolderShell
iFolderShell_LIBFLAGS = $(SHARED_LIB_FLAG)
iFolderShell_DEF = $(srcdir)/iFolderShell/iFolderShell.Def

DATA_FILES = $(srcdir)/iFolderShell/ifolder_emblem.ico

EXTRA_DIST = $(iFolderComponent_CSFILES) $(iFolderComponentTests_CSFILES) $(iFolderShell_SRCFILES) $(iFolderShell_DEF) $(DATA_FILES)

CUR_DIR := $(shell pwd)

all: iFolderShell.dll

iFolderShell.dll: iFolderComponent.tlb $(iFolderShell_OBJFILES)
	$(LD) $(iFolderShell_LIBFLAGS) $(LDFLAGS) $(LDOUT)$@ $(iFolderShell_LIBPATH:%=$(LDINC)%) $(iFolderShell_DEF:%=$(DEF)%) $(iFolderShell_OBJFILES) $(iFolderShell_LIBS:%=$(LIBFLAG)%) $(LIBS)

%.obj : %.c
	$(CC) $(COUT)$@ $(iFolderShell_FLAGS) $(iFolderShell_INCLUDES:%=$(CINC)%) $(CFLAGS) $<

%.obj : %.cpp
	$(CXX) $(COUT)$@ $(iFolderShell_FLAGS) $(iFolderShell_INCLUDES:%=$(CINC)%) $(iFolderShell_CXXFLAGS) $<

%$(RES_EXT) : %.rc
	$(RC) $(RCFLAGS) $(iFolderShell_CHARFLAGS) $(COUT)$@ $<

iFolderComponent.tlb: iFolderComponent.dll
	$(TLBX) $< $(LDOUT)$@

iFolderComponent.dll: $(iFolderComponent_CSFILES) $(iFolderComponent_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderComponent_FLAGS) $(iFolderComponent_LIBS:%=/r:%) $(iFolderComponent_LIBPATH:%=/lib:%) $(iFolderComponent_CSFILES_CSC) /doc:iFolderComponent.xml

iFolderComponentTests.dll: $(iFolderComponentTests_CSFILES) $(iFolderComponentTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderComponentTests_FLAGS) $(iFolderComponentTests_LIBS:%=/r:%) $(iFolderComponentTests_LIBPATH:%=/lib:%) $(iFolderComponentTests_CSFILES_CSC)
	
install-exec-local: iFolderComponent.dll iFolderShell.dll
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolderComponent.dll iFolderShell.dll $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolderComponent.dll iFolderShell.dll iFolderComponentTests.xml $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:iFolderComponentTests.xml $(NUNIT_TESTS)
	
CLEAN_FILES = iFolderShell.dll iFolderComponent.tlb iFolderComponent.dll $(iFolderShell_OBJFILES) iFolderShell/ifoldercomponent.tlh iFolderShell/ifoldercomponent.tli $(NUNIT_TESTS) iFolderComponent.xml iFolderComponentTests.xml iFolderShell.exp iFolderShell.lib
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

