Sync_CSFILES = $(srcdir)/SyncPass.cs $(srcdir)/SyncOps.cs $(srcdir)/Dredge.cs $(srcdir)/SyncLog.cs
Sync_CSFILES_CSC := $(subst /,$(SEP),$(Sync_CSFILES))
Sync_INCLUDES =
Sync_RESOURCES =
Sync_FLAGS = $(CSC_LIBFLAG)
Sync_LIBS = Common.dll Configuration.dll CollectionEvents.dll StoreProvider.dll CollectionStore.dll Agent.dll SyncCommon.dll System.dll System.Data.dll System.Runtime.Remoting.dll
Sync_LIBPATH = ../Common ../SyncCommon ../Agent ../CollectionEvents ../CollectionStore  ../../external/SecureChannelSinks ../StoreProvider ../FsProvider ../Configuration ../SqliteProvider ../../external/sqlite/linux

SyncCmd_CSFILES = $(srcdir)/SyncCmd.cs
SyncCmd_CSFILES_CSC := $(subst /,$(SEP),$(SyncCmd_CSFILES))
SyncCmd_INCLUDES =
SyncCmd_RESOURCES =
SyncCmd_FLAGS = $(CSC_EXEFLAG)
SyncCmd_LIBS = $(Sync_LIBS) Sync.dll
SyncCmd_LIBPATH = $(Sync_LIBPATH)

NUNIT_TESTS := TestSync.exe

TestSync_CSFILES = $(srcdir)/TestSync.cs
TestSync_CSFILES_CSC := $(subst /,$(SEP),$(TestSync_CSFILES))
TestSync_INCLUDES =
TestSync_RESOURCES =
TestSync_FLAGS = $(CSC_LIBFLAG)
TestSync_LIBS = $(Sync_LIBS) $(SYSTEM_XML) nunit.util.dll nunit.framework.dll Sync.dll
TestSync_LIBPATH = $(Sync_LIBPATH) ../../tools/NUnit/bin $(srcdir)/../../tools/NUnit/bin

EXTRA_DIST = $(Sync_CSFILES) $(SyncCmd_CSFILES) $(TestSync_CSFILES)

CUR_DIR := $(shell pwd)

all: Sync.dll SyncCmd.exe

Sync.dll: $(Sync_CSFILES) $(Sync_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(Sync_FLAGS) $(Sync_LIBS:%=/r:%) $(Sync_LIBPATH:%=/lib:%) $(Sync_CSFILES_CSC) /doc:Sync.doc.xml

SyncCmd.exe: $(SyncCmd_CSFILES) $(SyncCmd_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(SyncCmd_FLAGS) $(SyncCmd_LIBS:%=/r:%) $(SyncCmd_LIBPATH:%=/lib:%) $(SyncCmd_CSFILES_CSC) /doc:SyncCmd.doc.xml

TestSync.exe: $(TestSync_CSFILES) $(TestSync_RESOURCES)
	$(CSC) /out:$@ $(CSCFLAGS) $(TestSync_FLAGS) $(TestSync_LIBS:%=/r:%) $(TestSync_LIBPATH:%=/lib:%) $(TestSync_CSFILES_CSC)
	
install-exec-local: Sync.dll SyncCmd.exe
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) Sync.dll SyncCmd.exe $(DESTDIR)$(bindir)

uninstall-local:
	cd $(DESTDIR)$(bindir); \
		rm -f Sync.dll SyncCmd.exe TestSync.doc.test.xml SyncTests.xml $(NUNIT_TESTS); \
		rm -rf tmpS tmpStoreC tmpStoreS

installcheck-local: install $(NUNIT_TESTS)
	$(mkinstalldirs) $(DESTDIR)$(bindir)
	$(INSTALL_PROGRAM) $(NUNIT_TESTS) $(DESTDIR)$(bindir)
	cd $(DESTDIR)$(bindir); $(MONO) $(CUR_DIR)/$(NUNIT) $(NUNIT_FLAGS) /xml:TestSync.doc.test.xml $(NUNIT_TESTS)
	
CLEAN_FILES = Sync.dll SyncCmd.exe TestSync.exe $(NUNIT_TESTS) Sync.doc.xml SyncCmd.doc.xml TestSync.doc.test.xml SyncTests.xml

clean-local:
	rm -rf *.dbg *.exe *.dll $(CLEAN_FILES) $(COMMON_CLEAN_FILES)

distclean-local: clean

maintainer-clean-local:
	rm -f Makefile.in

