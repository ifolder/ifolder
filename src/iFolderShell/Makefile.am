# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolderComponent_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolderAdvanced.cs $(srcdir)/MyMessageBox.cs $(srcdir)/Win32Window.cs $(srcdir)/iFolderComponent.cs $(srcdir)/NewiFolder.cs $(srcdir)/ConflictResolver.cs $(srcdir)/CollisionNode.cs
iFolderComponent_CSFILES_CSC := $(subst /,$(SEP),$(iFolderComponent_CSFILES))
iFolderComponent_INCLUDES = 
iFolderComponent_RESOURCES =
iFolderComponent_FLAGS = $(CSC_LIBFLAG)
iFolderComponent_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Drawing.dll System.Windows.Forms.dll Novell.iFolder.dll FormsBookLib.dll Simias.dll
iFolderComponent_LIBPATH = ../iFolder

NUNIT_TESTS := iFolderComponentTests.dll

iFolderComponentTests_CSFILES = $(srcdir)/iFolderComponentTests/AssemblyInfo.cs $(srcdir)/iFolderComponentTests/iFolderComponentTests.cs
iFolderComponentTests_CSFILES_CSC := $(subst /,$(SEP),$(iFolderComponentTests_CSFILES))
iFolderComponentTests_INCLUDES = 
iFolderComponentTests_RESOURCES =
iFolderComponentTests_FLAGS = $(CSC_LIBFLAG)
iFolderComponentTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) iFolderComponent.dll nunit.framework.dll nunit.extensions.dll
iFolderComponentTests_LIBPATH = $(iFolderComponent_LIBPATH) $(NUNIT_LIBPATH)

iFolderShell_SRCFILES = $(srcdir)/iFolderShell/ContextMenu.cpp $(srcdir)/iFolderShell/IconOverlay.cpp $(srcdir)/iFolderShell/iFolderShell.cpp $(srcdir)/iFolderShell/PropertySheet.cpp $(srcdir)/iFolderShell/Registry.cpp $(srcdir)/iFolderShell/iFolderShell.rc
iFolderShell_OBJFILES = iFolderShell/ContextMenu.obj iFolderShell/IconOverlay.obj iFolderShell/iFolderShell.obj iFolderShell/PropertySheet.obj iFolderShell/Registry.obj iFolderShell/iFolderShell$(RES_EXT)
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
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderComponent_FLAGS) $(SIMIAS_LIBS) $(iFolderComponent_LIBS:%=/r:%) $(iFolderComponent_LIBPATH:%=/lib:%) $(iFolderComponent_CSFILES_CSC) /doc:iFolderComponent.doc.xml

iFolderComponentTests.dll: $(iFolderComponentTests_CSFILES) $(iFolderComponentTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderComponentTests_FLAGS) $(iFolderComponentTests_LIBS:%=/r:%) $(iFolderComponentTests_LIBPATH:%=/lib:%) $(iFolderComponentTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = iFolderComponent.pdb vc70.pdb
DEBUG_CHECK_FILES = iFolderComponentTests.pdb
endif
endif

install-exec-local: iFolderComponent.dll iFolderShell.dll
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) iFolderComponent.dll iFolderShell.dll $(DEBUG_FILES) $(DESTDIR)$(libdir)
    
uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f iFolderComponent.dll iFolderShell.dll iFolderComponentTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(libdir)
	cd $(DESTDIR)$(libdir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:iFolderComponentTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = iFolderShell.dll iFolderComponent.tlb iFolderComponent.dll $(iFolderShell_OBJFILES) iFolderShell/ifoldercomponent.tlh iFolderShell/ifoldercomponent.tli $(NUNIT_TESTS) iFolderComponent.doc.xml iFolderComponentTests.test.xml iFolderShell.exp iFolderShell.lib

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

