# handle Mono secondary dependencies
export MONO_PATH := $(MONO_PATH)

RemotingMonitorLibrary_CSFILES = $(srcdir)/Library/AssemblyInfo.cs $(srcdir)/Library/InviteAgent.cs
RemotingMonitorLibrary_CSFILES_CSC := $(subst /,$(SEP),$(InviteAgent_CSFILES))
RemotingMonitorLibrary_INCLUDES =
RemotingMonitorLibrary_RESOURCES =
RemotingMonitorLibrary_FLAGS = $(CSC_LIBFLAG)
RemotingMonitorLibrary_LIBS = System.dll System.Data.dll $(SYSTEM_XML) System.Web.dll Configuration.dll StoreProvider.dll Common.dll CollectionEvents.dll CollectionStore.dll Agent.dll AddressBook.dll Common.dll SyncCommon.dll
RemotingMonitorLibrary_LIBPATH = Library ../CollectionStore ../StoreProvider ../Agent ../AddressBook ../Common ../SyncCommon ../Configuration ../Common ../CollectionEvents

RemotingMonitorApplication_CSFILES = $(srcdir)/ConsoleApp/AssemblyInfo.cs $(srcdir)/ConsoleApp/MainClass.cs
RemotingMonitorApplication_CSFILES_CSC := $(subst /,$(SEP),$(RemotingMonitorApplication_CSFILES))
RemotingMonitorApplication_INCLUDES =
RemotingMonitorApplication_RESOURCES = $(srcdir)/ConsoleApp/App$(ICON_EXT)
RemotingMonitorApplication_FLAGS = $(CSC_WINEXEFLAG)
RemotingMonitorApplication_LIBS = $(RemotingMonitorLibrary_LIBS) $(SYSTEM_XML) RemotingMonitorLibrary.dll
RemotingMonitorApplication_LIBPATH = $(RemotingMonitorLibrary_LIBPATH)

NUNIT_TESTS := 

EXTRA_DIST = $(RemotingMonitorLibrary_CSFILES) $(RemotingMonitorApplication_CSFILES) $(RemotingMonitorLibraryGui_CSFILES) $(EmailRemotingMonitorLibrary_CSFILES) $(RemotingMonitorLibraryTests_CSFILES) $(RemotingMonitorApplication_RESOURCES) $(RemotingMonitorLibraryGui_RESOURCES)

CUR_DIR := $(shell pwd)

all: RemotingMonitorLibrary.dll EmailRemotingMonitorLibrary.dll RemotingMonitorApplication.exe RemotingMonitorLibraryGui.exe

RemotingMonitorLibrary.dll: $(RemotingMonitorLibrary_CSFILES) $(RemotingMonitorLibrary_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(RemotingMonitorLibrary_FLAGS) $(RemotingMonitorLibrary_LIBS:%=/r:%) $(RemotingMonitorLibrary_LIBPATH:%=/lib:%) $(RemotingMonitorLibrary_CSFILES_CSC) /doc:RemotingMonitorLibrary.doc.xml

RemotingMonitorApplication.exe: $(RemotingMonitorApplication_CSFILES) $(RemotingMonitorApplication_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(RemotingMonitorApplication_FLAGS) $(RemotingMonitorApplication_LIBS:%=/r:%) $(RemotingMonitorApplication_LIBPATH:%=/lib:%) $(RemotingMonitorApplication_RESOURCES:%=$(ICON_FLAG)%) $(RemotingMonitorApplication_CSFILES_CSC) /doc:RemotingMonitorApplication.doc.xml

install-exec-local: RemotingMonitorLibrary.dll EmailRemotingMonitorLibrary.dll RemotingMonitorApplication.exe RemotingMonitorLibraryGui.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) RemotingMonitorLibrary.dll EmailRemotingMonitorLibrary.dll RemotingMonitorApplication.exe RemotingMonitorLibraryGui.exe $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); rm -f RemotingMonitorLibrary.dll EmailRemotingMonitorLibrary.dll RemotingMonitorApplication.exe RemotingMonitorLibraryGui.exe RemotingMonitorLibraryTests.test.xml $(NUNIT_TESTS)

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:RemotingMonitorLibraryTests.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = RemotingMonitorLibrary.dll EmailRemotingMonitorLibrary.dll RemotingMonitorApplication.exe RemotingMonitorLibraryGui.exe $(NUNIT_TESTS) RemotingMonitorLibrary.doc.xml EmailRemotingMonitorLibrary.doc.xml RemotingMonitorApplication.doc.xml RemotingMonitorLibraryGui.xml RemotingMonitorLibraryTests.test.xml
CLEAN_DIRS = .cstore

clean-local:
	rm -f *.dbg *.exe *.dll $(CLEAN_FILES)
	rm -rf $(CLEAN_DIRS)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

