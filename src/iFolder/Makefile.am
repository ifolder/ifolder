# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

iFolder_CSFILES = $(srcdir)/iFolder.cs $(srcdir)/iFolderFile.cs $(srcdir)/iFolderManager.cs
iFolder_CSFILES_CSC := $(subst /,$(SEP),$(iFolder_CSFILES))
iFolder_INCLUDES = 
iFolder_RESOURCES =
iFolder_FLAGS = $(CSC_LIBFLAG)
iFolder_LIBS = System.dll $(SYSTEM_XML) CollectionStore.dll Agent.dll AddressBook.dll
iFolder_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin

iFolderCmd_CSFILES = $(srcdir)/iFolderCmd.cs
iFolderCmd_CSFILES_CSC := $(subst /,$(SEP),$(iFolderCmd_CSFILES))
iFolderCmd_INCLUDES = 
iFolderCmd_RESOURCES =
iFolderCmd_FLAGS = $(CSC_EXEFLAG)
iFolderCmd_LIBS = System.dll $(SYSTEM_XML) CollectionStore.dll Agent.dll iFolder.dll AddressBook.dll
iFolderCmd_LIBPATH = $(SIMIAS_INSTALL_PATH)/bin

NUNIT_TESTS := iFolderTests.dll

iFolderTests_CSFILES = $(srcdir)/iFolderTests.cs
iFolderTests_CSFILES_CSC := $(subst /,$(SEP),$(iFolderTests_CSFILES))
iFolderTests_INCLUDES = 
iFolderTests_RESOURCES =
iFolderTests_FLAGS = $(CSC_LIBFLAG)
iFolderTests_LIBS = System.dll System.Data.dll $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Agent.dll iFolder.dll CollectionStore.dll  AddressBook.dll 
iFolderTests_LIBPATH = $(srcdir)/../../tools/NUnit/bin $(SIMIAS_INSTALL_PATH)/bin

EXTRA_DIST = $(iFolder_CSFILES) $(iFolderCmd_CSFILES) $(iFolderTests_CSFILES)

CUR_DIR := $(shell pwd)

all: iFolder.dll iFolderCmd.exe

iFolder.dll: $(iFolder_CSFILES) $(iFolder_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolder_FLAGS) $(iFolder_LIBS:%=/r:%) $(iFolder_LIBPATH:%=/lib:%) $(iFolder_CSFILES_CSC) /doc:iFolder.doc.xml

iFolderCmd.exe: $(iFolderCmd_CSFILES) $(iFolderCmd_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderCmd_FLAGS) $(iFolderCmd_LIBS:%=/r:%) $(iFolderCmd_LIBPATH:%=/lib:%) $(iFolderCmd_CSFILES_CSC) /doc:iFolderCmd.doc.xml

iFolderTests.dll: $(iFolderTests_CSFILES) $(iFolderTests_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(iFolderTests_FLAGS) $(iFolderTests_LIBS:%=/r:%) $(iFolderTests_LIBPATH:%=/lib:%) $(iFolderTests_CSFILES_CSC)
	
install-exec-local: iFolder.dll iFolderCmd.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) iFolder.dll iFolderCmd.exe $(DESTDIR)$(bindir)
    
uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f iFolder.dll iFolderCmd.exe iFolderTests.test.xml $(NUNIT_TESTS)
    
installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
#	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:iFolderTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = iFolder.dll iFolderCmd.exe $(NUNIT_TESTS) iFolder.doc.xml iFolderTests.test.xml iFolderCmd.doc.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)
    
distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

