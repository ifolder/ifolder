# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolder_CSFILES = $(srcdir)/AssemblyInfo.cs $(srcdir)/iFolder.cs $(srcdir)/iFolderAccess.cs $(srcdir)/iFolderNode.cs $(srcdir)/iFolderManager.cs
iFolder_CSFILES_CSC := $(subst /,$(SEP),$(iFolder_CSFILES))
iFolder_INCLUDES =
iFolder_RESOURCES =
iFolder_FLAGS = $(CSC_LIBFLAG)
iFolder_LIBS = System.dll $(SYSTEM_XML)
iFolder_LIBPATH = $(SIMIAS_INSTALL_PATH)/lib

iFolderCmd_CSFILES = $(srcdir)/iFolderCmd.cs
iFolderCmd_CSFILES_CSC := $(subst /,$(SEP),$(iFolderCmd_CSFILES))
iFolderCmd_INCLUDES =
iFolderCmd_RESOURCES =
iFolderCmd_FLAGS = $(CSC_EXEFLAG)
iFolderCmd_LIBS = System.dll $(SYSTEM_XML) Novell.iFolder.dll
iFolderCmd_LIBPATH = $(SIMIAS_INSTALL_PATH)/lib

NUNIT_TESTS := iFolderTests.exe

iFolderTests_CSFILES = $(srcdir)/iFolderTests.cs
iFolderTests_CSFILES_CSC := $(subst /,$(SEP),$(iFolderTests_CSFILES))
iFolderTests_INCLUDES =
iFolderTests_RESOURCES =
iFolderTests_FLAGS = $(CSC_EXEFLAG)
iFolderTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Novell.iFolder.dll
iFolderTests_LIBPATH = $(NUNIT_LIBPATH) $(SIMIAS_INSTALL_PATH)/lib

EXTRA_DIST = $(iFolder_CSFILES) $(iFolderCmd_CSFILES) $(iFolderTests_CSFILES)

CUR_DIR := $(shell pwd)

all: Novell.iFolder.dll iFolderCmd.exe

Novell.iFolder.dll: $(iFolder_CSFILES) $(iFolder_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolder_FLAGS) $(SIMIAS_LIBS) $(iFolder_LIBS:%=/r:%) $(iFolder_LIBPATH:%=/lib:%) $(iFolder_CSFILES_CSC) /doc:Novell.iFolder.doc.xml

iFolderCmd.exe: $(iFolderCmd_CSFILES) $(iFolderCmd_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderCmd_FLAGS) $(SIMIAS_LIBS) $(iFolderCmd_LIBS:%=/r:%) $(iFolderCmd_LIBPATH:%=/lib:%) $(iFolderCmd_CSFILES_CSC) /doc:iFolderCmd.doc.xml

iFolderTests.exe: $(iFolderTests_CSFILES) $(iFolderTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderTests_FLAGS) $(SIMIAS_LIBS) $(iFolderTests_LIBS:%=/r:%) $(iFolderTests_LIBPATH:%=/lib:%) $(iFolderTests_CSFILES_CSC)
	
if DEBUG
if WINDOWS
DEBUG_FILES = Novell.iFolder.pdb iFolderCmd.pdb
DEBUG_CHECK_FILES = iFolderTests.pdb
endif
endif

install-exec-local: Novell.iFolder.dll iFolderCmd.exe
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) Novell.iFolder.dll iFolderCmd.exe $(DEBUG_FILES) $(DESTDIR)$(libdir)

uninstall-local:
	cd $(DESTDIR)$(libdir); rm -f Novell.iFolder.dll iFolderCmd.exe iFolderTests.test.xml $(NUNIT_TESTS) $(DEBUG_FILES) $(DEBUG_CHECK_FILES)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(libdir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DEBUG_CHECK_FILES) $(DESTDIR)$(libdir)
	cd $(DESTDIR)$(libdir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:iFolderTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Novell.iFolder.dll iFolderCmd.exe $(NUNIT_TESTS) Novell.iFolder.doc.xml iFolderTests.test.xml iFolderCmd.doc.xml

clean-local:
	rm -rf $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local:
	rm -rf $(COMMON_DISTCLEAN_FILES)

maintainer-clean-local:
	rm -rf $(COMMON_MAINTAINER_CLEAN_FILES)

